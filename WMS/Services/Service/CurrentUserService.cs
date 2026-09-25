using WMS.Services.impl;

namespace WMS.Services.Service
{
    public class CurrentUserService: ICurrentUserService
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

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            ID = int.Parse(user?.FindFirst("UserId")?.Value ?? "0");
            UserName = user?.FindFirst("UserName")?.Value;
            FullName = user?.FindFirst("FullName")?.Value;
            Email = user?.FindFirst("Email")?.Value;
            PhoneNumber = user?.FindFirst("PhoneNumber")?.Value;
            Address = user?.FindFirst("Address")?.Value;
            Avatar = user?.FindFirst("Avatar")?.Value;
            DateOfBirth = DateOnly.Parse(user?.FindFirst("DateOfBirth")?.Value ?? DateOnly.MinValue.ToString());
            Gender = user?.FindFirst("Gender")?.Value;
            RoleLevel = int.Parse(user?.FindFirst("RoleLevel")?.Value ?? "0");
            CCCD = user?.FindFirst("KhuVuc")?.Value;

        }
    }
}
