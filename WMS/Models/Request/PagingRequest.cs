namespace WMS.Models.Request
{
    public class PagingRequest
    {
        public int PageIndex { get; set; } = 1;   // Trang hiện tại
        public int PageSize { get; set; } = 10;   // Số bản ghi / trang
    }
}
