using WMS.Models;
using WMS.Models.Request.LeaveRequest;
using WMS.Models.Response.Common;
using WMS.Models.Response.LeaveRequest;
using WMS.Models.Response.User;

namespace WMS.Services.impl
{
    public interface ILeaveRequest
    {
        Task<CommonResponseModel<LeaveResponse>> CreateleaveRequest(CreateLeaveRequest req);
        Task<CommonResponseModel<PagingResponse<LeaveResponse>>> GetLeave(GetLeaveRequest req);

        Task<CommonResponseModel<string>> CancelLeaveRequest(int id);

        Task<CommonResponseModel<LeaveType>> GetTypes();
        Task<CommonResponseModel<LeaveResponse>> GetLeaveByCode(string Code);

        Task<CommonResponseModel<LeaveResponse>> PhatHanhDon(PhatHanhDon req);

        Task<CommonResponseModel<PendingAsignee>> GetPendingAsigneeByCurrentStep(string codeuser);

        Task<CommonResponseModel<PagingResponse<LeaveRequest>>> GetDSLeave(GetpaingLeaveRequets req);

        Task<CommonResponseModel<PagingResponse<LeaveResponse>>> GetDsPendingApproveByCodeUser(GetpaingLeaveApprovePending req);

        Task<CommonResponseModel<LeaveResponse>> XacNhanDon(XacNhanDonRequest req);

        Task<CommonResponseModel<LeaveResponse>> RejectLeave(RejectLeaveRequets req);
        Task<CommonResponseModel<LeaveResponse>> ApproveLeave(XacNhanDonRequest req);

        Task<CommonResponseModel<ReportLeave>> ReportLeave(GetReportLeave req);

        Task<byte[]> ExportReportLeave(GetReportLeave req);

        Task<CommonResponseModel<LeaveResponse>> ManagerRejectLeave(RejectLeaveRequets req);

        Task<CommonResponseModel<InfoLeaveUser>> GetInfoLeaveUser(string codeuser);
    }
}
