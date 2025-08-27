// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public class DefaultHttpLoggerTests
    {
        private const string ClientName = "clientTest";
        private readonly Mock<ILogger<DefaultHttpLogger>> _loggerMock;
        private readonly Mock<IOptionsMonitor<HttpLoggingOptions>> _optionsMonitorMock;
        private readonly DefaultHttpLogger _defaultLogger;
        private HttpLoggingOptions _options;

        public DefaultHttpLoggerTests()
        {
            _loggerMock = new Mock<ILogger<DefaultHttpLogger>>();
            // Mock IsEnabled for all log levels to return true
            _loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _optionsMonitorMock = new Mock<IOptionsMonitor<HttpLoggingOptions>>();
            _options = new HttpLoggingOptions
            {
                Enabled = true,
                ExcludedHeaders = new HashSet<string>(),
                LogRequestBody = true,
                LogResponseBody = true,
                MaxBodyLength = 5,
                RequestLevel = LogLevel.Information,
                ResponseSuccessLevel = LogLevel.Warning,
                ErrorLevel = LogLevel.Error
            };
            _optionsMonitorMock.Setup(m => m.Get(ClientName)).Returns(_options);
            _defaultLogger = new DefaultHttpLogger(_loggerMock.Object, _optionsMonitorMock.Object, ClientName);
        }

        [Fact]
        public void Constructor_MissingOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var optMock = new Mock<IOptionsMonitor<HttpLoggingOptions>>();
            optMock.Setup(m => m.Get(It.IsAny<string>())).Returns((HttpLoggingOptions)null!);

            // Act
            var act = () => new DefaultHttpLogger(_loggerMock.Object, optMock.Object, ClientName);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithMessage("HttpLoggingOptions not found for client: clientTest*");
        }

        [Fact]
        public async Task LogRequestAsync_Disabled_DoesNotLog()
        {
            // Arrange
            _options.Enabled = false;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");

            // Act
            await _defaultLogger.LogRequestAsync(request);

            // Assert
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LogRequestAsync_WithBody_TruncatesAndLogs()
        {
            // Arrange
            var body = "ABCDEFG"; // length 7 > MaxBodyLength 5
            var request = new HttpRequestMessage(HttpMethod.Post, "http://api/data")
            {
                Content = new StringContent(body, Encoding.UTF8, "text/plain")
            };
            request.Headers.Add("X-Test", "123");

            // Act
            await _defaultLogger.LogRequestAsync(request);

            // Read content after logging to ensure rewind
            var loggedBody = await request.Content.ReadAsStringAsync();

            // Assert: body truncated to 5 chars + '...'
            loggedBody.Should().Be(body);
            // Logger should be called once with Information level
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task LogResponseAsync_Disabled_DoesNotLog()
        {
            // Arrange
            _options.Enabled = false;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            await _defaultLogger.LogResponseAsync(request, response, TimeSpan.FromMilliseconds(10));

            // Assert
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LogResponseAsync_WithBody_TruncatesAndLogs()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("ResponseData", Encoding.UTF8, "text/plain") // length > 5
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://api");
            request.Headers.Add("H", "V");

            // Act
            await _defaultLogger.LogResponseAsync(request, response, TimeSpan.FromMilliseconds(20));
            var loggedBody = await response.Content.ReadAsStringAsync();

            // Assert
            loggedBody.Should().Be("ResponseData");
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task LogErrorAsync_Disabled_DoesNotLog()
        {
            // Arrange
            _options.Enabled = false;
            var request = new HttpRequestMessage(HttpMethod.Get, "http://error");
            var ex = new InvalidOperationException("fail");

            // Act
            await _defaultLogger.LogErrorAsync(request, ex, TimeSpan.FromMilliseconds(15));

            // Assert
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task LogErrorAsync_Enabled_LogsError()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Put, "http://error");
            var ex = new InvalidOperationException("fail");

            // Act
            await _defaultLogger.LogErrorAsync(request, ex, TimeSpan.FromMilliseconds(15));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    ex,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
