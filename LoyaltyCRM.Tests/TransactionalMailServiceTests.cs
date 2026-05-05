using System.Net;
using System.Text.Json;
using LoyaltyCRM.Services;
using LoyaltyCRM.Services.Services;
using LoyaltyCRM.Services.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace LoyaltyCRM.Tests
{
    public class TransactionalMailServiceTests
    {
        private readonly IAppSettingsProvider _config;
        private readonly TestHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly TransactionalMailService _sut;

        public TransactionalMailServiceTests()
        {
            _config = new TestAppSettingsProvider();

            _httpMessageHandler = new TestHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("https://mandrillapp.com/api/1.0/")
            };

            _sut = new TransactionalMailService(_httpClient, _config);
        }

        private class TestAppSettingsProvider : IAppSettingsProvider
        {
            public AppSettings Current => new AppSettings { MandrillApiKey = "test-api-key" };
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
        public async Task PingAsync_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\": \"ok\"}")
            };

            // Act
            var result = await _sut.PingAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PingAsync_ShouldReturnFalse_WhenUnsuccessful()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

            // Act
            var result = await _sut.PingAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_ShouldReturnTrue_WhenSuccessful()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[{\"status\": \"sent\"}]")
            };
            var variables = new Dictionary<string, string> { { "key", "value" } };

            // Act
            var result = await _sut.SendTemplateEmailAsync("template", "to@example.com", "from@example.com", variables);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task SendTemplateEmailAsync_ShouldThrowException_WhenUnsuccessful()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Error message")
            };
            var variables = new Dictionary<string, string> { { "key", "value" } };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.SendTemplateEmailAsync("template", "to@example.com", "from@example.com", variables));
        }

        [Fact]
        public async Task GetTemplatesAsync_ShouldReturnTemplateNames_WhenSuccessful()
        {
            // Arrange
            var templates = new List<TransactionalMailService.MandrillTemplate>
            {
                new() { Name = "Template1" },
                new() { Name = "Template2" }
            };
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(templates))
            };

            // Act
            var result = await _sut.GetTemplatesAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("Template1", result);
            Assert.Contains("Template2", result);
        }

        [Fact]
        public async Task GetTemplatesAsync_ShouldThrowException_WhenUnsuccessful()
        {
            // Arrange
            _httpMessageHandler.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Error")
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetTemplatesAsync());
        }
    }
}
