namespace WMS.Models.Response.Common
{
    public class CommonResponseModel<T>
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public T Data { get; set; }
        public List<T> ListData { get; set; }
    }
}
