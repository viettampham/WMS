namespace WMS.Models.Request.Audit
{
    public class GetAuditRequest: PagingRequest
    {
        public string Auditor { get; set; }
        public string Location { get; set; }
        public DateTime DateCreateFrom { get; set; }
        public DateTime DateCreateTo { get; set; }
    }
}
