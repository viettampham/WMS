namespace WMS.Models.Request.LeaveRequest
{
    public class GetLeaveRequest: PagingRequest
    {
        public string Code { get; set; }
        public string Status { get; set; }
        public string Username { get; set; }
    }
}
