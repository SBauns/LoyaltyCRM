using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LoyaltyCRM.Api.Mapping;
using LoyaltyCRM.Domain.DomainPrimitives;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.DTOs.Dtos.FileImport;
using LoyaltyCRM.DTOs.Requests.Yearcard;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LoyaltyCRM.Tests.FileImportTests;

public class FileImportServiceTests
{
    private readonly Mock<IFileReaderService> _fileReaderServiceMock;
    private readonly Mock<IYearcardService> _yearcardServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<FileImportService>> _loggerMock;
    private readonly FileImportService _sut;

    public FileImportServiceTests()
    {
        _fileReaderServiceMock = new Mock<IFileReaderService>();
        _yearcardServiceMock = new Mock<IYearcardService>();
        _loggerMock = new Mock<ILogger<FileImportService>>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        MapsterConfig.RegisterMappings();

        _sut = new FileImportService(
            _fileReaderServiceMock.Object,
            _yearcardServiceMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ImportAsync_ShouldCreateYearcard_WhenRowIsValid()
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Kortnummer"] = "100",
            ["Gyldig til"] = "2026-12-31",
            ["Telefon"] = "+45-12345678",
            ["Email"] = "test@example.com",
            ["Navn"] = "Test Navn"
        };

        _fileReaderServiceMock
            .Setup(x => x.ReadRowsAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { row });

        _yearcardServiceMock
            .Setup(x => x.ImportYearcard(It.IsAny<YearcardImportRequest>()))
            .ReturnsAsync(new YearcardCreateResponse
            {
                Id = Guid.NewGuid(),
                CardId = 100,
                Name = "Test Navn",
                Email = "test@example.com",
                PhoneNumber = "+45-12345678",
                UserName = null
            });

        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CardId"] = "Kortnummer",
            ["ValidTo"] = "Gyldig til",
            ["PhoneNumber"] = "Telefon",
            ["Email"] = "Email",
            ["Name"] = "Navn"
        };

        var result = await _sut.ImportAsync(new MemoryStream(), "import.csv", mapping, DateTime.Today);
        Console.WriteLine(result.Message);

        result.Success.Should().BeTrue();
        result.CreatedCount.Should().Be(1);
        result.FailedCount.Should().Be(0);
        result.ErrorFileBase64.Should().BeNull();
    }

    [Fact]
    public async Task ImportAsync_ShouldReturnErrorReport_WhenMappingIsInvalid() //TODO THIS TEST NEED TO TEST SOMETHING
    {
        var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Kortnummer"] = "100",
            ["Gyldig til"] = "",
            ["Telefon"] = "Not a Phone",
            ["Email"] = "test@example.com",
            ["Navn"] = "Test Navn"
        };

        _fileReaderServiceMock
            .Setup(x => x.ReadRowsAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { row });

        _yearcardServiceMock
            .Setup(x => x.ImportYearcard(It.IsAny<YearcardImportRequest>()))
            .Throws(new ArgumentException());

        var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CardId"] = "Kortnummer",
            ["ValidTo"] = "Telefon",
            ["PhoneNumber"] = "Gyldig til",
            ["Email"] = "Email",
            ["Name"] = "Navn"
        };

        var result = await _sut.ImportAsync(new MemoryStream(), "import.csv", mapping, DateTime.Today);

        result.Success.Should().BeFalse();
    }
}
