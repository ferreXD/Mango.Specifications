// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Metrics
{
    using FluentAssertions;
    using Mango.Http.Metrics;
    using Moq;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    public class MetricsHandlerTests
    {
        private const string ClientName = "TestClient";
        private static readonly string[] Tags = new[] { "t1", "t2" };

        private class FakeSuccessHandler(HttpResponseMessage response) : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(response);
            }
        }

        private class FakeFailureHandler(Exception exception) : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw exception;
            }
        }

        private static async Task InvokeHandlerAsync(DelegatingHandler handler)
        {
            using var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test");
            await invoker.SendAsync(request, CancellationToken.None);
        }

        [Fact]
        public async Task SendAsync_OnSuccess_ShouldRecordRequestAndDuration()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var successHandler = new FakeSuccessHandler(response);

            var recordRequestCalled = false;
            var recordedClient = string.Empty;
            HttpMethod recordedMethod = null!;
            string[] recordedTags1 = null!;

            var recordDurationCalled = false;
            var recordedDuration = TimeSpan.Zero;
            var recordedStatus = -1;
            string[] recordedTags2 = null!;

            var metricsMock = new Mock<IHttpClientMetricsProvider>(MockBehavior.Strict);
            metricsMock.Setup(m => m.RecordRequest(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string[]>()))
                .Callback<string, HttpMethod, string[]>((c, m, t) =>
                {
                    recordRequestCalled = true;
                    recordedClient = c;
                    recordedMethod = m;
                    recordedTags1 = t;
                });
            metricsMock.Setup(m => m.RecordDuration(
                    It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<int>(), It.IsAny<string[]>()))
                .Callback<string, HttpMethod, TimeSpan, int, string[]>((c, m, d, s, t) =>
                {
                    recordDurationCalled = true;
                    recordedDuration = d;
                    recordedStatus = s;
                    recordedTags2 = t;
                });
            // No failure call expected
            metricsMock.Setup(m => m.RecordFailure(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Exception>(), It.IsAny<string[]>()));

            var handler = new MetricsHandler(metricsMock.Object, ClientName, Tags)
            {
                InnerHandler = successHandler
            };

            // Act
            await InvokeHandlerAsync(handler);

            // Assert
            recordRequestCalled.Should().BeTrue();
            recordedClient.Should().Be(ClientName);
            recordedMethod.Should().Be(HttpMethod.Get);
            recordedTags1.Should().Equal(Tags);

            recordDurationCalled.Should().BeTrue();
            recordedStatus.Should().Be((int)HttpStatusCode.OK);
            recordedTags2.Should().Equal(Tags);
            recordedDuration.Should().BeGreaterThan(TimeSpan.Zero);

            metricsMock.Verify(m => m.RecordRequest(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string[]>()), Times.Once);
            metricsMock.Verify(m => m.RecordDuration(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<int>(), It.IsAny<string[]>()), Times.Once);
            metricsMock.Verify(m => m.RecordFailure(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Exception>(), It.IsAny<string[]>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_OnException_ShouldRecordRequestAndFailure_AndRethrow()
        {
            // Arrange
            var exception = new InvalidOperationException("failure");
            var failureHandler = new FakeFailureHandler(exception);

            var recordRequestCalled = false;
            var recordFailureCalled = false;
            string[] recordedTags = null!;
            var recordedError = string.Empty;

            var metricsMock = new Mock<IHttpClientMetricsProvider>(MockBehavior.Strict);
            metricsMock.Setup(m => m.RecordRequest(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string[]>()));
            metricsMock.Setup(m => m.RecordRequest(ClientName, HttpMethod.Get, Tags))
                .Callback(() => recordRequestCalled = true);

            metricsMock.Setup(m => m.RecordFailure(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<Exception>(), It.IsAny<string[]>()));
            metricsMock.Setup(m => m.RecordFailure(ClientName, HttpMethod.Get, exception, Tags))
                .Callback<string, HttpMethod, Exception, string[]>((c, m, ex, t) =>
                {
                    recordFailureCalled = true;
                    recordedError = ex.GetType().Name;
                    recordedTags = t;
                });

            // No duration expected
            metricsMock.Setup(m => m.RecordDuration(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<int>(), It.IsAny<string[]>()));

            var handler = new MetricsHandler(metricsMock.Object, ClientName, Tags)
            {
                InnerHandler = failureHandler
            };

            // Act
            Func<Task> act = () => InvokeHandlerAsync(handler);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();

            recordRequestCalled.Should().BeTrue();
            recordFailureCalled.Should().BeTrue();
            recordedError.Should().Be(exception.GetType().Name);
            recordedTags.Should().Equal(Tags);

            metricsMock.Verify(m => m.RecordRequest(ClientName, HttpMethod.Get, Tags), Times.Once);
            metricsMock.Verify(m => m.RecordFailure(ClientName, HttpMethod.Get, exception, Tags), Times.Once);
            metricsMock.Verify(m => m.RecordDuration(It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<int>(), It.IsAny<string[]>()), Times.Never);
        }
    }
}
