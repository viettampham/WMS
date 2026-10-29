namespace WMS.Models.Request.Audit
{
    public class PhanCong
    {
        public int IdAuditDetail { get; set; }
        public string Asignee { get; set; }
        public DateTime DueDate { get; set; }
    }
}
