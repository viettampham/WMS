namespace WMS.Models.Response.Cont
{
    public class ConImageResponse
    {
        public int ID { get; set; }
        public int? IDCont { get; set; }
        public string? Path { get; set; }
        public string? NguoiChup { get; set; }
        public DateTime? ThoiGianChup { get; set; }
    }

    public class ConImageResponseWithIDCont
    {
        public int IDCont { get; set; }
        public List<ConImageResponse> LstDataImage { get; set; }

    }
}
