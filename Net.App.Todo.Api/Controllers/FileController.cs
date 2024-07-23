using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Net.App.Todo.Api.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly string _storagePath;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var filePath = Path.Combine(_storagePath, file.FileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded: {file.FileName}");
                return Ok(new { message = "File uploaded successfully.", fileName = file.FileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "Filename is not specified." });
            }

            var filePath = Path.Combine(_storagePath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { message = "File not found." });
            }

            try
            {
                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                _logger.LogInformation($"File downloaded: {fileName}");
                return File(memory, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }
    }
}
