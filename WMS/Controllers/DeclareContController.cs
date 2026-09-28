using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Models.Request.Cont;
using WMS.Models.Response.Cont;
using WMS.Services.impl;

namespace WMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeclareContController : ControllerBase
    {
        private readonly IDeclareContService _declareContService;
        public DeclareContController(IDeclareContService declareContService)
        {
            _declareContService = declareContService;
        }

        [HttpPost("declare-cont")]
        public async Task<IActionResult> Declare(DeclareContRequest req) {
            var res = await _declareContService.Declare(req);
            return Ok(res);
        }

        [HttpPost("add-image-to-cont")]
        public async Task<IActionResult> AddImageToCont(AddImageToContRequest req) { 
            var res = await _declareContService.AddImageToCont(req);
            return Ok(res);
        }

        [HttpPost("get-declare-cont")]
        public async Task<IActionResult> GetList(GetDeclateContRequest req) { 
            var res = await _declareContService.GetList(req);
            return Ok(res);
        }

        [HttpGet("download-folder/{id}")]
        public async Task<IActionResult> DownloadFolder(int id)
        {
            var result = await _declareContService.DownloadFolder(id);

            return File(
                result.FileBytes,
                "application/zip",
                result.FileName
            );
        }
    }
}
