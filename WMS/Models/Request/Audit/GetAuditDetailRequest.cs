namespace WMS.Models.Request.Audit
{
    public class GetAuditDetailRequest: PagingRequest
    {
        public string Assignee { get; set; }
        public string Status { get; set; }

    }

    public class GetAuditOwner : PagingRequest
    {
        public string Owner { get; set; }
        public string Status { get; set; }
    }
}
