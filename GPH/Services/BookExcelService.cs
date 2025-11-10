using GPH.Data;
using GPH.Models;
using GPH.Models.DTOs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Microsoft.EntityFrameworkCore;

namespace GPH.Services
{
    public class BookExcelService
    {
        private readonly ApplicationDbContext _context;

        public BookExcelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public byte[] GenerateExcelTemplate()
        {
            using var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Books");

            // Create header style
            var headerStyle = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            // Create header row
            var headerRow = sheet.CreateRow(0);
            var headers = new[] { "Title", "Subject", "ClassLevel", "Medium", "IsGift", "UnitPrice" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            // Add sample data row
            var sampleRow = sheet.CreateRow(1);
            sampleRow.CreateCell(0).SetCellValue("Sample Book Title");
            sampleRow.CreateCell(1).SetCellValue("Mathematics");
            sampleRow.CreateCell(2).SetCellValue("Class 10");
            sampleRow.CreateCell(3).SetCellValue("English Medium");
            sampleRow.CreateCell(4).SetCellValue("false");
            sampleRow.CreateCell(5).SetCellValue(299.99);

            // Auto-size columns
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }

        public byte[] ExportBooksToExcel(List<Book> books)
        {
            using var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Books");

            // Create header style
            var headerStyle = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            // Create header row
            var headerRow = sheet.CreateRow(0);
            var headers = new[] { "Title", "Subject", "ClassLevel", "Medium", "IsGift", "UnitPrice" };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
            }

            // Add data rows
            for (int i = 0; i < books.Count; i++)
            {
                var row = sheet.CreateRow(i + 1);
                var book = books[i];

                row.CreateCell(0).SetCellValue(book.Title);
                row.CreateCell(1).SetCellValue(book.Subject ?? "");
                row.CreateCell(2).SetCellValue(book.ClassLevel ?? "");
                row.CreateCell(3).SetCellValue(book.Medium ?? "");
                row.CreateCell(4).SetCellValue(book.IsGift.ToString().ToLower());
                row.CreateCell(5).SetCellValue((double)book.UnitPrice);
            }

            // Auto-size columns
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.AutoSizeColumn(i);
            }

            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }

        public async Task<BulkUploadResultDto> ImportBooksFromExcel(Stream fileStream)
        {
            var result = new BulkUploadResultDto();

            try
            {
                using var workbook = new XSSFWorkbook(fileStream);
                var sheet = workbook.GetSheetAt(0);

                // Skip header row
                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    result.TotalRows++;
                    var row = sheet.GetRow(i);

                    if (row == null)
                    {
                        result.Errors.Add($"Row {i + 1}: Empty row");
                        result.FailureCount++;
                        continue;
                    }

                    try
                    {
                        // Read cell values
                        var title = GetCellValue(row.GetCell(0))?.Trim();
                        var subject = GetCellValue(row.GetCell(1))?.Trim();
                        var classLevel = GetCellValue(row.GetCell(2))?.Trim();
                        var medium = GetCellValue(row.GetCell(3))?.Trim();
                        var isGiftStr = GetCellValue(row.GetCell(4))?.Trim().ToLower();
                        var unitPriceStr = GetCellValue(row.GetCell(5))?.Trim();

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(title))
                        {
                            result.Errors.Add($"Row {i + 1}: Title is required");
                            result.FailureCount++;
                            continue;
                        }

                        // Check if book already exists
                        var existingBook = await _context.Books
                            .FirstOrDefaultAsync(b => b.Title.ToLower() == title.ToLower());

                        if (existingBook != null)
                        {
                            result.Errors.Add($"Row {i + 1}: Book with title '{title}' already exists");
                            result.FailureCount++;
                            continue;
                        }

                        // Parse IsGift
                        bool isGift = false;
                        if (!string.IsNullOrWhiteSpace(isGiftStr))
                        {
                            if (!bool.TryParse(isGiftStr, out isGift))
                            {
                                result.Errors.Add($"Row {i + 1}: Invalid IsGift value. Use 'true' or 'false'");
                                result.FailureCount++;
                                continue;
                            }
                        }

                        // Parse UnitPrice
                        decimal unitPrice = 0;
                        if (!string.IsNullOrWhiteSpace(unitPriceStr))
                        {
                            if (!decimal.TryParse(unitPriceStr, out unitPrice))
                            {
                                result.Errors.Add($"Row {i + 1}: Invalid UnitPrice value");
                                result.FailureCount++;
                                continue;
                            }
                        }

                        // Create new book
                        var book = new Book
                        {
                            Title = title,
                            Subject = subject ?? string.Empty,
                            ClassLevel = classLevel ?? string.Empty,
                            Medium = medium,
                            IsGift = isGift,
                            IsSpecimen = !isGift,
                            UnitPrice = unitPrice
                        };

                        _context.Books.Add(book);
                        await _context.SaveChangesAsync();

                        result.SuccessCount++;
                        result.SuccessfulBooks.Add(new BookDto
                        {
                            Id = book.Id,
                            Title = book.Title,
                            Subject = book.Subject ?? string.Empty,
                            ClassLevel = book.ClassLevel ?? string.Empty,
                            Medium = book.Medium,
                            IsGift = book.IsGift,
                            UnitPrice = book.UnitPrice
                        });
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Row {i + 1}: {ex.Message}");
                        result.FailureCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"File processing error: {ex.Message}");
            }

            return result;
        }

        private string? GetCellValue(ICell? cell)
        {
            if (cell == null) return null;

            return cell.CellType switch
            {
                CellType.String => cell.StringCellValue,
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.Boolean => cell.BooleanCellValue.ToString(),
                CellType.Formula => cell.StringCellValue,
                _ => null
            };
        }
    }
}
