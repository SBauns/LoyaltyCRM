using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using LoyaltyCRM.Services.Services.Interfaces;

namespace LoyaltyCRM.Services.Services
{
    public class FileReaderService : IFileReaderService
    {
        static FileReaderService()
        {
            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task<IReadOnlyList<IDictionary<string, string>>> ReadRowsAsync(Stream fileStream, string fileName)
        {
            fileStream = await CopyToSeekableStreamAsync(fileStream);
            if (IsCsvFile(fileName))
            {
                return await ReadCsvRowsAsync(fileStream);
            }

            if (IsExcelFile(fileName))
            {
                return await ReadExcelRowsAsync(fileStream);
            }

            throw new InvalidDataException("Unsupported file type. Only CSV and Excel files are supported.");
        }

        public async Task<IReadOnlyList<string>> GetHeadersAsync(Stream fileStream, string fileName)
        {
            fileStream = await CopyToSeekableStreamAsync(fileStream);
            if (IsCsvFile(fileName))
            {
                return await ReadCsvHeadersAsync(fileStream);
            }

            if (IsExcelFile(fileName))
            {
                return await ReadExcelHeadersAsync(fileStream);
            }

            throw new InvalidDataException("Unsupported file type. Only CSV and Excel files are supported.");
        }

        private static bool IsCsvFile(string fileName)
        {
            return fileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsExcelFile(string fileName)
        {
            return fileName.EndsWith(".xlsx", System.StringComparison.OrdinalIgnoreCase)
                || fileName.EndsWith(".xls", System.StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<Stream> CopyToSeekableStreamAsync(Stream source)
        {
            if (source.CanSeek)
            {
                source.Position = 0;
                return source;
            }

            var memoryStream = new MemoryStream();
            await source.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static async Task<List<Dictionary<string, string>>> ReadCsvRowsAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null,
                Delimiter = ","
            });

            await csv.ReadAsync();
            csv.ReadHeader();
            var headers = csv.HeaderRecord?.Select(h => h.Trim()).ToArray() ?? Array.Empty<string>();
            var rows = new List<Dictionary<string, string>>();

            while (await csv.ReadAsync())
            {
                var row = new Dictionary<string, string>();
                bool hasData = false;
                foreach (var header in headers)
                {
                    var value = csv.GetField(header)?.Trim() ?? string.Empty;
                    row[header] = value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        hasData = true;
                    }
                }

                if (hasData)
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        private static async Task<List<string>> ReadCsvHeadersAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true,
                BadDataFound = null,
                HeaderValidated = null,
                MissingFieldFound = null,
                Delimiter = ","
            });

            await csv.ReadAsync();
            csv.ReadHeader();
            return csv.HeaderRecord?.Select(h => h.Trim()).Where(h => !string.IsNullOrWhiteSpace(h)).ToList() ?? new List<string>();
        }

        private static async Task<List<Dictionary<string, string>>> ReadExcelRowsAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var rows = new List<Dictionary<string, string>>();

            if (!reader.Read())
            {
                return rows;
            }

            var headers = Enumerable.Range(0, reader.FieldCount)
                .Select(i => reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty)
                .ToArray();

            while (reader.Read())
            {
                var row = new Dictionary<string, string>();
                bool hasData = false;

                for (var i = 0; i < headers.Length; i++)
                {
                    var value = reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty;
                    row[headers[i]] = value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        hasData = true;
                    }
                }

                if (hasData)
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        private static async Task<List<string>> ReadExcelHeadersAsync(Stream stream)
        {
            stream.Position = 0;
            using var reader = ExcelReaderFactory.CreateReader(stream);
            if (!reader.Read())
            {
                return new List<string>();
            }

            return Enumerable.Range(0, reader.FieldCount)
                .Select(i => reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList();
        }
    }
}
