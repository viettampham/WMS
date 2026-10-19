namespace WMS.Models.Response.Audit
{
    public class AuditResponse
    {
        public int Id { get; set; }
        public string Auditor { get; set; }
        public DateTime DateCreate { get; set; }
        public string Location { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
    }
}
