using WMS.Models.Request.User;
using WMS.Models.Response.Common;
using WMS.Models.Response.User;

namespace WMS.Services.impl
{
    public interface IUserService
    {
        Task<CommonResponseModel<PagingResponse<UserResponseModal>>> SearchUser(GetUserRequest request);
        Task<CommonResponseModel<string>> AddUser(AddUserRequest request);
        Task<CommonResponseModel<string>> UpdateUser(UpdateUserRequest request);
        Task<CommonResponseModel<string>> DeleteUser(int id);
        Task<CommonResponseModel<string>> Authentication(AuthenRequest request);
    }
}
