using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Models.Request.LeaveRequest;
using WMS.Services.impl;

namespace WMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestController : ControllerBase
    {
        private ILeaveRequest _leaveRequest;
        public LeaveRequestController(ILeaveRequest leaveRequest)
        {
            _leaveRequest = leaveRequest;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateLeaveRequest(CreateLeaveRequest req) {
            var res = await _leaveRequest.CreateleaveRequest(req);
            return Ok(res);
        }

        [HttpPost("get-list")]
        public async Task<IActionResult> GetDS(GetLeaveRequest req) {
            var res = await _leaveRequest.GetLeave(req);
            return Ok(res);
        }

        [HttpPost("get-leavetype")]
        public async Task<IActionResult> getleaveType()
        {
            var res = await _leaveRequest.GetTypes();
            return Ok(res);
        }
    }
}
