using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Platform.Ping;

internal sealed partial class PingHandler : IPingHandler
{
    public static PingHandler Resolve(IServiceProvider serviceProvider, HttpMessageHandler httpMessageHandler)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(httpMessageHandler);

        return new(
            httpMessageHandler: httpMessageHandler,
            logger: serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<PingHandler>());
    }

    private const int DefaultRetryDelayInSeconds = 5;

    private const int DefaultMaxAttempts = 3;

    private readonly HttpMessageHandler httpMessageHandler;

    private readonly ILogger logger;

    private PingHandler(HttpMessageHandler httpMessageHandler, ILogger logger)
    {
        this.httpMessageHandler = httpMessageHandler;
        this.logger = logger;
    }
}