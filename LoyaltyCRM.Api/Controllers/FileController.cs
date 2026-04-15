using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LoyaltyCRM.Authorization;
using LoyaltyCRM.Domain.Enums;
using LoyaltyCRM.DTOs.Dtos.FileImport;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltyCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileImportService _fileImportService;

        public FileController(IFileImportService fileImportService)
        {
            _fileImportService = fileImportService;
        }

        [HttpPost("preview")]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<ImportPreviewResponse>> Preview(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("A file must be provided for preview.");
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _fileImportService.PreviewHeadersAsync(stream, file.FileName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("import")]
        [RequireRole(Role.Papa)]
        public async Task<ActionResult<ImportResultDto>> Import([FromForm] ImportFileCommand request)
        {
            if (request.File == null)
            {
                return BadRequest(new { message = "A file must be provided for import." });
            }

            if (string.IsNullOrWhiteSpace(request.ColumnMappingJson))
            {
                return BadRequest(new { message = "Column mapping must be provided." });
            }

            Dictionary<string, string>? mapping;
            try
            {
                mapping = JsonSerializer.Deserialize<Dictionary<string, string>>(request.ColumnMappingJson);
            }
            catch
            {
                return BadRequest(new { message = "Column mapping JSON is invalid." });
            }

            if (mapping == null || mapping.Count == 0)
            {
                return BadRequest(new { message = "Column mapping cannot be empty." });
            }

            try
            {
                using var stream = request.File.OpenReadStream();
                var startDate = string.IsNullOrWhiteSpace(request.StartDate)
                    ? DateTime.Today
                    : DateTime.TryParse(request.StartDate, out var parsed) ? parsed : DateTime.Today;

                var result = await _fileImportService.ImportAsync(stream, request.File.FileName, mapping, startDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        public class ImportFileCommand
        {
            public IFormFile? File { get; set; }
            public string? ColumnMappingJson { get; set; }
            public string? StartDate { get; set; }
        }
    }
}
