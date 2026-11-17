namespace WMS.Models.Request.User
{
    public class ChangePasswordRequest
    {
        public string CodeUser { get; set; }
        public string CurrentPass { get; set; }
        public string NewPass { get; set; }
        public string ConfirmNewPass { get; set; }
    }

    public class ChangePasswordByAdminRequest
    {
        public string CodeUser { get; set; }
        public string NewPass { get; set; }
    }
}
