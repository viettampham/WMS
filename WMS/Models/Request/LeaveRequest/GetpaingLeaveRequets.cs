namespace WMS.Models.Request.LeaveRequest
{
    public class GetpaingLeaveRequets:PagingRequest
    {
        public string CodeUser { get; set; }
        public string Status { get; set; }
    }
}
