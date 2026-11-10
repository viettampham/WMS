namespace WMS.Models.Request.LeaveRequest
{
    public class CreateLeaveRequest
    {
        public string CodeUser { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string LeaveType { get; set; }
        public string TypeShift { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalDay { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string PendingAsigneeCode { get; set; }
    }
}
