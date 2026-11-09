using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IBlobStorageService _blobStorageService;

        public FilesController(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("No file provided.");
            }

            try
            {
                var container = string.IsNullOrWhiteSpace(dto.Container) ? "banners" : dto.Container;
                var fileUrl = await _blobStorageService.UploadFileAsync(dto.File, container);
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Upload failed: {ex.Message}");
            }
        }
    }

    public class FileUploadDto
    {
        public required IFormFile File { get; set; }
        public string? Container { get; set; } = "banners";
    }
}