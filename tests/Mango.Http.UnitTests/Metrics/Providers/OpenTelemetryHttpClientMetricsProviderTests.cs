// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Metrics
{
    using FluentAssertions;
    using Mango.Http.Metrics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Metrics;
    using System.Linq;

    public class OpenTelemetryHttpClientMetricsProviderTests : IDisposable
    {
        private readonly MeterListener _listener;
        private readonly List<Measurement<long>> _requestMeasurements = new();
        private readonly List<Measurement<double>> _durationMeasurements = new();

        public OpenTelemetryHttpClientMetricsProviderTests()
        {
            _listener = new MeterListener();
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == "Mango.Http.Client")
                {
                    listener.EnableMeasurementEvents(instrument);
                }
            };
            _listener.SetMeasurementEventCallback<long>((inst, measurement, tags, state) =>
            {
                _requestMeasurements.Add(new Measurement<long>(measurement, tags.ToArray().ToDictionary(kv => kv.Key, kv => kv.Value)));
            });
            _listener.SetMeasurementEventCallback<double>((inst, measurement, tags, state) =>
            {
                _durationMeasurements.Add(new Measurement<double>(measurement, tags.ToArray().ToDictionary(kv => kv.Key, kv => kv.Value)));
            });
            _listener.Start();
        }

        public void Dispose()
        {
            _listener.Dispose();
        }

        [Fact]
        public void RecordRequest_ShouldRecordCountOne_WithCorrectTags()
        {
            // Arrange
            var provider = new OpenTelemetryHttpClientMetricsProvider();
            var clientName = "clientX";
            var method = HttpMethod.Post;
            string[] tags = { "tag1", "tag2" };

            // Act
            provider.RecordRequest(clientName, method, tags);

            // Give listener time
            System.Threading.Thread.Sleep(50);

            // Assert
            _requestMeasurements.Should().ContainSingle();
            var measurement = _requestMeasurements.Single();
            measurement.Value.Should().Be(1);
            measurement.Tags.Should().ContainKey("client").WhoseValue.Should().Be(clientName);
            measurement.Tags.Should().ContainKey("method").WhoseValue.Should().Be(method.Method);
            measurement.Tags.Should().ContainKey("tag1").WhoseValue.Should().Be("true");
            measurement.Tags.Should().ContainKey("tag2").WhoseValue.Should().Be("true");
        }

        [Fact]
        public void RecordDuration_ShouldRecordHistogram_WithDurationAndStatusAndTags()
        {
            // Arrange
            var provider = new OpenTelemetryHttpClientMetricsProvider();
            var clientName = "clientY";
            var method = HttpMethod.Get;
            var duration = TimeSpan.FromMilliseconds(123);
            var status = 404;
            string[] tags = { "x1" };

            // Act
            provider.RecordDuration(clientName, method, duration, status, tags);
            System.Threading.Thread.Sleep(50);

            // Assert
            _durationMeasurements.Should().ContainSingle();
            var measurement = _durationMeasurements.Single();
            measurement.Value.Should().BeApproximately(duration.TotalMilliseconds, 0.1);
            measurement.Tags.Should().ContainKey("client").WhoseValue.Should().Be(clientName);
            measurement.Tags.Should().ContainKey("method").WhoseValue.Should().Be(method.Method);
            measurement.Tags.Should().ContainKey("x1").WhoseValue.Should().Be("true");
            measurement.Tags.Should().ContainKey("http.status_code").WhoseValue.Should().Be(status);
        }

        [Fact]
        public void RecordFailure_ShouldRecordOneCount_WithErrorTag()
        {
            // Arrange
            var provider = new OpenTelemetryHttpClientMetricsProvider();
            var clientName = "clientZ";
            var method = HttpMethod.Delete;
            var ex = new InvalidOperationException();
            string[] tags = { "t3" };

            // Act
            provider.RecordFailure(clientName, method, ex, tags);
            System.Threading.Thread.Sleep(50);

            // Assert
            _requestMeasurements.Should().ContainSingle();
            var measurement = _requestMeasurements.Single();
            measurement.Value.Should().Be(1);
            measurement.Tags.Should().ContainKey("client").WhoseValue.Should().Be(clientName);
            measurement.Tags.Should().ContainKey("method").WhoseValue.Should().Be(method.Method);
            measurement.Tags.Should().ContainKey("t3").WhoseValue.Should().Be("true");
            measurement.Tags.Should().ContainKey("error").WhoseValue.Should().Be(ex.GetType().Name);
        }
    }

    internal record Measurement<T>(T Value, Dictionary<string, object?> Tags);
}
