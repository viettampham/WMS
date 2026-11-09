using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WMS.Models;
using WMS.Models.Request.LeaveRequest;
using WMS.Models.Response.Common;
using WMS.Models.Response.LeaveRequest;
using WMS.Models.Response.User;
using WMS.Services.impl;

namespace WMS.Services.Service
{
    public class LeaveRequestService : ILeaveRequest
    {
        private readonly LienVietStorageContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public LeaveRequestService(LienVietStorageContext context, IMapper mapper, ICurrentUserService currentUserService) { 
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }
        public Task<CommonResponseModel<string>> CancelLeaveRequest(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<CommonResponseModel<LeaveRequest>> CreateleaveRequest(CreateLeaveRequest req)
        {
            CommonResponseModel<LeaveRequest> res = new CommonResponseModel<LeaveRequest>();
            try
            {
                int CurrentYear = DateTime.Now.Year;
                int currentStt = await _context.LeaveRequests.Where(x=>x.CreateDate.Value.Year == CurrentYear).MaxAsync(x => x.Stt) ?? 0;

                int stt = currentStt + 1;

                LeaveRequest newi = new LeaveRequest();
                newi.Code = "LR" + CurrentYear.ToString() + stt.ToString();
                newi.UserCode = req.CodeUser;
                newi.Username = req.UserName;
                newi.Fullname = req.FullName;
                newi.LeaveType = req.LeaveType;
                newi.TypeShift = req.TypeShift;
                newi.FromDate = req.FromDate;
                newi.ToDate = req.ToDate;
                newi.TotalDays = req.TotalDay;
                newi.Reason = req.Reason;
                newi.Status = req.Status;
                newi.CurrentStep = 1;
                newi.CreateDate = DateTime.Now;
                newi.CreateBy = _currentUserService.UserName;
                newi.Stt = stt;

                await _context.LeaveRequests.AddAsync(newi);
                await _context.SaveChangesAsync();

                res.Message = "Đăng ký số thành công đơn xin nghỉ phép";
                res.Status = "SUCCESS";
                res.Data = newi;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<PagingResponse<LeaveResponse>>> GetLeave(GetLeaveRequest req)
        {
            CommonResponseModel<PagingResponse<LeaveResponse>> res = new CommonResponseModel<PagingResponse<LeaveResponse>>();
            try {
                var query = _context.LeaveRequests
                    .Where(x=>(x.Code.Contains(req.Code) || req.Code == "") 
                    && (x.Status == req.Status || req.Status == "") 
                    && (x.Username == req.Username || req.Username == "")).OrderByDescending(x=>x.Id).AsQueryable();
                
                var totalRecords = query.Count();

                var leave = await query
                .Skip((req.PageIndex - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var data = _mapper.Map<List<LeaveResponse>>(leave);

                var paging = new PagingResponse<LeaveResponse>
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

        public async Task<CommonResponseModel<LeaveResponse>> GetLeaveByCode(string Code)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            try
            {
                LeaveRequest tar = await _context.LeaveRequests.Where(x => x.Code == Code).FirstOrDefaultAsync();
                if (tar == null) {
                    res.Message = "Không tồn tại thông tin";
                    res.Status = "ERROR";
                    return res;
                }

                User u = await _context.Users.Where(x => x.CodeUser == tar.UserCode).FirstOrDefaultAsync();

                LeaveResponse ds = new LeaveResponse();
                ds.Id = tar.Id;
                ds.Code = tar.Code;
                ds.UserCode = tar.UserCode;
                ds.UserName = tar.Username;
                ds.LeaveType = tar.LeaveType;
                ds.FromDate = tar.FromDate;
                ds.ToDate = tar.ToDate;
                ds.TotalDay = tar.TotalDays;
                ds.Reason = tar.Reason;
                ds.Status = tar.Status;
                ds.CurentStep = tar.CurrentStep;
                ds.CreateDate = tar.CreateDate;
                ds.CreateBy = tar.CreateBy;
                ds.UpdateDate = tar.UpdateDate;
                ds.UpdateBy = tar.UpdateBy;
                ds.Stt = tar.Stt;
                ds.TypeShift = tar.TypeShift;
                ds.Fullname = tar.Fullname;
                ds.PhongBan = u.PhongBan;
                ds.ChucVu = u.RoleName;

                res.Message = "Tìm thấy thông tin";
                res.Status = "SUCCESS";
                res.Data = ds;
                return res;
            }
            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveType>> GetTypes()
        {
            CommonResponseModel<LeaveType> res = new CommonResponseModel<LeaveType>();
            try {
                List<LeaveType> lst = await _context.LeaveTypes.ToListAsync();
                res.Message = "Lấy dữ liệu thành công";
                res.Status = "SUCCESS";
                res.ListData = lst;
                return res;
            }
            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveRequest>> PhatHanhDon(string Code)
        {
            CommonResponseModel<LeaveRequest> res = new CommonResponseModel<LeaveRequest>();
            try {
                LeaveRequest tget = await _context.LeaveRequests.Where(x => x.Code == Code).FirstOrDefaultAsync();
                if (tget == null) {
                    res.Message = "Không tồn tại đơn này";
                    res.Status = "ERROR";
                    return res;
                }

                if (tget.Status != "Đăng ký số") {
                    res.Message = "Phiếu đang không ở trạng thái đăng ký số";
                    res.Status = "ERROR";
                    return res;
                }

                tget.Status = "Phát hành";
                tget.CurrentStep = 2;

                await _context.SaveChangesAsync();
                res.Message = "Phát hành đơn thành công";
                res.Status = "SUCCESS";
                res.Data = tget;
                return res;
            }
            catch (Exception ex) {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }
    }
}
