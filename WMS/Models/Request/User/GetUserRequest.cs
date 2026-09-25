namespace WMS.Models.Request.User
{
    public class GetUserRequest: PagingRequest
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string Status { get; set; }
        public string PhongBan { get; set; }
    }
}
