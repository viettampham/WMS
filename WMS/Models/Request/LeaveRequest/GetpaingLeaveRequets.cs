namespace WMS.Models.Request.LeaveRequest
{
    public class GetpaingLeaveRequets:PagingRequest
    {
        public string CodeUser { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string StatusDone { get; set; }
    }

    public class GetpaingLeaveApprovePending : PagingRequest
    {
        public string CodePending { get; set; }
        public string Status { get; set; }
    }
}
