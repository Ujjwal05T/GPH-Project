// GPH/Services/ExcelParserService.cs
using GPH.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GPH.Services;

public class ExcelParserService : IExcelParserService
{
    public (List<ConsignmentItem> items, List<string> errors) ParseConsignmentFile(Stream fileStream, List<Book> allBooks)
    {
        var items = new List<ConsignmentItem>();
        var errors = new List<string>();
        
        IWorkbook workbook;
        try 
        { 
            workbook = new XSSFWorkbook(fileStream); 
        }
        catch (Exception ex)
        {
            errors.Add($"Failed to read the Excel file. It might be corrupted. Error: {ex.Message}");
            return (items, errors);
        }

        ISheet sheet = workbook.GetSheetAt(0);

        // --- VALIDATION: Check for all three headers ---
        IRow headerRow = sheet.GetRow(0);
        if (headerRow == null || 
            headerRow.GetCell(0)?.ToString()?.Trim().ToUpper() != "TITLE" || 
            headerRow.GetCell(1)?.ToString()?.Trim().ToUpper() != "QUANTITY" ||
            headerRow.GetCell(2)?.ToString()?.Trim().ToUpper() != "RATE")
        {
            errors.Add("Invalid file format. Headers must be 'Title', 'Quantity', and 'Rate'.");
            return (items, errors);
        }

        // For faster lookups, create a dictionary mapping the UPPERCASE title to the full Book object
       // var bookDictionary = allBooks.ToDictionary(b => b.Title.ToUpper().Trim(), b => b);
var bookDictionary = allBooks
    .GroupBy(b => b.Title.ToUpper().Trim())
    .ToDictionary(g => g.Key, g => g.First());
        // Start from row 1 to skip the header
        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            if (row == null) continue;

            string title = row.GetCell(0)?.ToString()?.ToUpper().Trim() ?? "";
            var quantityCell = row.GetCell(1);
            var rateCell = row.GetCell(2);

            if (string.IsNullOrWhiteSpace(title)) continue;

            // Validate Quantity
            if (quantityCell == null || !int.TryParse(quantityCell.ToString(), out int quantity) || quantity <= 0)
            {
                errors.Add($"Row {i + 1} ('{row.GetCell(0)?.ToString()}'): Invalid Quantity '{quantityCell?.ToString()}'.");
                continue;
            }

            // Find the book in our dictionary by its title
            if (bookDictionary.TryGetValue(title, out Book? foundBook))
            {
                decimal finalUnitPrice;

                // --- HYBRID RATE LOGIC ---
                // Try to parse the rate from the Excel file.
                if (rateCell != null && decimal.TryParse(rateCell.ToString(), out decimal overridePrice) && overridePrice > 0)
                {
                    // If a valid, positive rate is provided in the file, use it.
                    finalUnitPrice = overridePrice;
                }
                else
                {
                    // Otherwise (if cell is empty, text, or zero), use the default UnitPrice from the database.
                    finalUnitPrice = foundBook.UnitPrice;
                }
                // --- END HYBRID LOGIC ---

                items.Add(new ConsignmentItem { 
                    BookId = foundBook.Id, 
                    Quantity = quantity, 
                    UnitPrice = finalUnitPrice 
                });
            }
            else
            {
                errors.Add($"Row {i + 1}: No book found in the database with the exact title '{row.GetCell(0)?.ToString()}'.");
            }
        }
        
        return (items, errors);
    }
}