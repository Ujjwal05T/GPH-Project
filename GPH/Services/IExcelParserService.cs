// GPH/Services/IExcelParserService.cs
using GPH.Models;
using System.IO;
using System.Collections.Generic;

namespace GPH.Services;

public interface IExcelParserService
{
    (List<ConsignmentItem> items, List<string> errors) ParseConsignmentFile(Stream fileStream, List<Book> allBooks);
}