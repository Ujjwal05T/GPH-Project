// GPH/Controllers/FilesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace GPH.Controllers;
[Authorize] // Sirf logged-in user hi file upload kar sakta hai
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    public FilesController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }
    // POST: /api/files/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was selected for upload." });
        }
        // 1. Define a safe folder to save the files.
        var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }
        // 2. Create a unique file name.
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
        // 3. Save the file.
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        // 4. Return the public URL.
        var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";
        return Ok(new { url = fileUrl });
    }
}