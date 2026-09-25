using AutoMapper;
using Azure.Core;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WMS.Models;
using WMS.Models.Request.User;
using WMS.Models.Response.Common;
using WMS.Models.Response.User;
using WMS.Services.impl;


namespace WMS.Services.Service
{
    public class UserService : IUserService
    {
        private readonly IUserService _userService;
        private readonly LienVietStorageContext _dbcontext;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUserService _currentUserService;

        private readonly IMapper _mapper;
        public UserService(LienVietStorageContext context, IMapper mapper, IConfiguration configuration, ICurrentUserService currentUserService) { 
            _dbcontext = context;
            _mapper = mapper;
            _configuration = configuration;
            _currentUserService = currentUserService;
        }

        public async Task<CommonResponseModel<string>> AddUser(AddUserRequest request)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try {
                User targetU = _dbcontext.Users.Where(x => x.Cccd == request.CCCD).FirstOrDefault();
                if (targetU != null) {
                    res.Message = "Đã tồn tại account của người này, vui lòng kiểm tra lại";
                    res.Status = "ERROR";
                    return res;
                }

                User nu = new User();
                nu.FullName = request.FullName;
                nu.UserName = request.Username;
                nu.PasswordHash = HashPassword(request.Password);
                nu.Email = request.Email;
                nu.PhoneNumber = request.PhoneNumber;
                nu.Address = request.Address;
                nu.Avatar = request.Avatar;
                nu.DateOfBirth = request.DateOfBirth;
                nu.Status = "Active";
                nu.FailedLoginAttempts = 0;
                nu.RoleLevel = request.RoleLevel;
                nu.CreateAt = DateTime.Now;
                nu.Cccd = request.CCCD;
                nu.Gender = request.Gender;
                nu.PhongBan = request.PhongBan;

                _dbcontext.Users.Add(nu);
                _dbcontext.SaveChanges();

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                return res;
            }
            catch (Exception ex) { 
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<PagingResponse<UserResponseModal>>> SearchUser(GetUserRequest req)
        {
            CommonResponseModel<PagingResponse<UserResponseModal>> res = new CommonResponseModel<PagingResponse<UserResponseModal>>();
            PagingResponse<UserResponseModal> pres = new PagingResponse<UserResponseModal>();
            try
            {
                var query = _dbcontext.Users.Where(x => (x.UserName.Contains(req.UserName) || req.UserName == "")
                && (x.FullName.Contains(req.FullName)|| req.FullName== "")
                && (x.Address.Contains(req.Address) || req.Address == "")
                && (x.Email.Contains(req.Email) || req.Email == "")
                && (x.PhoneNumber.Contains(req.PhoneNumber) || req.PhoneNumber == "")
                && (x.Gender == req.Gender || req.Gender == "")
                && (x.Status == req.Status || req.Status == "")
                && x.Status == "Active").AsQueryable();

                var totalRecords = query.Count();

                var users = await query
                .Skip((req.PageIndex - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var data = _mapper.Map<List<UserResponseModal>>(users);

                var paging = new PagingResponse<UserResponseModal>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = req.PageIndex,
                    PageSize = req.PageSize
                };

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.Data = paging;

                return res;
            }

            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        public async Task<CommonResponseModel<string>> Authentication(AuthenRequest request)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try
            {
                if (String.IsNullOrEmpty(request.UserName) || String.IsNullOrEmpty(request.Password))
                {
                    res.Status = "WARNING";
                    res.Message = "Vui lòng nhập thông tin Code và Pass";
                    return res;
                }

                User tnv = _dbcontext.Users.Where(x => x.UserName == request.UserName && x.Status == "Active").FirstOrDefault();
                if (tnv != null)
                {
                    bool verifyPass = VerifyPassword(request.Password, tnv.PasswordHash);
                    if (verifyPass)
                    {
                        res.Status = "SUCCESS";
                        res.Message = "Thành công";

                        string token = GenToken(tnv);

                        res.Data = token;
                        return res;
                    }
                    else
                    {
                        res.Status = "ERROR";
                        res.Message = "Sai mật khẩu";
                        return res;
                    }
                }
                else
                {
                    res.Status = "WARNING";
                    res.Message = "Sai code hoặc pass hoặc tài khoản đã bị khoá";
                    return res;
                }

            }
            catch (Exception ex)
            {
                res.Status = "ERROR";
                res.Message = "Lỗi đăng nhập: " + ex.Message;
                return res;
            }
        }
        public string GenToken(User nhanvien)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])
            );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            string roleName = _dbcontext.Roles.Where(x => x.RoleLevel == nhanvien.RoleLevel).FirstOrDefault().RoleName;

            var claims = new[]
            {
                new Claim("UserName", nhanvien.UserName ?? ""),
                new Claim("UserId", nhanvien.Id.ToString()),
                new Claim("Fullname", nhanvien.FullName?? ""),
                new Claim("RoleLevel", nhanvien.RoleLevel.ToString()?? ""),
                new Claim(ClaimTypes.Role, roleName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<CommonResponseModel<string>> UpdateUser(UpdateUserRequest request)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try {
                User targetU = _dbcontext.Users.Where(x => x.Id == request.ID).FirstOrDefault();
                if (targetU == null) {
                    res.Message = "Không tìm được thông tin người dùng này";
                    res.Status = "ERROR";
                    return res;
                }

                targetU.FullName = request.FullName;
                targetU.UserName = request.Username;
                targetU.Email = request.Email;
                targetU.PhoneNumber = request.PhoneNumber;
                targetU.Address = request.Address;
                targetU.Avatar = request.Avatar;
                targetU.DateOfBirth = request.DateOfBirth;
                targetU.Gender = request.Gender;
                targetU.Cccd = request.CCCD;

                targetU.EditAt = DateTime.Now;
                targetU.EditBy = _currentUserService.UserName;

                _dbcontext.SaveChanges();

                res.Message = "Cập nhật thông tin người dùng thành công";
                res.Status = "SUCCESS";
                return res;
            }
            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<string>> DeleteUser(int id)
        {
            CommonResponseModel<string> res = new CommonResponseModel<string>();
            try
            {
                User targetU = _dbcontext.Users.Where(x => x.Id == id).FirstOrDefault();
                if (targetU == null)
                {
                    res.Message = "Không tìm được thông tin người dùng này";
                    res.Status = "ERROR";
                    return res;
                }

                targetU.Status = "Delete";
                targetU.DeleteBy = _currentUserService.UserName;
                targetU.DeleteAt = DateTime.Now;
                _dbcontext.SaveChanges();

                res.Message = "Xóa thành công thông tin người dùng";
                res.Status = "SUCCESS";
                return res;
            } catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }
    }

}
