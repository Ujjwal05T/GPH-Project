// GPH/Models/BookDistribution.cs

namespace GPH.Models;

public class BookDistribution
{
    public int Id { get; set; }

    // --- Foreign Keys ---
    public int VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public int TeacherId { get; set; } // The teacher who received the book
    public Teacher Teacher { get; set; } = null!;

    public int BookId { get; set; } // The book that was given
    public Book Book { get; set; } = null!;

    // --- Payload Data ---
    // This is extra information about the relationship
    public int Quantity { get; set; } = 1;

    public bool WasRecommended { get; set; } = false; // Did the teacher recommend THIS book?
}