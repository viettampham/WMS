using AutoMapper;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
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
        public LeaveRequestService(LienVietStorageContext context, IMapper mapper, ICurrentUserService currentUserService)
        {
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
                int currentStt = await _context.LeaveRequests.Where(x => x.CreateDate.Value.Year == CurrentYear).MaxAsync(x => x.Stt) ?? 0;

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
                newi.PendingAsigneeName = await _context.Users.Where(x => x.CodeUser == req.PendingAsigneeCode).Select(x => x.FullName).FirstOrDefaultAsync();
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
            catch (Exception ex)
            {
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
                        : req.Status == "PENDING"
                            ? new[] { "Phát hành" }
                            : new[] { "Đã xác nhận", "Đã phê duyệt", "Hoàn thành", "Phát hành" };
                    }
                    else if (tgu.RoleLevel <= 3)
                    {
                        int CheckCapGs = await _context.Users.Where(x => x.RoleLevel == 4 && x.PhongBan == tgu.PhongBan).CountAsync();
                        if (CheckCapGs == 0)
                        {
                            status = req.Status == "APPROVED"
                            ? new[] { "Đã phê duyệt", "Hoàn thành" }
                            : req.Status == "PENDING"
                                ? new[] { "Đã xác nhận", "Phát hành" }
                                : new[] { "Đã phê duyệt", "Hoàn thành", "Đã xác nhận" };
                        }
                        else {
                            status = req.Status == "APPROVED"
                            ? new[] { "Đã phê duyệt", "Hoàn thành" }
                            : req.Status == "PENDING"
                                ? new[] { "Đã xác nhận" }
                                : new[] { "Đã phê duyệt", "Hoàn thành", "Đã xác nhận" };
                        }
                    }
                    else
                    {
                        status = [];
                    }

                    IQueryable<LeaveRequest> query = _context.LeaveRequests.AsQueryable();

                    if (req.Status == "PENDING")
                    {
                        query = _context.LeaveRequests
                        .Where(x => (x.PendingAsigneeCode == req.CodePending)
                        && (status.Contains(x.Status) || status.Length == 0)).OrderByDescending(x => x.Id).AsQueryable();
                    }
                    else if (req.Status == "APPROVED")
                    {
                        query = _context.LeaveRequests
                        .Where(x =>
                            _context.LogApproves.Any(y =>
                                y.ApproverCode == req.CodePending &&
                                y.LeaveRequestId == x.Id)
                            &&
                            (status.Contains(x.Status) || status.Length == 0))
                        .OrderByDescending(x => x.Id);
                    }
                    //else {
                    //    query = _context.LeaveRequests
                    //    .Where(x => status.Contains(x.Status) || status.Length == 0)
                    //    .OrderByDescending(x => x.Id);
                    //}

                    var totalRecords = await query.CountAsync();
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
                else
                {
                    res.Message = "Không tồn tại người phê duyệt";
                    res.Status = "ERROR";
                    return res;
                }
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
            try
            {
                var query = _context.LeaveRequests
                    .Where(x => (x.Code.Contains(req.Code) || req.Code == "")
                    && (x.Status == req.Status || req.Status == "")
                    && (x.Username == req.Username || req.Username == "")).OrderByDescending(x => x.Id).AsQueryable();

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
            catch (Exception ex)
            {
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
                if (tar == null)
                {
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
                ds.Message = tar.Message;
                ds.MessageNotice = tar.MessageNotice;

                ds.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tar.Id).ToListAsync();

                res.Message = "Tìm thấy thông tin";
                res.Status = "SUCCESS";
                res.Data = ds;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<PendingAsignee>> GetPendingAsigneeByCurrentStep(string codeuser)
        {
            CommonResponseModel<PendingAsignee> res = new CommonResponseModel<PendingAsignee>();
            try
            {
                List<PendingAsignee> resu = new List<PendingAsignee>();
                User u = await _context.Users.Where(x => x.CodeUser == codeuser).FirstOrDefaultAsync();

                if (u.RoleLevel >= 4)
                {
                    resu = await _context.Users.Where(x => (x.RoleLevel == u.RoleLevel - 1) && x.PhongBan == u.PhongBan).Select(x => new PendingAsignee()
                    {
                        Code = x.CodeUser,
                        Name = x.FullName
                    }).ToListAsync();
                    if (resu.Count == 0) {
                        resu = await _context.Users.Where(x => (x.RoleLevel == u.RoleLevel - 2) && x.PhongBan == u.PhongBan).Select(x => new PendingAsignee()
                        {
                            Code = x.CodeUser,
                            Name = x.FullName
                        }).ToListAsync();
                    }
                }
                else if (u.RoleLevel == 3) {
                    resu = await _context.Users.Where(x => (x.RoleLevel == 3) && x.PhongBan == "Hành chính nhân sự").Select(x => new PendingAsignee()
                    {
                        Code = x.CodeUser,
                        Name = x.FullName
                    }).ToListAsync();
                }

                res.Message = "Thành công";
                res.Status = "SUCCESS";
                res.ListData = resu;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveType>> GetTypes()
        {
            CommonResponseModel<LeaveType> res = new CommonResponseModel<LeaveType>();
            try
            {
                List<LeaveType> lst = await _context.LeaveTypes.ToListAsync();
                res.Message = "Lấy dữ liệu thành công";
                res.Status = "SUCCESS";
                res.ListData = lst;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveResponse>> PhatHanhDon(PhatHanhDon req)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                LeaveRequest tget = await _context.LeaveRequests.Where(x => x.Code == req.CodeLeave).FirstOrDefaultAsync();
                if (tget == null)
                {
                    res.Message = "Không tồn tại đơn này";
                    res.Status = "ERROR";
                    return res;
                }

                if (tget.Status != "Đăng ký số")
                {
                    res.Message = "Phiếu đang không ở trạng thái đăng ký số";
                    res.Status = "ERROR";
                    return res;
                }

                User upl = await _context.Users.Where(x => x.CodeUser == req.CodeUserPhatHanh).FirstOrDefaultAsync();

                if (upl.RoleLevel == 5)
                {// nhân viên
                    tget.Status = "Phát hành";
                    tget.MessageNotice = "Phát hành đơn nghỉ phép thành công";
                    tget.Message = "Chờ cấp giám sát xác nhận";
                    tget.CurrentStep = 2;
                }
                else if (upl.RoleLevel == 4)
                { // cấp giám sát
                    tget.Status = "Đã xác nhận";
                    tget.MessageNotice = "Đã xác nhận cấp giám sát";
                    tget.Message = "Chờ cấp quản lý xác nhận";
                    tget.CurrentStep = 3;
                }
                else if (upl.RoleLevel == 3)
                {
                    tget.Status = "Đã phê duyệt";
                    tget.MessageNotice = "Hoàn thành";
                    tget.Message = "Đã xác nhận cấp quản lý";
                    tget.CurrentStep = 4;
                }

                await _context.SaveChangesAsync();

                List<LogApprove> lstLog = new List<LogApprove>();
                if (upl.RoleLevel == 5) {
                    LogApprove log = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = tget.UserCode,
                        ApproverName = tget.Fullname,
                        Action = "Phát hành",
                        Comment = "",
                        ActionTime = DateTime.Now
                    };
                    lstLog.Add(log);
                } else if (upl.RoleLevel == 4) {
                    LogApprove log = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = tget.UserCode,
                        ApproverName = tget.Fullname,
                        Action = "Phát hành",
                        Comment = "",
                        ActionTime = DateTime.Now
                    };

                    LogApprove log2 = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = tget.UserCode,
                        ApproverName = tget.Fullname,
                        Action = "Đã xác nhận",
                        Comment = "",
                        ActionTime = DateTime.Now
                    };
                    lstLog.Add(log);
                    lstLog.Add(log2);
                }
                else if (upl.RoleLevel == 3)
                {
                    LogApprove log = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = tget.UserCode,
                        ApproverName = tget.Fullname,
                        Action = "Phát hành",
                        Comment = "",
                        ActionTime = DateTime.Now
                    };

                    LogApprove log2 = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = tget.UserCode,
                        ApproverName = tget.Fullname,
                        Action = "Đã xác nhận",
                        Comment = "",
                        ActionTime = DateTime.Now
                    };
                    LogApprove log3 = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = tget.UserCode,
                        ApproverName = tget.Fullname,
                        Action = "Đã phê duyệt",
                        Comment = "",
                        ActionTime = DateTime.Now
                    };
                    lstLog.Add(log);
                    lstLog.Add(log2);
                    lstLog.Add(log3);
                }

                await _context.LogApproves.AddRangeAsync(lstLog);
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
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveResponse>> RejectLeave(RejectLeaveRequets req)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            try
            {
                LeaveRequest tget = await _context.LeaveRequests.Where(x => x.Code == req.CodeLeave).FirstOrDefaultAsync();
                if (tget == null)
                {
                    res.Message = "Không tồn tại thông tin đơn " + req.CodeLeave;
                    res.Status = "ERROR";
                    return res;
                }

                if (tget.Status != "Phát hành")
                {
                    res.Message = "Đơn đã phê duyệt hoặc chưa phát hành, kiểm tra lại";
                    res.Status = "WARNING";
                }

                tget.Status = "Từ chối";
                tget.CurrentStep = 1;
                tget.Message = "Đơn bị từ chối";
                tget.MessageNotice = "Đơn bị cấp giám sát từ chối";


                User userLeave = await _context.Users.Where(x => x.CodeUser == tget.UserCode).FirstOrDefaultAsync();
                //tget.PendingAsigneeCode = userLeave.CodeUser;
                //tget.PendingAsigneeName = userLeave.FullName;

                User userApprove = await _context.Users.Where(x => x.CodeUser == req.CodeReject).FirstOrDefaultAsync();

                LogApprove log = new LogApprove
                {
                    LeaveRequestId = tget.Id,
                    ApproverCode = userApprove.CodeUser,
                    ApproverName = userApprove.FullName,
                    Action = "Từ chối",
                    Comment = req.ReasonReject,
                    ActionTime = DateTime.Now
                };

                await _context.LogApproves.AddAsync(log);
                await _context.SaveChangesAsync();


                LeaveResponse resul = new LeaveResponse();
                resul = _mapper.Map<LeaveResponse>(tget);
                resul.PhongBan = userLeave.PhongBan;
                resul.ChucVu = userLeave.RoleName;

                resul.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tget.Id).ToListAsync();

                res.Message = "Từ chối thành công đơn nghỉ phép";
                res.Status = "SUCCESS";
                res.Data = resul;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveResponse>> XacNhanDon(XacNhanDonRequest req)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            try
            {
                LeaveRequest tget = await _context.LeaveRequests.Where(x => x.Code == req.CodeLeave).FirstOrDefaultAsync();
                if (tget == null)
                {
                    res.Message = "Không tồn tại mã phiếu " + req.CodeLeave;
                    res.Status = "ERROR";
                    return res;
                }

                if (tget.Status != "Phát hành")
                {
                    res.Message = "Phiếu nghỉ phép này đã được phê duyệt hoặc chưa phát hành";
                    res.Status = "WARNING";
                    return res;
                }

                tget.Status = "Đã xác nhận";
                tget.CurrentStep = 3;
                tget.MessageNotice = "Đã xác nhận thành công";
                tget.Message = "Chờ phê duyệt cấp quản lý";

                User approver = await _context.Users.Where(x => x.CodeUser == req.CodeApprover).FirstOrDefaultAsync();


                User manager = await _context.Users.Where(x => x.RoleLevel == 3 && x.PhongBan == approver.PhongBan).FirstOrDefaultAsync();
                tget.PendingAsigneeCode = manager.CodeUser;
                tget.PendingAsigneeName = manager.FullName;

                LogApprove log = new LogApprove
                {
                    LeaveRequestId = tget.Id,
                    ApproverCode = approver.CodeUser,
                    ApproverName = approver.FullName,
                    Action = "Xác nhận",
                    Comment = "",
                    ActionTime = DateTime.Now
                };

                await _context.LogApproves.AddAsync(log);
                await _context.SaveChangesAsync();



                LeaveResponse resul = new LeaveResponse();
                resul = _mapper.Map<LeaveResponse>(tget);
                User u = await _context.Users.Where(x => x.CodeUser == tget.UserCode).FirstOrDefaultAsync();
                resul.PhongBan = u.PhongBan;
                resul.ChucVu = u.RoleName;

                resul.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tget.Id).ToListAsync();

                res.Message = "Xác nhận thành công";
                res.Status = "SUCCESS";
                res.Data = resul;
                return res;

            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<LeaveResponse>> ApproveLeave(XacNhanDonRequest req)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            try
            {
                LeaveRequest tget = await _context.LeaveRequests.Where(x => x.Code == req.CodeLeave).FirstOrDefaultAsync();
                if (tget == null)
                {
                    res.Message = "Không tồn tại thông tin phiếu " + req.CodeLeave;
                    res.Status = "ERROR";
                    return res;
                }
                User approver = await _context.Users.Where(x => x.CodeUser == req.CodeApprover).FirstOrDefaultAsync();


                int CheckCapGS = await _context.Users.Where(x => x.PhongBan == approver.PhongBan && x.RoleLevel == 4).CountAsync();

                if (CheckCapGS != 0)
                {
                    if (tget.Status != "Đã xác nhận")
                    {
                        res.Message = "Phiếu đã được phê duyệt hoặc chưa được xác nhận cấp giám sát";
                        res.Status = "WARNING";
                        return res;
                    }
                }




                tget.Status = "Đã phê duyệt";
                tget.CurrentStep = 4;
                tget.Message = "Hoàn thành";
                tget.MessageNotice = "Đã phê duyệt cấp quản lý";
                tget.PendingAsigneeCode = "";
                tget.PendingAsigneeName = "";

                User userleave = await _context.Users.Where(x => x.CodeUser == tget.UserCode).FirstOrDefaultAsync();
                userleave.LeaveRequest += 1;

                List<LogApprove> lstLog = new List<LogApprove>();
                if (tget.Status == "Đã xác nhận")
                {
                    LogApprove log = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = approver.CodeUser,
                        ApproverName = approver.FullName,
                        Action = "Phê duyệt",
                        Comment = "Phê duyệt nghỉ phép",
                        ActionTime = DateTime.Now
                    };
                    lstLog.Add(log);
                }
                else {
                    LogApprove log1 = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = approver.CodeUser,
                        ApproverName = approver.FullName,
                        Action = "Xác nhận",
                        Comment = "Xác nhận nghỉ phép",
                        ActionTime = DateTime.Now
                    };
                    lstLog.Add(log1);

                    LogApprove log = new LogApprove
                    {
                        LeaveRequestId = tget.Id,
                        ApproverCode = approver.CodeUser,
                        ApproverName = approver.FullName,
                        Action = "Phê duyệt",
                        Comment = "Phê duyệt nghỉ phép",
                        ActionTime = DateTime.Now
                    };
                    lstLog.Add(log);
                }

                await _context.LogApproves.AddRangeAsync(lstLog);
                await _context.SaveChangesAsync();

                LeaveResponse resul = new LeaveResponse();
                resul = _mapper.Map<LeaveResponse>(tget);
                User u = await _context.Users.Where(x => x.CodeUser == tget.UserCode).FirstOrDefaultAsync();
                resul.PhongBan = u.PhongBan;
                resul.ChucVu = u.RoleName;

                resul.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tget.Id).ToListAsync();


                res.Message = "Phê duyệt thành công đơn xin nghỉ phép";
                res.Status = "SUCCESS";
                res.Data = resul;
                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<ReportLeave>> ReportLeave(GetReportLeave req)
        {
            CommonResponseModel<ReportLeave> res = new CommonResponseModel<ReportLeave>();
            try
            {
                DateTime frDate = Convert.ToDateTime(req.FromDate).Date;
                DateTime toDate = Convert.ToDateTime(req.ToDate)
                .Date
                .AddDays(1)
                .AddTicks(-1);

                List<LeaveRequest> lst = await _context.LeaveRequests.Where(x => x.FromDate >= frDate && x.FromDate <= toDate).ToListAsync();
                ReportLeave rsult = new ReportLeave();
                rsult.TotalLeaves = lst.Where(x=>x.CurrentStep >= 2).Count();
                rsult.LeaveApproveds = lst.Where(x => x.Status == "Đã phê duyệt").Count();
                rsult.LeavePendings = lst.Where(x => x.Status == "Phát hành" || x.Status == "Đã xác nhận").Count();
                rsult.LeaveRefuseds = lst.Where(x => x.Status == "Từ chối").Count();


                var totalEmployeesByDept = await _context.Users
                .GroupBy(x => x.PhongBan)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.Count()
                );


                var userDepartments = await _context.Users
                .ToDictionaryAsync(x => x.CodeUser, x => x.PhongBan);

                List<ReportByDepart> rpDepartment = lst
                    .Select(x => new
                    {
                        x.UserCode,
                        PhongBan = userDepartments.GetValueOrDefault(x.UserCode, "Khác"),
                        x.TotalDays,
                        x.LeaveType
                    })
                    .GroupBy(x => x.PhongBan)
                    .Select(g => new ReportByDepart
                    {
                        PhongBan = g.Key,
                        TotalEmpl = totalEmployeesByDept.GetValueOrDefault(g.Key, 0),
                        TotalLeave = g.Count(),
                        TotalDayLeave = g.Sum(x => x.TotalDays),
                        AnnualLeave = g.Where(x => x.LeaveType == "Nghỉ phép")
                                       .Sum(x => x.TotalDays),
                        UnpaidLeave = g.Where(x => x.LeaveType == "Nghỉ không lương")
                                       .Sum(x => x.TotalDays),
                        Percentleave = totalEmployeesByDept.GetValueOrDefault(g.Key, 0) == 0
                        ? 0
                        : g.Select(x => x.UserCode)
                           .Distinct()
                           .Count() * 100.0
                           / totalEmployeesByDept.GetValueOrDefault(g.Key, 0)
                    }).ToList();

                rsult.ReportByDeparts = rpDepartment;

                var users = await _context.Users
                .ToDictionaryAsync(
                    x => x.CodeUser,
                    x => new
                    {
                        x.FullName,
                        x.PhongBan
                    });

                List<TopLeaveEmpl> topLeaveEmpls = lst
                .GroupBy(x => x.UserCode)
                .Select(g =>
                {
                    var user = users.GetValueOrDefault(g.Key);

                    return new TopLeaveEmpl
                    {
                        CodeEmpl = g.Key,
                        FullName = user?.FullName ?? "",
                        PhongBan = user?.PhongBan ?? "",
                        TotalLeaves = g.Count(),
                        TotalLeaveDays = g.Sum(x => x.TotalDays)
                    };
                })
                .OrderByDescending(x => x.TotalLeaveDays)
                .Take(10)
                .ToList();

                rsult.TopLeaveEmpls = topLeaveEmpls;


                res.Message = "Lấy báo cáo nghỉ phép thành công";
                res.Status = "SUCCESS";
                res.Data = rsult;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<byte[]> ExportReportLeave(GetReportLeave req)
        {
            DateTime fromDate = Convert.ToDateTime(req.FromDate).Date;
            DateTime toDate = Convert.ToDateTime(req.ToDate)
                .Date
                .AddDays(1)
                .AddTicks(-1);

            List<LeaveRequest> data = await _context.LeaveRequests
                .Where(x => x.FromDate >= fromDate
                         && x.FromDate <= toDate)
                .OrderByDescending(x => x.CreateDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("BaoCaoNghiPhep");

            // ==========================
            // Tiêu đề báo cáo
            // ==========================
            var titleRange = ws.Range("A1:L1");
            titleRange.Merge();

            titleRange.Value = "BÁO CÁO NGHỈ PHÉP NHÂN SỰ";

            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 18;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E40AF");
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Row(1).Height = 28;

            // ==========================
            // Thông tin báo cáo
            // ==========================
            ws.Cell("A3").Value = $"Từ ngày";
            ws.Cell("A4").Value = $"{fromDate:dd/MM/yyyy}";
            ws.Cell("C3").Value = $"Đến ngày";
            ws.Cell("C4").Value = $"{toDate:dd/MM/yyyy HH:mm}";

            ws.Cell("E3").Value = $"Thời điểm xuất";
            ws.Cell("E4").Value = $"{DateTime.Now:dd/MM/yyyy HH:mm}";

            ws.Range("A3:L3").Style.Font.Bold = true;

            // ==========================
            // Thống kê nhanh
            // ==========================
            ws.Cell("G3").Value = "Tổng số đơn";
            ws.Cell("G4").Value = data.Count;

            ws.Cell("I3").Value = "Tổng số ngày nghỉ:";
            ws.Cell("I4").Value = data.Sum(x => x.TotalDays);

            ws.Range("A4:L4").Style.Font.Bold = true;
            //ws.Range("A4:L4").Style.Fill.BackgroundColor =
            //    XLColor.FromHtml("#F3F4F6");

            //ws.Range("A5:L5").Style.Font.Bold = true;
            //ws.Range("A5:L5").Style.Fill.BackgroundColor =
            //    XLColor.FromHtml("#F3F4F6");

            // ==========================
            // Header bảng
            // ==========================
            int headerRow = 7;

            ws.Cell(headerRow, 1).Value = "Mã NV";
            ws.Cell(headerRow, 2).Value = "Họ tên";
            ws.Cell(headerRow, 3).Value = "Loại nghỉ";
            ws.Cell(headerRow, 4).Value = "Hình thức nghỉ";
            ws.Cell(headerRow, 5).Value = "Từ ngày";
            ws.Cell(headerRow, 6).Value = "Đến ngày";
            ws.Cell(headerRow, 7).Value = "Số ngày";
            ws.Cell(headerRow, 8).Value = "Lý do";
            ws.Cell(headerRow, 9).Value = "Trạng thái";
            ws.Cell(headerRow, 10).Value = "Thông tin";
            ws.Cell(headerRow, 11).Value = "Pending";
            ws.Cell(headerRow, 12).Value = "Ngày đăng ký";

            var headerRange = ws.Range($"A{headerRow}:L{headerRow}");

            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor =
                XLColor.FromHtml("#2563EB");

            headerRange.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ==========================
            // Dữ liệu
            // ==========================
            int row = headerRow + 1;

            foreach (var item in data)
            {
                ws.Cell(row, 1).Value = item.UserCode;
                ws.Cell(row, 2).Value = item.Fullname;
                ws.Cell(row, 3).Value = item.LeaveType;
                ws.Cell(row, 4).Value = item.TypeShift;
                ws.Cell(row, 5).Value = item.FromDate;
                ws.Cell(row, 6).Value = item.ToDate;
                ws.Cell(row, 7).Value = item.TotalDays;
                ws.Cell(row, 8).Value = item.Reason;
                ws.Cell(row, 9).Value = item.Status;
                ws.Cell(row, 10).Value = item.Message;
                ws.Cell(row, 11).Value = item.PendingAsigneeName;
                ws.Cell(row, 12).Value = item.CreateDate;

                ws.Cell(row, 5).Style.DateFormat.Format =
                    "dd/MM/yyyy HH:mm";
                ws.Cell(row, 6).Style.DateFormat.Format =
                    "dd/MM/yyyy HH:mm";
                ws.Cell(row, 12).Style.DateFormat.Format =
                    "dd/MM/yyyy HH:mm";


                // Tô màu trạng thái
                var statusCell = ws.Cell(row, 9);

                if (item.Status == "Đã phê duyệt" ||
                    item.Status == "Hoàn thành")
                {
                    statusCell.Style.Font.FontColor = XLColor.Green;
                    statusCell.Style.Font.Bold = true;
                }
                else if (item.Status == "Từ chối")
                {
                    statusCell.Style.Font.FontColor = XLColor.Red;
                    statusCell.Style.Font.Bold = true;
                }
                else
                {
                    statusCell.Style.Font.FontColor = XLColor.Orange;
                    statusCell.Style.Font.Bold = true;
                }

                // Màu xen kẽ
                if (row % 2 == 0)
                {
                    ws.Range(row, 1, row, 6)
                        .Style.Fill.BackgroundColor =
                        XLColor.FromHtml("#F8FAFC");
                }

                row++;
            }

            // ==========================
            // Border
            // ==========================
            var tableRange = ws.Range($"A{headerRow}:L{row - 1}");

            tableRange.Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            tableRange.Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            // ==========================
            // Filter
            // ==========================
            tableRange.SetAutoFilter();

            // ==========================
            // Freeze Header
            // ==========================
            ws.SheetView.FreezeRows(headerRow);

            // ==========================
            // Căn lề
            // ==========================
            ws.Column(1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Column(4).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            ws.Column(5).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            // ==========================
            // Auto Width
            // ==========================
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public async Task<CommonResponseModel<LeaveResponse>> ManagerRejectLeave(RejectLeaveRequets req)
        {
            CommonResponseModel<LeaveResponse> res = new CommonResponseModel<LeaveResponse>();
            try
            {
                LeaveRequest tget = await _context.LeaveRequests.Where(x => x.Code == req.CodeLeave).FirstOrDefaultAsync();
                if (tget == null)
                {
                    res.Message = "Không tồn tại thông tin đơn " + req.CodeLeave;
                    res.Status = "ERROR";
                    return res;
                }

                if (tget.Status != "Đã xác nhận")
                {
                    res.Message = "Đơn chưa được xác nhận hoặc đã phê duyệt thành công.";
                    res.Status = "WARNING";
                }

                tget.Status = "Từ chối";
                tget.CurrentStep = 1;
                tget.Message = "Đơn bị từ chối";
                tget.MessageNotice = "Đơn bị cấp quản lý từ chối";


                User userLeave = await _context.Users.Where(x => x.CodeUser == tget.UserCode).FirstOrDefaultAsync();
                //tget.PendingAsigneeCode = userLeave.CodeUser;
                //tget.PendingAsigneeName = userLeave.FullName;

                User userApprove = await _context.Users.Where(x => x.CodeUser == req.CodeReject).FirstOrDefaultAsync();

                LogApprove log = new LogApprove
                {
                    LeaveRequestId = tget.Id,
                    ApproverCode = userApprove.CodeUser,
                    ApproverName = userApprove.FullName,
                    Action = "Từ chối",
                    Comment = req.ReasonReject,
                    ActionTime = DateTime.Now
                };

                await _context.LogApproves.AddAsync(log);
                await _context.SaveChangesAsync();


                LeaveResponse resul = new LeaveResponse();
                resul = _mapper.Map<LeaveResponse>(tget);
                resul.PhongBan = userLeave.PhongBan;
                resul.ChucVu = userLeave.RoleName;

                resul.LstLog = await _context.LogApproves.Where(x => x.LeaveRequestId == tget.Id).ToListAsync();

                res.Message = "Từ chối thành công đơn nghỉ phép";
                res.Status = "SUCCESS";
                res.Data = resul;

                return res;
            }
            catch (Exception ex)
            {
                res.Message = "Lỗi " + ex.Message;
                res.Status = "ERROR";
                return res;
            }
        }

        public async Task<CommonResponseModel<InfoLeaveUser>> GetInfoLeaveUser(string codeuser)
        {
            CommonResponseModel<InfoLeaveUser> res = new CommonResponseModel<InfoLeaveUser>();
            try {
                User u = await _context.Users.Where(x => x.CodeUser == codeuser).FirstOrDefaultAsync();
                if (u == null) {
                    res.Message = "Không tồn tại thông tin người dùng " + codeuser;
                    res.Status = "ERROR";
                    return res;
                }
                InfoLeaveUser resul = new InfoLeaveUser() { 
                    AnualLeave = u.AnualLeave,
                    LeaveUsed = u.LeaveRequest,
                    LeaveRemaining = u.AnualLeave - u.LeaveRequest
                };


                res.Message = "Thành công";
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
