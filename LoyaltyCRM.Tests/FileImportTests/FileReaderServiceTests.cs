using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Xunit;

namespace LoyaltyCRM.Tests.FileImportTests;

public class FileReaderServiceTests
{
    [Fact]
    public async Task ReadRowsAsync_ShouldReturnRowsForCsv()
    {
        var csv = "Kortnummer,Gyldig til,Telefon,Email,Navn\n123,2026-12-31,+45-12345678,test@example.com,Test Navn\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        IFileReaderService sut = new FileReaderService();
        var rows = await sut.ReadRowsAsync(stream, "import.csv");

        rows.Should().HaveCount(1);
        rows[0].Should().ContainKey("Kortnummer");
        rows[0]["Kortnummer"].Should().Be("123");
        rows[0]["Navn"].Should().Be("Test Navn");
    }

    [Fact]
    public async Task GetHeadersAsync_ShouldReturnHeaderNamesForCsv()
    {
        var csv = "Kortnummer,Gyldig til,Telefon,Email,Navn\n123,2026-12-31,+45-12345678,test@example.com,Test Navn\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        IFileReaderService sut = new FileReaderService();
        var headers = await sut.GetHeadersAsync(stream, "import.csv");

        headers.Should().BeEquivalentTo(new[] { "Kortnummer", "Gyldig til", "Telefon", "Email", "Navn" });
    }
}
