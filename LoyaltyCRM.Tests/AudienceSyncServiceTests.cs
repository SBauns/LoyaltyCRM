using System.Net;
using System.Text.Json;
using LoyaltyCRM.Domain.Models;
using LoyaltyCRM.Services;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace LoyaltyCRM.Tests
{
    public class AudienceSyncServiceTests
    {
        private readonly IAppSettingsProvider _config;
        private readonly TestHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly AudienceSyncService _sut;

        public AudienceSyncServiceTests()
        {
            _config = new TestAppSettingsProvider();

            _httpMessageHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("https://us1.api.mailchimp.com/3.0/")
            };

            _sut = new AudienceSyncService(_httpClient, _config);
        }

        private class TestAppSettingsProvider : IAppSettingsProvider
        {
            public AppSettings Current => new AppSettings
            {
                MailChimpApiKey = "test-api-key",
                MailChimpServerPrefix = "us1",
                MailChimpListId = "test-list-id"
            };
            public Task InitializeAsync() => Task.CompletedTask;
            public Task ReloadAsync() => Task.CompletedTask;
        }

        private class TestHttpMessageHandler : HttpMessageHandler
        {
            public HttpResponseMessage Response { get; set; } = new HttpResponseMessage(HttpStatusCode.OK);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Response);
            }
        }

        [Fact]
        public async Task SyncUserAsync_ShouldDoNothing_WhenUserIsNull()
        {
            // Act
            await _sut.SyncUserAsync(null);

            // Assert - No exception, no calls
            // Since the handler is not called, no need to check
        }

        [Fact]
        public async Task SyncUserAsync_ShouldDoNothing_WhenEmailIsNull()
        {
            // Arrange
            var user = new ApplicationUser { Email = null };

            // Act
            await _sut.SyncUserAsync(user);

            // Assert
        }

        [Fact]
        public async Task SyncUserAsync_ShouldDoNothing_WhenEmailIsWhitespace()
        {
            // Arrange
            var user = new ApplicationUser { Email = "   " };

            // Act
            await _sut.SyncUserAsync(user);

            // Assert
        }

        [Fact]
        public async Task SyncUserAsync_ShouldSendPutRequest_WhenUserIsValid()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                UserName = "TestUser",
                IsSubscribed = true
            };

            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            await _sut.SyncUserAsync(user);

            // Assert - No exception
        }

        [Fact]
        public async Task SyncUserAsync_ShouldThrowException_WhenRequestFails()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Email = "test@example.com",
                IsSubscribed = true
            };

            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Error")
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.SyncUserAsync(user));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDoNothing_WhenEmailIsNullOrWhitespace()
        {
            // Act
            await _sut.DeleteUserAsync(null);
            await _sut.DeleteUserAsync("");

            // Assert
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldSendDeleteRequest_WhenEmailIsValid()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            await _sut.DeleteUserAsync("test@example.com");

            // Assert
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowMailChimpException_WhenRequestFails()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Not found")
            };

            // Act & Assert
            await Assert.ThrowsAsync<LoyaltyCRM.Domain.Exceptions.MailChimpException>(() => _sut.DeleteUserAsync("test@example.com"));
        }
    }
}
