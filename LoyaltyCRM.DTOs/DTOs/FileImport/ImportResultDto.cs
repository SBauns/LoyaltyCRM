namespace LoyaltyCRM.DTOs.Dtos.FileImport
{
    public class ImportResultDto
    {
        public bool Success { get; set; }
        public int CreatedCount { get; set; }
        public int FailedCount { get; set; }
        public string? ErrorFileName { get; set; }
        public string? ErrorFileBase64 { get; set; }
        public string? Message { get; set; }
    }
}
