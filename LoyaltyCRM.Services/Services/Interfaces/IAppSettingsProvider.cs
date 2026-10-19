using System.Threading.Tasks;

namespace LoyaltyCRM.Services.Services.Interfaces
{
    public interface IAppSettingsProvider
    {
        AppSettings Current { get; }

        /// <summary>
        /// Raised after settings have been successfully reloaded.
        /// Subscribers must not throw.
        /// </summary>
        event EventHandler? SettingsReloaded;
        Task InitializeAsync();
        Task ReloadAsync();
    }
}
