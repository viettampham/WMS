namespace WMS.Models.Response.Audit
{
    public class AuditDetailResponse
    {
        public int Id { get; set; }
        public bool IsPrivate { get; set; }
        public int AuditId { get; set; }
        public string OperationStandard { get; set; }
        public string DetechIssue { get; set; }
        public string ImageBefore { get; set; }
        public string ImageAfter { get; set; }
        public string? Assignee { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string Evalue { get; set; }
        public DateTime? DoneTime{ get; set; }
        public string Location { get; set; }
        public string Owner { get; set; }
        public string ActionPlan { get; set; }
        public DateTime? DateCreate { get; set; }
    }
}
