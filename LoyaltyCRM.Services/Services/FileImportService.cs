using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mapster;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Dtos.FileImport;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System.IO;
using LoyaltyCRM.DTOs.Requests.Yearcard;

namespace LoyaltyCRM.Services.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly IFileReaderService _fileReaderService;
        private readonly IYearcardService _yearcardService;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<FileImportService> _logger;

        public FileImportService(
            IFileReaderService fileReaderService,
            IYearcardService yearcardService,
            UserManager<ApplicationUser> userManager,
            ILogger<FileImportService> logger)
        {
            _fileReaderService = fileReaderService;
            _yearcardService = yearcardService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ImportPreviewResponse> PreviewHeadersAsync(Stream fileStream, string fileName)
        {
            var headers = await _fileReaderService.GetHeadersAsync(fileStream, fileName);
            return new ImportPreviewResponse { Headers = headers.ToList() };
        }

        public async Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName, Dictionary<string, string> columnMapping, DateTime startDate)
        {
            var rows = await _fileReaderService.ReadRowsAsync(fileStream, fileName);
            var result = new ImportResultDto();
            var invalidRows = new List<Dictionary<string, string>>();
            var createdCount = 0;

            foreach (var row in rows)
            {
                try
                {
                    var importRow = MapRow(row, columnMapping);
                    importRow.StartDate = startDate;
                    await ProcessRowAsync(importRow);
                    createdCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Import row failed");
                    var errorRow = new Dictionary<string, string>(row, StringComparer.OrdinalIgnoreCase)
                    {
                        ["Error"] = ex.Message
                    };
                    invalidRows.Add(errorRow);
                }
            }

            result.CreatedCount = createdCount;
            result.FailedCount = invalidRows.Count;
            result.Success = result.FailedCount == 0;
            result.Message = result.Success
                ? $"Imported {createdCount} rows."
                : $"Imported {createdCount} rows, but {result.FailedCount} row(s) failed. Please download the error report.";

            if (invalidRows.Any())
            {
                result.ErrorFileName = GenerateErrorFileNameExcel(fileName);

                var excelBytes = BuildExcelErrorReport(invalidRows);
                result.ErrorFileBase64 = Convert.ToBase64String(excelBytes);
            }

            return result;
        }

        private static YearcardImportRequest MapRow(IDictionary<string, string> row, Dictionary<string, string> columnMapping)
        {
            var importRow = new YearcardImportRequest
            {
                CardId = GetMappedValue<int>(row, columnMapping, nameof(YearcardImportRequest.CardId)),
                ValidTo = GetMappedValue<DateTime?>(row, columnMapping, nameof(YearcardImportRequest.ValidTo)),
                PhoneNumber = GetMappedValue<string>(row, columnMapping, nameof(YearcardImportRequest.PhoneNumber)),
                Email = GetMappedValue<string>(row, columnMapping, nameof(YearcardImportRequest.Email)),
                Name = GetMappedValue<string>(row, columnMapping, nameof(YearcardImportRequest.Name)),
                UserName = GetMappedValue<string>(row, columnMapping, nameof(YearcardImportRequest.UserName))
            };


            return importRow;
        }

        private static T? GetMappedValue<T>(
            IDictionary<string, string> row,
            Dictionary<string, string> columnMapping,
            string targetProperty)
        {
            if (!columnMapping.TryGetValue(targetProperty, out var header) ||
                string.IsNullOrWhiteSpace(header) ||
                !row.TryGetValue(header, out var raw))
            {
                return default;
            }

            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                if (DateTime.TryParse(raw, out var dt))
                    return (T)(object)dt;

                return default;
            }

            // int
            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
            {
                if (int.TryParse(raw, out var i))
                    return (T)(object)i;

                return default;
            }

            return (T)(object)raw;
        }

        private async Task ProcessRowAsync(YearcardImportRequest importRow)
        {
            await _yearcardService.ImportYearcard(importRow);
        }

        private static string BuildCsvErrorReport(IEnumerable<Dictionary<string, string>> invalidRows)
        {
            var headers = invalidRows
                .SelectMany(row => row.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

            foreach (var row in invalidRows)
            {
                var values = headers.Select(header => row.TryGetValue(header, out var value) ? EscapeCsvValue(value) : string.Empty);
                sb.AppendLine(string.Join(",", values));
            }

            return sb.ToString();
        }

        private static string EscapeCsvValue(string value)
        {
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return '"' + value.Replace("\"", "\"\"") + '"';
            }
            return value;
        }

        private static string GenerateErrorFileNameCsv(string originalFileName)
        {
            var extension = ".csv";
            return Path.GetFileNameWithoutExtension(originalFileName) + "-import-errors" + extension;
        }

        private static string GenerateErrorFileNameExcel(string originalFileName)
        {
            return Path.GetFileNameWithoutExtension(originalFileName) + "-import-errors.xlsx";
        }

        private static byte[] BuildExcelErrorReport(IEnumerable<Dictionary<string, string>> invalidRows)
        {
            var headers = invalidRows
                .SelectMany(row => row.Keys)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Errors");

            // Headers
            for (int col = 0; col < headers.Count; col++)
            {
                worksheet.Cell(1, col + 1).Value = headers[col];
            }

            // Rows
            int rowIndex = 2;
            foreach (var row in invalidRows)
            {
                for (int col = 0; col < headers.Count; col++)
                {
                    var header = headers[col];
                    worksheet.Cell(rowIndex, col + 1).Value =
                        row.TryGetValue(header, out var value) ? value : string.Empty;
                }
                rowIndex++;
            }

            // Nice formatting (optional but recommended)
            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
