using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LoyaltyCRM.DTOs.Dtos.FileImport;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IFileImportService
    {
        Task<ImportPreviewResponse> PreviewHeadersAsync(Stream fileStream, string fileName);
        Task<ImportResultDto> ImportAsync(Stream fileStream, string fileName, Dictionary<string, string> columnMapping, DateTime startDate);
    }
}
