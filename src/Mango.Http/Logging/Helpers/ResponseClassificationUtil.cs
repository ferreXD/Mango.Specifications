// ReSharper disable once CheckNamespace
namespace Mango.Http.Logging
{
    using Microsoft.Extensions.Logging;
    using System.Net;

    internal static class ResponseClassificationUtil
    {
        internal static LogLevel Classify(HttpResponseMessage resp, HttpLoggingOptions options) =>
            options.CustomClassifier?.Invoke(resp)
            ?? ((int)resp.StatusCode >= 500 && options.Treat5xxAsError ? options.ResponseServerErrorLevel
                : (int)resp.StatusCode >= 400 && options.Treat4xxAsError
                    ? (options.Treat404AsInfo && resp.StatusCode == HttpStatusCode.NotFound
                        ? options.ResponseSuccessLevel
                        : options.ResponseClientErrorLevel)
                    : options.ResponseSuccessLevel);
    }
}
