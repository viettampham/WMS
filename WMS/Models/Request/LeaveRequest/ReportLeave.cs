namespace WMS.Models.Request.LeaveRequest
{
    public class ReportLeave
    {
        public int TotalLeaves { get; set; }
        public int LeaveApproveds { get; set; }
        public int LeavePendings { get; set; }
        public int LeaveRefuseds { get; set; }

        public List<ReportByDepart> ReportByDeparts { get; set; }
        public List<TopLeaveEmpl> TopLeaveEmpls { get; set; }
    }

    public class ReportByDepart
    {
        public string PhongBan { get; set; }
        public int TotalEmpl { get; set; }
        public int TotalLeave { get; set; }
        public double? TotalDayLeave { get; set; }
        public double? AnnualLeave { get; set; }
        public double? UnpaidLeave { get; set; }
        public double Percentleave { get; set; }
    }

    public class TopLeaveEmpl {
        public string CodeEmpl { get; set; }
        public string FullName { get; set; }
        public string PhongBan { get; set; }
        public int TotalLeaves { get; set; }
        public double? TotalLeaveDays { get; set; }
    }
}
