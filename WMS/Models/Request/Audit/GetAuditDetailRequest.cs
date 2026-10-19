namespace WMS.Models.Request.Audit
{
    public class GetAuditDetailRequest: PagingRequest
    {
        public string Assignee { get; set; }
        //public DateTime FromDate { get; set; }
        //public DateTime ToDate { get; set; }
        public string Status { get; set; }
    }
}
