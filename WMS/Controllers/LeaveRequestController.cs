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

        [HttpGet("get-leavetype")]
        public async Task<IActionResult> getleaveType()
        {
            var res = await _leaveRequest.GetTypes();
            return Ok(res);
        }

        [HttpGet("get-leave-by-code")]
        public async Task<IActionResult> GetLeaveByCode(string code) { 
            var res = await _leaveRequest.GetLeaveByCode(code);
            return Ok(res);
        }

        [HttpPost("phat-hanh-don")]
        public async Task<IActionResult> PhatHanhDon(string code) { 
            var res = await _leaveRequest.PhatHanhDon(code);
            return Ok(res);
        }

        [HttpGet("get-assigne-pending")]
        public async Task<IActionResult> GetpendingAsignee(int currstp) { 
            var res = await _leaveRequest.GetPendingAsigneeByCurrentStep(currstp);
            return Ok(res);
        }

        [HttpPost("get-ds-leaves")]
        public async Task<IActionResult> GetDsleave(GetpaingLeaveRequets req) {
            var res = await _leaveRequest.GetDSLeave(req);
            return Ok(res);
        }

        [HttpPost("get-pending-currentstep")]
        public async Task<IActionResult> GetPendingCurrentStep(GetpaingLeaveApprovePending req) { 
            var res = await _leaveRequest.GetDsPendingApproveByCodeUser(req);
            return Ok(res);
        }


        [HttpPost("xac-nhan-don")]
        public async Task<IActionResult> XacNhanDon(XacNhanDonRequest req) {
            var res = await _leaveRequest.XacNhanDon(req);
            return Ok(res);
        }

        [HttpPost("reject-leave")]
        public async Task<IActionResult> RejectLeave(RejectLeaveRequets req) {
            var res = await _leaveRequest.RejectLeave(req);
            return Ok(res);
        }

        [HttpPost("approve-leave")]
        public async Task<IActionResult> ApproveLeave(XacNhanDonRequest req) {
            var res = await _leaveRequest.ApproveLeave(req);
            return Ok(res);
        }
    }
}
