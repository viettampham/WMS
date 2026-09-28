namespace WMS.Models.Request.Cont
{
    public class GetDeclateContRequest : PagingRequest
    {
        public string SoBooking { get; set; }
        public string SoCont { get; set; }
    }
}
