namespace WMS.Models.Response.LeaveRequest
{
    public class LeaveResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string LeaveType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDay { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string CurentStep { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }

    }
}
