// ReSharper disable once CheckNamespace
namespace Mango.Http.Common
{
    /// <summary>
    /// Specifies the order values for common Mango HTTP delegating handlers in the pipeline.
    /// </summary>
    public enum MangoHttpHandlerOrder
    {
        /// <summary>Default handler order (first in pipeline).</summary>
        Default = 0,
        /// <summary> Activity handler order.</summary>
        ActivityScope = 1,
        /// <summary>Authentication handler order.</summary>
        Authentication = 2,
        /// <summary>Headers handler order.</summary>
        Headers = 3,
        /// <summary>Logging handler order.</summary>
        Logging = 4,
        /// <summary>Metrics handler order.</summary>
        Metrics = 5,
        /// <summary>Resiliency handler order.</summary>
        Resiliency = 6,
        /// <summary>Hooks handler order (last in pipeline).</summary>
        Hooks = 7,
    }
}
