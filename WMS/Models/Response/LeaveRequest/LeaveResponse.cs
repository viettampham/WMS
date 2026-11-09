namespace WMS.Models.Response.LeaveRequest
{
    public class LeaveResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string LeaveType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? TotalDay { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int? CurentStep { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public string PhongBan { get; set; }
        public string ChucVu { get; set; }
        public int? Stt{ get; set; }
        public string? TypeShift { get; set; }
        public string? Fullname { get; set; }

    }
}
