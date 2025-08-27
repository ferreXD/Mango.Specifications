// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Mango.Http.Logging;
    using Moq;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class HttpLoggingHandlerTests
    {
        private const string TestUrl = "http://test/logging";

        private class FakeSuccessHandler(HttpResponseMessage response) : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(response);
        }

        private class FakeFailureHandler(Exception exception) : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw exception;
        }

        private static async Task<HttpResponseMessage> InvokeHandlerAsync(DelegatingHandler handler)
        {
            using var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, TestUrl);
            return await invoker.SendAsync(request, CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_Success_InvokesLogRequestAndResponse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var fakeInner = new FakeSuccessHandler(response);

            var logRequestCalled = false;
            var logResponseCalled = false;
            var capturedElapsed = TimeSpan.Zero;

            var loggerMock = new Mock<IMangoHttpLogger>(MockBehavior.Strict);
            loggerMock
                .Setup(l => l.LogRequestAsync(It.IsAny<HttpRequestMessage>()))
                .Callback(() => logRequestCalled = true)
                .Returns(Task.CompletedTask);
            loggerMock
                .Setup(l => l.LogResponseAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<HttpResponseMessage>(),
                    It.IsAny<TimeSpan>()))
                .Callback<HttpRequestMessage, HttpResponseMessage, TimeSpan>((req, resp, elapsed) =>
                {
                    logResponseCalled = true;
                    capturedElapsed = elapsed;
                })
                .Returns(Task.CompletedTask);
            // No errors expected
            loggerMock.Setup(l => l.LogErrorAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<Exception>(), It.IsAny<TimeSpan>()));

            var handler = new HttpLoggingHandler(loggerMock.Object)
            {
                InnerHandler = fakeInner
            };

            // Act
            var result = await InvokeHandlerAsync(handler);

            // Assert
            result.Should().BeSameAs(response);
            logRequestCalled.Should().BeTrue();
            logResponseCalled.Should().BeTrue();
            capturedElapsed.Should().BeGreaterThan(TimeSpan.Zero);

            loggerMock.Verify(l => l.LogRequestAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            loggerMock.Verify(l => l.LogResponseAsync(
                It.IsAny<HttpRequestMessage>(), It.IsAny<HttpResponseMessage>(), It.IsAny<TimeSpan>()), Times.Once);
            loggerMock.Verify(l => l.LogErrorAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<Exception>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_Error_InvokesLogRequestAndErrorAndRethrows()
        {
            // Arrange
            var exception = new InvalidOperationException("boom");
            var fakeInner = new FakeFailureHandler(exception);

            var logRequestCalled = false;
            var logErrorCalled = false;
            var capturedElapsed = TimeSpan.Zero;

            var loggerMock = new Mock<IMangoHttpLogger>(MockBehavior.Strict);
            loggerMock
                .Setup(l => l.LogRequestAsync(It.IsAny<HttpRequestMessage>()))
                .Callback(() => logRequestCalled = true)
                .Returns(Task.CompletedTask);
            // No response logging expected
            loggerMock.Setup(l => l.LogResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpResponseMessage>(), It.IsAny<TimeSpan>()));
            loggerMock
                .Setup(l => l.LogErrorAsync(
                    It.IsAny<HttpRequestMessage>(),
                    It.IsAny<Exception>(),
                    It.IsAny<TimeSpan>()))
                .Callback<HttpRequestMessage, Exception, TimeSpan>((req, ex, elapsed) =>
                {
                    logErrorCalled = true;
                    capturedElapsed = elapsed;
                })
                .Returns(Task.CompletedTask);

            var handler = new HttpLoggingHandler(loggerMock.Object)
            {
                InnerHandler = fakeInner
            };

            // Act
            Func<Task> act = () => InvokeHandlerAsync(handler);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            logRequestCalled.Should().BeTrue();
            logErrorCalled.Should().BeTrue();
            capturedElapsed.Should().BeGreaterThan(TimeSpan.Zero);

            loggerMock.Verify(l => l.LogRequestAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            loggerMock.Verify(l => l.LogErrorAsync(
                It.IsAny<HttpRequestMessage>(), It.Is<Exception>(e => e == exception), It.IsAny<TimeSpan>()), Times.Once);
            loggerMock.Verify(l => l.LogResponseAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpResponseMessage>(), It.IsAny<TimeSpan>()), Times.Never);
        }
    }
}
