namespace WMS.Models.Request.Audit
{
    public class CreateAuditDetailRequest
    {
        public int IdAudit { get; set; }
        public bool IsPrivate { get; set; }
        public int IdOps { get; set; }
        public int IdDetachIssue { get; set; }

        public string? Assignee { get; set; }
        public string? DueDate { get; set; }
        public string Status  { get; set; }
        public IFormFile ImageBefore { get; set; }
        public string Evalue { get; set; }
        public string Note { get; set; }
        public string Location { get; set; }
        public string Owner { get; set; }
    }
}
