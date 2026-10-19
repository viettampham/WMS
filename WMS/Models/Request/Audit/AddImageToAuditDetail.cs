namespace WMS.Models.Request.Audit
{
    public class AddImageToAuditDetail
    {
        public int IdAuditDetail { get; set; }
        public IFormFile ImageAfter { get; set; }
    }
}
