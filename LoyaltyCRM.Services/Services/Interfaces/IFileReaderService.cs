using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IFileReaderService
    {
        Task<IReadOnlyList<IDictionary<string, string>>> ReadRowsAsync(Stream fileStream, string fileName);
        Task<IReadOnlyList<string>> GetHeadersAsync(Stream fileStream, string fileName);
    }
}
