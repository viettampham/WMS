using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
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

        public async Task<CommonResponseModel<LeaveResponse>> CreateleaveRequest(CreateLeaveRequest req)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            using var transaction = await _context.Database.BeginTransactionAsync();
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
                newi.Message = "Phát hành đơn để cấp trên phê duyệt.";
                newi.MessageNotice = "Đăng ký đơn nghỉ phép thành công";
                newi.PendingAsigneeName = await _context.Users.Where(x=>x.CodeUser == req.PendingAsigneeCode).Select(x=>x.FullName).FirstOrDefaultAsync();
                newi.PendingAsigneeCode = req.PendingAsigneeCode;

                await _context.LeaveRequests.AddAsync(newi);
                await _context.SaveChangesAsync();

                LogApprove log = new LogApprove
                {
                    LeaveRequestId = newi.Id,
                    ApproverCode = req.CodeUser,
                    ApproverName = req.FullName,
                    Action = "Đăng ký số",
                    Comment = "",
                    ActionTime = DateTime.Now
                };

                await _context.LogApproves.AddAsync(log);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                LeaveResponse resul = new LeaveResponse();

                resul = _mapper.Map<LeaveResponse>(newi);

                resul.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == newi.Id).ToListAsync();

                res.Message = "Đăng ký số thành công đơn xin nghỉ phép";
                res.Status = "SUCCESS";
                res.Data = resul;
                return res;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<PagingResponse<LeaveRequest>>> GetDSLeave(GetpaingLeaveRequets req)
        {
            CommonResponseModel<PagingResponse<LeaveRequest>> res = new CommonResponseModel<PagingResponse<LeaveRequest>>();
            try
            {
                DateTime fromDate = Convert.ToDateTime(req.FromDate).Date;

                DateTime toDate = Convert.ToDateTime(req.ToDate)
                    .Date
                    .AddDays(1)
                    .AddTicks(-1);

                var status = req.StatusDone == "DONE"
                    ? new[] { "Đã phê duyệt", "Hoàn thành" }
                    : new[] { "Đăng ký số", "Phát hành", "Đã xác nhận" };

                var query = _context.LeaveRequests
                .Where(x =>
                    (string.IsNullOrEmpty(req.CodeUser) || x.UserCode == req.CodeUser) &&
                    x.FromDate >= fromDate &&
                    x.FromDate <= toDate &&
                    (req.StatusDone == "ALL" || status.Contains(x.Status))
                )
                .OrderByDescending(x => x.Id);


                var totalRecords = query.Count();

                var leaves = await query
                .Skip((req.PageIndex - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

                var data = _mapper.Map<List<LeaveRequest>>(leaves);

                var paging = new PagingResponse<LeaveRequest>
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

        public async Task<CommonResponseModel<PagingResponse<LeaveResponse>>> GetDsPendingApproveByCodeUser(GetpaingLeaveApprovePending req)
        {
            CommonResponseModel<PagingResponse<LeaveResponse>> res = new CommonResponseModel<PagingResponse<LeaveResponse>>();
            try
            {
                User tgu = await _context.Users.Where(x => x.CodeUser == req.CodePending).FirstOrDefaultAsync();
                string[] status;
                if (tgu != null)
                {
                    if (tgu.RoleLevel == 4)
                    {
                        status = req.Status == "APPROVED"
                            ? new[] { "Đã xác nhận", "Đã phê duyệt", "Hoàn thành" }
                            : new[] { "Phát hành" };
                    }
                    else if (tgu.RoleLevel <= 3)
                    {
                        status = req.Status == "APPROVED"
                                ? new[] { "Đã phê duyệt", "Hoàn thành" }
                                : new[] { "Đã xác nhận" };
                    }
                    else
                    {
                        status = [];
                    }

                    var query = _context.LeaveRequests
                        .Where(x => (x.PendingAsigneeCode == req.CodePending)
                        && (status.Contains(x.Status))).OrderByDescending(x => x.Id).AsQueryable();

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
                else {
                    res.Message = "Không tồn tại người phê duyệt";
                    res.Status = "ERROR";
                    return res;
                }
            }
            catch (Exception ex) {
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
                ds.TotalDays = tar.TotalDays;
                ds.Reason = tar.Reason;
                ds.Status = tar.Status;
                ds.CurrentStep = tar.CurrentStep;
                ds.CreateDate = tar.CreateDate;
                ds.CreateBy = tar.CreateBy;
                ds.UpdateDate = tar.UpdateDate;
                ds.UpdateBy = tar.UpdateBy;
                ds.Stt = tar.Stt;
                ds.TypeShift = tar.TypeShift;
                ds.Fullname = tar.Fullname;
                ds.PhongBan = u.PhongBan;
                ds.ChucVu = u.RoleName;

                ds.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tar.Id).ToListAsync();

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

        public async Task<CommonResponseModel<PendingAsignee>> GetPendingAsigneeByCurrentStep(int CurrentStep)
        {
            CommonResponseModel<PendingAsignee> res = new CommonResponseModel<PendingAsignee>();
            try {
                List<PendingAsignee> resu = new List<PendingAsignee>();
                if (CurrentStep == 0 || CurrentStep == 1) // chưa đăng ký số || đăng ký số
                {
                    resu = await _context.Users.Where(x => x.RoleLevel == 4).Select(x=> new PendingAsignee() { 
                        Code = x.CodeUser,
                        Name = x.FullName
                    }).ToListAsync(); // cấp giám sát
                }
                else if (CurrentStep == 2)  // đã phát hành
                {
                    resu = await _context.Users.Where(x => x.RoleLevel == 3).Select(x => new PendingAsignee()
                    {
                        Code = x.CodeUser,
                        Name = x.FullName
                    }).ToListAsync(); // cấp quản lý
                }

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.ListData = resu;
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

        public async Task<CommonResponseModel<LeaveResponse>> PhatHanhDon(string Code)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
                tget.MessageNotice = "Phát hành đơn nghỉ phép thành công";
                tget.Message = "Chờ cấp giám sát xác nhận";
                tget.CurrentStep = 2;

                await _context.SaveChangesAsync();

                LogApprove log = new LogApprove
                {
                    LeaveRequestId = tget.Id,
                    ApproverCode = tget.UserCode,
                    ApproverName = tget.Fullname,
                    Action = "Phát hành",
                    Comment = "",
                    ActionTime = DateTime.Now
                };

                await _context.LogApproves.AddAsync(log);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                LeaveResponse resul = new LeaveResponse();

                resul = _mapper.Map<LeaveResponse>(tget);
                User u = await _context.Users.Where(x => x.CodeUser == tget.UserCode).FirstOrDefaultAsync();
                resul.PhongBan = u.PhongBan;
                resul.ChucVu = u.RoleName;

                resul.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tget.Id).ToListAsync();

                res.Message = "Phát hành đơn thành công";
                res.Status = "SUCCESS";
                res.Data = resul;
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
