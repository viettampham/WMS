namespace WMS.Models.Request.Audit
{
    public class CreateAuditRequest
    {
        public string Auditor { get; set; }
        public string Location { get; set; }
        public string Note { get; set; }
    }
}
