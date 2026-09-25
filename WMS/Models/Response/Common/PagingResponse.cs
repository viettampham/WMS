namespace WMS.Models.Response.Common
{
    public class PagingResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();

        public int TotalRecords { get; set; }  // Tổng số bản ghi

        public int PageIndex { get; set; }     // Trang hiện tại

        public int PageSize { get; set; }      // Số bản ghi mỗi trang

        public int TotalPages
        {
            get
            {
                if (PageSize == 0) return 0;
                return (int)Math.Ceiling((double)TotalRecords / PageSize);
            }
        }
    }
}
