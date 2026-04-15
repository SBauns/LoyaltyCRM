using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Dtos.FileImport;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LoyaltyCRM.Services.Services
{
    public class FileImportService : IFileImportService
    {
        private readonly IFileReaderService _fileReaderService;
        private readonly IMapper _mapper;
        private readonly ICustomerRepo _customerRepo;
        private readonly IYearcardRepo _yearcardRepo;
        private readonly IYearcardService _yearcardService;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<FileImportService> _logger;

        public FileImportService(
            IFileReaderService fileReaderService,
            IMapper mapper,
            ICustomerRepo customerRepo,
            IYearcardRepo yearcardRepo,
            IYearcardService yearcardService,
            UserManager<ApplicationUser> userManager,
            ILogger<FileImportService> logger)
        {
            _fileReaderService = fileReaderService;
            _mapper = mapper;
            _customerRepo = customerRepo;
            _yearcardRepo = yearcardRepo;
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
                    await ProcessRowAsync(importRow, startDate);
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
                result.ErrorFileName = GenerateErrorFileName(fileName);
                result.ErrorFileBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(BuildCsvErrorReport(invalidRows)));
            }

            return result;
        }

        private static ImportRowDto MapRow(IDictionary<string, string> row, Dictionary<string, string> columnMapping)
        {
            var importRow = new ImportRowDto
            {
                CardId = GetMappedValue(row, columnMapping, nameof(ImportRowDto.CardId)),
                ValidTo = GetMappedValue(row, columnMapping, nameof(ImportRowDto.ValidTo)),
                PhoneNumber = GetMappedValue(row, columnMapping, nameof(ImportRowDto.PhoneNumber)),
                Email = GetMappedValue(row, columnMapping, nameof(ImportRowDto.Email)),
                Name = GetMappedValue(row, columnMapping, nameof(ImportRowDto.Name)),
                UserName = GetMappedValue(row, columnMapping, nameof(ImportRowDto.UserName))
            };

            return importRow;
        }

        private static string? GetMappedValue(IDictionary<string, string> row, Dictionary<string, string> columnMapping, string targetProperty)
        {
            if (columnMapping == null || !columnMapping.TryGetValue(targetProperty, out var header))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(header))
            {
                return null;
            }

            return row.TryGetValue(header, out var value) ? value : null;
        }

        private static bool TryParseDate(string value, out DateTime result)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return true;
            }

            return DateTime.TryParse(value, new CultureInfo("da-DK"), DateTimeStyles.None, out result);
        }

        private async Task ProcessRowAsync(ImportRowDto importRow, DateTime startDate)
        {
            var yearcard = _mapper.Map<Yearcard>(importRow);

            Yearcard createdYearcard = await _yearcardService.CreateOrExtendYearcard(yearcard, new StartDate(startDate), false);

            var customer = createdYearcard.User;

            await _userManager.UpdateAsync(customer);
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

        private static string GenerateErrorFileName(string originalFileName)
        {
            var extension = ".csv";
            return Path.GetFileNameWithoutExtension(originalFileName) + "-import-errors" + extension;
        }
    }
}
