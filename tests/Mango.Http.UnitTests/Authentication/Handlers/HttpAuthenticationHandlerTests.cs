// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Authentication
{
    using Authorization;
    using FluentAssertions;
    using Moq;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class HttpAuthenticationHandlerTests
    {
        private class TestHandler(HttpResponseMessage response) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(response);
        }

        private static async Task<HttpResponseMessage> InvokeHandlerAsync(DelegatingHandler handler, HttpRequestMessage request)
        {
            using var invoker = new HttpMessageInvoker(handler);
            return await invoker.SendAsync(request, CancellationToken.None);
        }

        [Fact]
        public void Ctor_NullStrategy_ThrowsArgumentNullException()
        {
            // Arrange
            var opts = new HttpAuthOptions { Enabled = true };

            // Act
            Action act = () => new HttpAuthenticationHandler(opts, null!);

            // Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .WithParameterName("strategy");
        }

        [Fact]
        public async Task SendAsync_AuthDisabled_DoesNotCallStrategyAndReturnsResponse()
        {
            // Arrange
            var opts = new HttpAuthOptions { Enabled = false };
            var strategyMock = new Mock<IAuthenticationStrategy>();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var inner = new TestHandler(response);
            var handler = new HttpAuthenticationHandler(opts, strategyMock.Object)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "https://test/");

            // Act
            var result = await InvokeHandlerAsync(handler, request);

            // Assert
            result.Should().BeSameAs(response);
            strategyMock.Verify(s => s.ApplyAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SendAsync_AuthEnabled_NoCondition_CallsStrategyAndReturnsResponse()
        {
            // Arrange
            var opts = new HttpAuthOptions { Enabled = true, Condition = null };
            var strategyMock = new Mock<IAuthenticationStrategy>();
            strategyMock
                .Setup(s => s.ApplyAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            var inner = new TestHandler(response);
            var handler = new HttpAuthenticationHandler(opts, strategyMock.Object)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Post, "https://test/");

            // Act
            var result = await InvokeHandlerAsync(handler, request);

            // Assert
            result.Should().BeSameAs(response);
            strategyMock.Verify(
                s => s.ApplyAsync(
                    It.Is<HttpRequestMessage>(r => r == request),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_AuthEnabled_ConditionFalse_DoesNotCallStrategy()
        {
            // Arrange
            var opts = new HttpAuthOptions
            {
                Enabled = true,
                Condition = req => false
            };
            var strategyMock = new Mock<IAuthenticationStrategy>();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var inner = new TestHandler(response);
            var handler = new HttpAuthenticationHandler(opts, strategyMock.Object)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Put, "https://test/");

            // Act
            var result = await InvokeHandlerAsync(handler, request);

            // Assert
            result.Should().BeSameAs(response);
            strategyMock.Verify(s => s.ApplyAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SendAsync_AuthEnabled_ConditionTrue_CallsStrategy()
        {
            // Arrange
            var opts = new HttpAuthOptions
            {
                Enabled = true,
                Condition = req => true
            };
            var strategyMock = new Mock<IAuthenticationStrategy>();
            strategyMock
                .Setup(s => s.ApplyAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask)
                .Verifiable();

            var response = new HttpResponseMessage(HttpStatusCode.NoContent);
            var inner = new TestHandler(response);
            var handler = new HttpAuthenticationHandler(opts, strategyMock.Object)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Delete, "https://test/");

            // Act
            var result = await InvokeHandlerAsync(handler, request);

            // Assert
            result.Should().BeSameAs(response);
            strategyMock.Verify(
                s => s.ApplyAsync(
                    It.Is<HttpRequestMessage>(r => r == request),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAsync_StrategyThrows_PropagatesException()
        {
            // Arrange
            var opts = new HttpAuthOptions { Enabled = true };
            var strategyMock = new Mock<IAuthenticationStrategy>();
            strategyMock
                .Setup(s => s.ApplyAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new MangoAuthenticationException("fail"));

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var inner = new TestHandler(response);
            var handler = new HttpAuthenticationHandler(opts, strategyMock.Object)
            {
                InnerHandler = inner
            };
            var request = new HttpRequestMessage(HttpMethod.Get, "https://test/");

            // Act
            var act = async () => await InvokeHandlerAsync(handler, request);

            // Assert
            await act.Should().ThrowAsync<MangoAuthenticationException>();
        }
    }
}
