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

        Task<CommonResponseModel<LeaveResponse>> PhatHanhDon(string Code);

        Task<CommonResponseModel<PendingAsignee>> GetPendingAsigneeByCurrentStep(int CurrentStep);

        Task<CommonResponseModel<PagingResponse<LeaveRequest>>> GetDSLeave(GetpaingLeaveRequets req);

        Task<CommonResponseModel<PagingResponse<LeaveResponse>>> GetDsPendingApproveByCodeUser(GetpaingLeaveApprovePending req);

        Task<CommonResponseModel<LeaveResponse>> XacNhanDon(XacNhanDonRequest req);
    }
}
