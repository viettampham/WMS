namespace WMS.Models.Request.LeaveRequest
{
    public class CreateLeaveRequest
    {
        public string CodeUser { get; set; }
        public string UserName { get; set; }
        public string LeaveType { get; set; }
        public string TypeShift { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDay { get; set; }
        public string Reason { get; set; }
    }
}
