using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Models.Request.Audit;
using WMS.Services.impl;

namespace WMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpPost("get-audit")]
        public async Task<IActionResult> GetAudit(GetAuditRequest req) {
            var res = await _auditService.GetAudit(req);
            return Ok(res);
        }

        [HttpPost("create-audit")]
        public async Task<IActionResult> CreateAudit(CreateAuditRequest req) {
            var res = await _auditService.CreateAudit(req);
            return Ok(res);
        }

        [HttpDelete("delete-audit{id}")]
        public async Task<IActionResult> DeleteAudit(int id) { 
            var res = await _auditService.DeleteAudit(id);
            return Ok(res);
        }

        [HttpGet("get-all-operationstandard")]
        public async Task<IActionResult> GetOperationStandard() { 
            var res = await _auditService.GetOperation();
            return Ok(res);
        }

        [HttpGet("get-detechissue")]
        public async Task<IActionResult> GetDetechIssue(int id) { 
            var res = await _auditService.GetDetechByOps(id);
            return Ok(res);
        }

        [HttpGet("get-assignee")]
        public async Task<IActionResult> GetAllAssignee() { 
            var res = await _auditService.GetAssignee();
            return Ok(res);
        }

        [HttpPost("create-audit-detail")]
        public async Task<IActionResult> CreateAuditDetail(CreateAuditDetailRequest req) { 
            var res = await _auditService.CreateAuditDetail(req);
            return Ok(res);
        }

        [HttpPost("create-audit-detail-daily")]
        public async Task<IActionResult> CreateAuditDetailDaily(CreateAuditDetailRequest req)
        {
            var res = await _auditService.CreateAuditDetailDaily(req);
            return Ok(res);
        }

        [HttpPost("get-audit-detail")]
        public async Task<IActionResult> GetAuditDetail(GetAuditDetailRequest req) { 
            var res = await _auditService.GetAuditDetail(req);
            return Ok(res);
        }

        [HttpGet("get-location")]
        public async Task<IActionResult> GetLocation() { 
            var res = await _auditService.GetLocation();
            return Ok(res);
        }

        [HttpGet("get-auditdetail-by-id")]
        public async Task<IActionResult> GetAuditDetailByID(int id) { 
            var res = await _auditService.GetAuditDetailByID(id);
            return Ok(res);
        }

        [HttpPost("add-image-after")]
        public async Task<IActionResult> AddImageAfter(AddImageToAuditDetail req) {
            var res = await _auditService.AddImageAfterToAuditDetail(req);
            return Ok(res);
        }
    }
}
