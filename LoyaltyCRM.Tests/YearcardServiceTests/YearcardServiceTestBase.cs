using LoyaltyCRM.Api.Mapping;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Infrastructure.Context;
using LoyaltyCRM.Services;
using LoyaltyCRM.Services.Repositories.Interfaces;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

public abstract class YearcardServiceTestBase : IDisposable
{
    protected readonly Mock<IYearcardRepo> _yearcardRepoMock;
    protected readonly Mock<ICustomerRepo> _customerRepoMock;
    protected readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    protected readonly Mock<ILogger<YearcardService>> _loggerMock;
    protected readonly Mock<LoyaltyContext> _contextMock;
    protected readonly Mock<ITransactionService> _transactionMock;

    protected readonly Mock<IAppSettingsProvider> _settingsMock;
    protected readonly Mock<IAudienceSyncService> _audienceSyncMock;

    protected readonly YearcardService _sut;

    protected YearcardServiceTestBase()
    {
        _yearcardRepoMock = new Mock<IYearcardRepo>();
        _customerRepoMock = new Mock<ICustomerRepo>();
        _loggerMock = new Mock<ILogger<YearcardService>>();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _contextMock = new Mock<LoyaltyContext>();
        _transactionMock = new Mock<ITransactionService>();
        _settingsMock = new Mock<IAppSettingsProvider>();
        _settingsMock
            .Setup(s => s.Current)
            .Returns(new AppSettings());
        _audienceSyncMock = new Mock<IAudienceSyncService>();

        MapsterConfig.RegisterMappings();

        _sut = new YearcardService(
            _yearcardRepoMock.Object,
            _customerRepoMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object,
            _transactionMock.Object,
            _settingsMock.Object,
            _audienceSyncMock.Object
        );
    }

    public virtual void Dispose()
    {
        // shared cleanup
    }
}