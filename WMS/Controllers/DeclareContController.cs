using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WMS.Models.Request.Cont;
using WMS.Models.Response;
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

        [HttpGet("view-image")]
        public async Task<IActionResult> ViewImage(int id) { 
            var res = await _declareContService.ViewImage(id);
            return Ok(res);
        }

        [HttpPost("count")]
        public async Task<IActionResult> Count(IFormFile file)
        {
            using var client = new HttpClient();

            using var form = new MultipartFormDataContent();

            using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

            form.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync(
                "http://127.0.0.1:5000/count",
                form
            );

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result = System.Text.Json.JsonSerializer.Deserialize<CountResponse>(
                json,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            // Tải ảnh từ FastAPI
            var imageBytes = await client.GetByteArrayAsync(result!.image_url);

            // Convert sang Base64
            var imageBase64 = Convert.ToBase64String(imageBytes);

            return Ok(new
            {
                count = result.Count,
                imageBase64 = imageBase64
            });
        }
    }
}
