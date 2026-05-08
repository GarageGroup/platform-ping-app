using System;
using GarageGroup.Infra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Platform.Ping;

internal sealed partial class PingHandler : IPingHandler
{
    public static PingHandler Resolve(IServiceProvider serviceProvider, IHttpApi httpApi)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(httpApi);

        return new(
            httpApi: httpApi,
            logger: serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PingHandler)));
    }

    private const int DefaultRetryDelayInSeconds = 5;

    private const int DefaultMaxAttempts = 3;

    private const StringSplitOptions HeadersSplitOptions
        =
        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

    private readonly IHttpApi httpApi;

    private readonly ILogger logger;

    private PingHandler(IHttpApi httpApi, ILogger logger)
    {
        this.httpApi = httpApi;
        this.logger = logger;
    }
}