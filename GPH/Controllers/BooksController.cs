// Controllers/BooksController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using GPH.Models.DTOs;
using GPH.Services;
using Microsoft.AspNetCore.Authorization; // Authorize add karein
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace GPH.Controllers;
[Authorize(Roles = "Admin")] // Sirf Admin isko access kar sakta hai
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly BookExcelService _excelService;
    public BooksController(ApplicationDbContext context, BookExcelService excelService)
    {
        _context = context;
        _excelService = excelService;
    }
    // POST: /api/books
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookDto bookDto)
    {
        var newBook = new Book
        {
            Title = bookDto.Title,
            Subject = bookDto.Subject,
            ClassLevel = bookDto.ClassLevel,
            Medium = bookDto.Medium,
            IsSpecimen = true,
            IsGift = bookDto.IsGift,
            UnitPrice = bookDto.UnitPrice
        };
        _context.Books.Add(newBook);
        await _context.SaveChangesAsync();
        return Ok(newBook);
    }
    // GET: /api/books
    [HttpGet]
    [AllowAnonymous] // Sabhi logged-in users books dekh sakte hain
    public async Task<IActionResult> GetAllBooks()
    {
        var books = await _context.Books
            .Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Subject = b.Subject,
                ClassLevel = b.ClassLevel,
                Medium = b.Medium,
                IsGift = b.IsGift,
                UnitPrice = b.UnitPrice
            }).ToListAsync();
        return Ok(books);
    }
    // === NAYA METHOD: UPDATE BOOK ===
    // PUT: /api/books/{id}
   // [HttpPut("{id}")]
   [HttpPost("{id}/update")]

    public async Task<IActionResult> UpdateBook(int id, [FromBody] CreateBookDto bookDto)
    {
        var bookToUpdate = await _context.Books.FindAsync(id);
        if (bookToUpdate == null)
        {
            return NotFound();
        }
        bookToUpdate.Title = bookDto.Title;
        bookToUpdate.Subject = bookDto.Subject;
        bookToUpdate.ClassLevel = bookDto.ClassLevel;
        bookToUpdate.Medium = bookDto.Medium;
        bookToUpdate.IsGift = bookDto.IsGift;
        bookToUpdate.UnitPrice = bookDto.UnitPrice;
        await _context.SaveChangesAsync();
        return NoContent(); // Success, but no content to return
    }
    // === NAYA METHOD: DELETE BOOK ===
    // DELETE: /api/books/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var bookToDelete = await _context.Books.FindAsync(id);
        if (bookToDelete == null)
        {
            return NotFound();
        }
        _context.Books.Remove(bookToDelete);
        await _context.SaveChangesAsync();
        return NoContent(); // Success
    }

    // === BULK UPLOAD & DOWNLOAD METHODS ===

    // GET: /api/books/template - Download Excel template
    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var fileBytes = _excelService.GenerateExcelTemplate();
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "books_template.xlsx");
    }

    // GET: /api/books/export - Export all books to Excel
    [HttpGet("export")]
    public async Task<IActionResult> ExportBooks()
    {
        var books = await _context.Books.ToListAsync();
        var fileBytes = _excelService.ExportBooksToExcel(books);
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"books_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    // POST: /api/books/bulk-upload - Upload Excel file with books
    [HttpPost("bulk-upload")]
    public async Task<IActionResult> BulkUploadBooks(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        if (!file.FileName.EndsWith(".xlsx"))
        {
            return BadRequest(new { message = "Only .xlsx files are supported" });
        }

        using var stream = file.OpenReadStream();
        var result = await _excelService.ImportBooksFromExcel(stream);

        return Ok(result);
    }

    // POST: /api/books/bulk-delete - Delete multiple books
    [HttpPost("bulk-delete")]
    public async Task<IActionResult> BulkDeleteBooks([FromBody] List<int> bookIds)
    {
        var result = new BulkDeleteResultDto
        {
            TotalRequested = bookIds.Count
        };

        if (bookIds == null || bookIds.Count == 0)
        {
            return BadRequest(new { message = "No book IDs provided" });
        }

        foreach (var bookId in bookIds)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    result.Errors.Add($"Book with ID {bookId} not found");
                    result.FailureCount++;
                    continue;
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                result.SuccessCount++;
                result.DeletedBookIds.Add(bookId);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Failed to delete book ID {bookId}: {ex.Message}");
                result.FailureCount++;
            }
        }

        return Ok(result);
    }
}

