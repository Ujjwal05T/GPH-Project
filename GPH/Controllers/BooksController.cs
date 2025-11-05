// Controllers/BooksController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
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
    public BooksController(ApplicationDbContext context)
    {
        _context = context;
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
            IsGift = bookDto.IsGift
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
                IsGift = b.IsGift
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
}

