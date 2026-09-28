namespace WMS.Models.Request.Cont
{
    public class AddImageToContRequest
    {
        public int IDCont { get; set; }
        public IFormFile Image { get; set; }
        public string NguoiChup { get; set; }
    }
}
