// ReSharper disable once CheckNamespace
namespace Mango.Http.UnitTests.Logging
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics;

    public class MangoHttpTracingExtensionsTests
    {
        [Fact]
        public void AddMangoHttpTracing_WithoutExistingSource_RegistersDefaultActivitySource()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returned = services.AddMangoHttpTracing();
            var sp = services.BuildServiceProvider();
            var source = sp.GetService<ActivitySource>();

            // Assert
            returned.Should().BeSameAs(services);
            source.Should().NotBeNull();
            source.Name.Should().Be("MangoHttp");
        }

        [Fact]
        public void AddMangoHttpTracing_WithExistingSource_UsesExistingActivitySource()
        {
            // Arrange
            var services = new ServiceCollection();
            var customSource = new ActivitySource("CustomSource");
            services.AddSingleton<ActivitySource>(customSource);

            // Act
            services.AddMangoHttpTracing();
            var sp = services.BuildServiceProvider();
            var resolved = sp.GetService<ActivitySource>();

            // Assert
            resolved.Should().BeSameAs(customSource);
        }

        [Fact]
        public void AddMangoHttpTracing_MultipleCalls_OnlyOneDescriptorRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddMangoHttpTracing();
            services.AddMangoHttpTracing();

            // Assert
            var descriptors = services.Where(sd => sd.ServiceType == typeof(ActivitySource)).ToList();
            descriptors.Should().HaveCount(1);
        }
    }
}
