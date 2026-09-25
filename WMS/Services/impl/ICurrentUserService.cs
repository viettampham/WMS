namespace WMS.Services.impl
{
    public interface ICurrentUserService
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int RoleLevel { get; set; }
        public string CCCD { get; set; }
    }
}
