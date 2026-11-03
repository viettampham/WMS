using WMS.Models;
using WMS.Models.Request.LeaveRequest;
using WMS.Models.Response.Common;
using WMS.Models.Response.LeaveRequest;

namespace WMS.Services.impl
{
    public interface ILeaveRequest
    {
        Task<CommonResponseModel<string>> CreateleaveRequest(CreateLeaveRequest req);
        Task<CommonResponseModel<PagingResponse<LeaveResponse>>> GetLeave(GetLeaveRequest req);

        Task<CommonResponseModel<string>> CancelLeaveRequest(int id);

        Task<CommonResponseModel<LeaveType>> GetTypes();
    }
}
