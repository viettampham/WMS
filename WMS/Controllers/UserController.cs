using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Models.Request.User;
using WMS.Services.impl;
using WMS.Services.Service;

namespace WMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) {
            _userService = userService;
        }

        [HttpPost("search-user")]
        public async Task<IActionResult> SearchUser(GetUserRequest req) {
            var res = await _userService.SearchUser(req);
            return Ok(res);
        }

        [HttpPost("add-user")]
        public async Task<IActionResult> AddUser(AddUserRequest req) {
            var res = await _userService.AddUser(req);
            return Ok(res);
        }

        [HttpPost("authentication")]
        public async Task<IActionResult> Authen(AuthenRequest req) {
            var res = await _userService.Authentication(req);
            return Ok(res);
        }

        [HttpPost("update-user")]
        public async Task<IActionResult> UpdateUser(UpdateUserRequest req)
        {
            var res = await _userService.UpdateUser(req);
            return Ok(res);
        }

        [HttpDelete("delete-user{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var res = await _userService.DeleteUser(id);
            return Ok(res);
        }
    }
}
