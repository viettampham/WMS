namespace WMS.Models.Request.Audit
{
    public class AddImageToAuditDetail
    {
        public int IdAuditDetail { get; set; }
        public string ActionPlan { get; set; }
        public IFormFile ImageAfter { get; set; }
    }
}
