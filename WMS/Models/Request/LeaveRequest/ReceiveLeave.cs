using WMS.Models.Response.LeaveRequest;

namespace WMS.Models.Request.LeaveRequest
{
    public class ReceiveLeave
    {
        public int CountPending { get; set; }
        public int CountReceive { get; set; }
    }
}
