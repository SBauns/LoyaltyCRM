using System.Threading.Tasks;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IAppSettingsProvider
    {
        AppSettings Current { get; }
        Task InitializeAsync();
        Task ReloadAsync();
    }
}
