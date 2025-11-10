// GPH/Controllers/ConsignmentsController.cs
using GPH.Data;
using GPH.DTOs;
using GPH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Add this for accessing user claims
using Microsoft.AspNetCore.Hosting; // Isko add karein
using System.Text.Json; // JSON ke liye isko add kareinnamespace GPH.Controllers;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using GPH.Services;
[Authorize] // Protect the entire controller by default
[Route("api/[controller]")]
public class ConsignmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
        private readonly IExcelParserService _excelParserService; // <-- 1. Declare the field

    private readonly IWebHostEnvironment _hostingEnvironment; // Error #1 Fix: Isko yahan declare karein
    public ConsignmentsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, IExcelParserService excelParserService) // << --- CONSTRUCTOR UPDATE KAREIN ---
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment; // << --- YEH ADD KAREIN ---
         _excelParserService = excelParserService; 
    }
    // POST: /api/consignments
    [HttpPost]
    [Authorize(Roles = "Admin,ASM")]
    public async Task<IActionResult> CreateConsignment([FromForm] CreateConsignmentDto consignmentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        // --- File Upload Logic ---
        string? billUrl = null;
        if (consignmentDto.BiltyBillFile != null && consignmentDto.BiltyBillFile.Length > 0)
        {
            var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "bilty_bills");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }
            var uniqueFileName = $"{Guid.NewGuid()}_{consignmentDto.BiltyBillFile.FileName}";
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await consignmentDto.BiltyBillFile.CopyToAsync(stream);
            }
            billUrl = $"{Request.Scheme}://{Request.Host}/uploads/bilty_bills/{uniqueFileName}";
        }
        var newConsignment = new Consignment
        {
            TransportCompanyName = consignmentDto.TransportCompanyName,
            BiltyNumber = consignmentDto.BiltyNumber,
            DispatchDate = consignmentDto.DispatchDate.ToUniversalTime(),
            SalesExecutiveId = consignmentDto.SalesExecutiveId,
            Status = ConsignmentStatus.InTransit,
            BiltyBillUrl = billUrl
        };
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var items = JsonSerializer.Deserialize<List<ConsignmentItemDto>>(consignmentDto.ItemsJson ?? "[]", options);
        if (items != null && items.Any())
        {
            // Ab is nayi 'items' list par loop chalayein
            foreach (var itemDto in items)
            {
                newConsignment.Items.Add(new ConsignmentItem
                {
                    BookId = itemDto.BookId,
                    Quantity = itemDto.Quantity
                });
            }
        }
        else
        {
            return BadRequest(new { message = "Consignment must contain at least one item." });
        }
        // --- END FIX ---
        _context.Consignments.Add(newConsignment);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Consignment created successfully.", consignmentId = newConsignment.Id });
    }
    // GET: /api/consignments
    [HttpGet]
    [Authorize(Roles = "Admin,ASM")] // Only managers can see all consignments
    public async Task<IActionResult> GetAllConsignments()
    {
        var consignments = await _context.Consignments
            .Include(c => c.SalesExecutive)
            .Include(c => c.Items)
                .ThenInclude(item => item.Book)
            .OrderByDescending(c => c.DispatchDate)
            .Select(c => new ConsignmentDto
            {
                Id = c.Id,
                TransportCompanyName = c.TransportCompanyName,
                BiltyNumber = c.BiltyNumber,
                AssignedTo = c.SalesExecutive.Name,
                Status = c.Status,
                DispatchDate = c.DispatchDate,
                ReceivedDate = c.ReceivedDate,
                FreightCost = c.FreightCost,
                BiltyBillUrl = c.BiltyBillUrl,
                Items = c.Items.Select(item => new ConsignmentItemDto
                {
                    Id = item.Id,
                    BookId = item.BookId,
                    BookTitle = item.Book.Title,
                    BookSubject = item.Book.Subject,
                    BookClassLevel = item.Book.ClassLevel,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            })
            .ToListAsync();
        return Ok(consignments);
    }
    // In GPH/Controllers/ConsignmentsController.cs
    [HttpGet("/api/executives/me/consignments")]
    public async Task<IActionResult> GetMyConsignments()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var executiveId))
        {
            return Unauthorized();
        }
        var consignments = await _context.Consignments
            .Include(c => c.SalesExecutive)
            .Include(c => c.Items) // Items ko include karein
                .ThenInclude(item => item.Book) // Har item ke Book ko include karein
            .Where(c => c.SalesExecutiveId == executiveId)
            .OrderByDescending(c => c.DispatchDate)
            .Select(c => new ConsignmentDto
            {
                Id = c.Id,
                TransportCompanyName = c.TransportCompanyName,
                BiltyNumber = c.BiltyNumber,
                AssignedTo = c.SalesExecutive.Name,
                Status = c.Status,
                DispatchDate = c.DispatchDate,
                ReceivedDate = c.ReceivedDate,
                FreightCost = c.FreightCost,
                BiltyBillUrl = c.BiltyBillUrl,
                // Naya Logic: Items ki list ko map karein
                Items = c.Items.Select(item => new ConsignmentItemDto
                {
                    Id = item.Id,
                    BookId = item.BookId,
                    BookTitle = item.Book.Title,
                    BookSubject = item.Book.Subject,
                    BookClassLevel = item.Book.ClassLevel,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            })
            .ToListAsync();
        return Ok(consignments);
    }
    // POST: /api/consignments/{id}/receive
    [HttpPost("{id}/receive")]
    [Authorize(Roles = "Executive")] // Only executives can receive
    public async Task<IActionResult> ReceiveConsignment(int id, [FromForm] ReceiveConsignmentDto receiveDto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var executiveId))
        {
            return Unauthorized();
        }
        var consignment = await _context.Consignments.FindAsync(id);
        if (consignment == null)
        {
            return NotFound();
        }
        if (consignment.SalesExecutiveId != executiveId)
        {
            return Forbid();
        }
        if (consignment.Status == ConsignmentStatus.Delivered)
        {
            return BadRequest(new { message = "Consignment has already been marked as received." });
        }
        consignment.Status = ConsignmentStatus.Delivered;
        consignment.ReceivedDate = DateTime.UtcNow;
         consignment.FreightCost = receiveDto.FreightCost;
         if (receiveDto.BiltyBillPhoto != null && receiveDto.BiltyBillPhoto.Length > 0)
{
    string uniqueFileName = $"bilty_{id}_{Guid.NewGuid()}_{receiveDto.BiltyBillPhoto.FileName}";
    string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "bilty-bills");
    
    if (!Directory.Exists(uploadsFolder))
    {
        Directory.CreateDirectory(uploadsFolder);
    }
    
    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
        await receiveDto.BiltyBillPhoto.CopyToAsync(fileStream);
    }
    
    consignment.BiltyBillUrl = Path.Combine("bilty-bills", uniqueFileName).Replace('\\', '/');
}
        var itemsToAssign = await _context.ConsignmentItems
            .Where(ci => ci.ConsignmentId == id)
            .ToListAsync();
        foreach (var item in itemsToAssign)
        {
            var newAssignment = new InventoryAssignment
            {
                SalesExecutiveId = executiveId,
                BookId = item.BookId,
                QuantityAssigned = item.Quantity,
                DateAssigned = DateTime.UtcNow
            };
            _context.InventoryAssignments.Add(newAssignment);
        }
        await _context.SaveChangesAsync();
        return Ok(new { message = "Consignment marked as received and inventory updated." });
    }
    [HttpPost("bulk-upload")]
    [Authorize(Roles = "Admin")] // Only Admins can do bulk uploads
    public async Task<IActionResult> BulkUploadConsignment([FromForm] BulkConsignmentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var consignmentItems = new List<ConsignmentItem>();
        var errors = new List<string>();

        // Use a memory stream to read the file without saving it to disk
        using (var stream = new MemoryStream())
        {
            await dto.File.CopyToAsync(stream);
            stream.Position = 0;

            IWorkbook workbook = new XSSFWorkbook(stream);
            ISheet sheet = workbook.GetSheetAt(0); // Get the first sheet

            // --- VALIDATION: Check for correct headers ---
            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null || headerRow.GetCell(0)?.ToString()?.Trim().ToUpper() != "BOOKID" || headerRow.GetCell(1)?.ToString()?.Trim().ToUpper() != "QUANTITY")
            {
                return BadRequest("Invalid file format. The first row must contain 'BookID' and 'Quantity' headers.");
            }

            // --- DATA PROCESSING: Loop through the rows ---
            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row == null) continue; // Skip empty rows

                var bookIdCell = row.GetCell(0);
                var quantityCell = row.GetCell(1);

                if (bookIdCell == null || quantityCell == null)
                {
                    errors.Add($"Row {i + 1}: Contains empty cells.");
                    continue;
                }

                // Validate BookID
                if (!int.TryParse(bookIdCell.ToString(), out int bookId))
                {
                    errors.Add($"Row {i + 1}: Invalid BookID '{bookIdCell.ToString()}'. Must be a number.");
                    continue;
                }

                // Validate Quantity
                if (!int.TryParse(quantityCell.ToString(), out int quantity) || quantity <= 0)
                {
                    errors.Add($"Row {i + 1}: Invalid Quantity '{quantityCell.ToString()}'. Must be a positive number.");
                    continue;
                }

                // Check if the BookID exists in our database
                var bookExists = await _context.Books.AnyAsync(b => b.Id == bookId);
                if (!bookExists)
                {
                    errors.Add($"Row {i + 1}: Book with ID '{bookId}' not found in the database.");
                    continue;
                }

                consignmentItems.Add(new ConsignmentItem { BookId = bookId, Quantity = quantity });
            }
        }

        // If there were any errors during processing, return them
        if (errors.Any())
        {
            return BadRequest(new { message = "File processing failed with the following errors:", errors = errors });
        }

        if (!consignmentItems.Any())
        {
            return BadRequest("No valid items found in the uploaded file.");
        }

        // --- SAVE TO DATABASE ---
        var newConsignment = new Consignment
        {
            TransportCompanyName = dto.TransportCompanyName,
            BiltyNumber = dto.BiltyNumber,
            DispatchDate = dto.DispatchDate.ToUniversalTime(),
            SalesExecutiveId = dto.SalesExecutiveId,
            Status = ConsignmentStatus.InTransit,
            Items = consignmentItems // Add all the parsed items
        };

        _context.Consignments.Add(newConsignment);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Consignment with {consignmentItems.Count} items created successfully.", consignmentId = newConsignment.Id });
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConsignmentById(int id)
    {
        var consignment = await _context.Consignments
            .Include(c => c.SalesExecutive)
            .Include(c => c.Items)
                .ThenInclude(item => item.Book) // <-- This is the key: include the Book details
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consignment == null)
        {
            return NotFound();
        }

        var dto = new ConsignmentDetailDto
        {
            Id = consignment.Id,
            TransportCompanyName = consignment.TransportCompanyName,
            BiltyNumber = consignment.BiltyNumber,
            AssignedTo = consignment.SalesExecutive.Name,
            DispatchDate = consignment.DispatchDate,
            Items = consignment.Items.Select(item => new ConsignmentItemDetailDto
            {
                BookTitle = item.Book.Title,
                BookSubject = item.Book.Subject,
                BookClassLevel = item.Book.ClassLevel,
                Quantity = item.Quantity
            }).ToList()
        };

        return Ok(dto);
    }
[HttpGet("template")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DownloadTemplate()
{
    var allBooks = await _context.Books
        .OrderBy(b => b.Title)
.Select(b => new { b.Title, b.Subject, b.ClassLevel, b.Medium, b.UnitPrice })
        .ToListAsync();

    IWorkbook workbook = new XSSFWorkbook();
    
    // --- Sheet 1: The NEW, SIMPLER Template ---
    ISheet templateSheet = workbook.CreateSheet("Consignment Entry");
    
    IFont boldFont = workbook.CreateFont();
    boldFont.IsBold = true;
    ICellStyle headerStyle = workbook.CreateCellStyle();
    headerStyle.SetFont(boldFont);

    IRow headerRow = templateSheet.CreateRow(0);
    headerRow.CreateCell(0).SetCellValue("Title"); // CORRECTED HEADER
        headerRow.CreateCell(1).SetCellValue("Quantity");
    headerRow.CreateCell(2).SetCellValue("Rate"); // <-- ADD THIS
headerRow.GetCell(2).CellStyle = headerStyle; // <-- And its style
    headerRow.GetCell(0).CellStyle = headerStyle;
    headerRow.GetCell(1).CellStyle = headerStyle;

    // Add an example row to guide the user
    int templateRowNum = 1;
foreach (var book in allBooks)
{
    IRow row = templateSheet.CreateRow(templateRowNum++);
    row.CreateCell(0).SetCellValue(book.Title);
    row.CreateCell(1).SetCellValue(""); // Leave Quantity blank
    row.CreateCell(2).SetCellValue((double)book.UnitPrice); // Pre-fill the default rate
}

    templateSheet.SetColumnWidth(0, 256 * 50); // 50 characters wide for the title
        templateSheet.AutoSizeColumn(1);
    templateSheet.AutoSizeColumn(2); // <-- ADD THIS


    // --- Sheet 2: The Book Reference list ---
    ISheet referenceSheet = workbook.CreateSheet("Book Reference List");

    IRow refHeaderRow = referenceSheet.CreateRow(0);
    refHeaderRow.CreateCell(0).SetCellValue("Official Title (Copy this exactly)");
    refHeaderRow.CreateCell(1).SetCellValue("Subject");
    refHeaderRow.CreateCell(2).SetCellValue("ClassLevel");
        refHeaderRow.CreateCell(3).SetCellValue("Medium");
    refHeaderRow.CreateCell(4).SetCellValue("Default Rate (UnitPrice)"); // <-- ADD THIS
refHeaderRow.GetCell(4).CellStyle = headerStyle; // <-- And its style
    refHeaderRow.GetCell(0).CellStyle = headerStyle;
    refHeaderRow.GetCell(1).CellStyle = headerStyle;
    refHeaderRow.GetCell(2).CellStyle = headerStyle;
    refHeaderRow.GetCell(3).CellStyle = headerStyle;

    int rowNum = 1;
    foreach (var book in allBooks)
    {
        IRow row = referenceSheet.CreateRow(rowNum++);
        row.CreateCell(0).SetCellValue(book.Title);
        row.CreateCell(1).SetCellValue(book.Subject);
        row.CreateCell(2).SetCellValue(book.ClassLevel);
            row.CreateCell(3).SetCellValue(book.Medium);
        row.CreateCell(4).SetCellValue((double)book.UnitPrice); // <-- ADD THIS

    }

    referenceSheet.SetColumnWidth(0, 256 * 50);
    referenceSheet.AutoSizeColumn(1);
    referenceSheet.AutoSizeColumn(2);
        referenceSheet.AutoSizeColumn(3);
    referenceSheet.AutoSizeColumn(4); // <-- ADD THIS


    // --- Stream the file back to the user ---
    using (var memoryStream = new MemoryStream())
    {
        workbook.Write(memoryStream);
        var content = memoryStream.ToArray();
        return File(
            content, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            "TitleBased_ConsignmentTemplate.xlsx"
        );
    }
}
    // POST: /api/consignments/smart-upload

    [HttpPost("smart-upload")]
    [Authorize(Roles = "Admin")] // Only Admins can use this feature
    public async Task<IActionResult> SmartUploadConsignment([FromForm] BulkConsignmentDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Fetch all books from the database once for efficient lookup inside the parser
        var allBooks = await _context.Books.ToListAsync();

        // Use a memory stream to read the file without saving it to disk
        using var stream = new MemoryStream();
        await dto.File.CopyToAsync(stream);
        stream.Position = 0;

        // --- USE THE NEW SERVICE ---
        var (parsedItems, errors) = _excelParserService.ParseConsignmentFile(stream, allBooks);

        // If the parser found any errors, return them to the user
        if (errors.Any())
        {
            return BadRequest(new { message = "File processing failed with validation errors:", errors = errors });
        }

        if (!parsedItems.Any())
        {
            return BadRequest("No valid items could be parsed from the uploaded file.");
        }

 string? billUrl = null;
    if (dto.BiltyBillFile != null && dto.BiltyBillFile.Length > 0)
    {
        var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "bilty_bills");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }
        var uniqueFileName = $"{Guid.NewGuid()}_{dto.BiltyBillFile.FileName}";
       var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);
using (var fileStream = new FileStream(filePath, FileMode.Create)) // <-- YAHAN 'stream' ko 'fileStream' kiya gaya hai
{
    await dto.BiltyBillFile.CopyToAsync(fileStream); // <-- YAHAN bhi 'stream' ko 'fileStream' kiya gaya hai
}
        billUrl = $"{Request.Scheme}://{Request.Host}/uploads/bilty_bills/{uniqueFileName}";
    }

        // If parsing was successful, create the consignment
        var newConsignment = new Consignment
        {
            TransportCompanyName = dto.TransportCompanyName,
            BiltyNumber = dto.BiltyNumber,
            DispatchDate = dto.DispatchDate.ToUniversalTime(),
            SalesExecutiveId = dto.SalesExecutiveId,
            Status = ConsignmentStatus.InTransit,
  BiltyBillUrl = billUrl,
            Items = parsedItems // Add all the items parsed from the Excel file
        };

        _context.Consignments.Add(newConsignment);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Consignment with {parsedItems.Count} items created successfully.", consignmentId = newConsignment.Id });
    }
[HttpPost("smart-preview")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SmartPreviewConsignment([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded for preview.");
        }

        var allBooks = await _context.Books.ToListAsync();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        var (parsedItems, errors) = _excelParserService.ParseConsignmentFile(stream, allBooks);

        var previewResult = new ConsignmentPreviewDto
        {
            ErrorMessages = errors
        };

        // --- THIS IS THE FIX ---
        // Use the original 'allBooks' list to create a reliable dictionary for lookups.
        var allBooksDictionary = allBooks.ToDictionary(b => b.Id);

        previewResult.SuccessItems = parsedItems.Select(item => new ParsedItemDto
        {
            BookId = item.BookId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            // Safely get the title from the dictionary. This prevents the null reference crash.
            BookTitle = allBooksDictionary.TryGetValue(item.BookId, out var book) ? book.Title : "Unknown Book"
        }).ToList();
        // --- END FIX ---

        return Ok(previewResult);
    }
}

