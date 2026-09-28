using WMS.Models.Request;

namespace WMS.Models.Response.Cont
{
    public class DeclareContResponse
    {
        public int ID { get; set; }
        public string SoCont { get; set; }
        public string SoLuongAnh { get; set; }
        public string NguoiKhaiBao { get; set; }
        public string SoBooking { get; set; }
    }
}
