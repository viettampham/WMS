namespace WMS.Models.Request.User
{
    public class AddUserRequest
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int RoleLevel { get; set; }
        public string CCCD { get; set; }
        public string PhongBan { get; set; }
    }
}
