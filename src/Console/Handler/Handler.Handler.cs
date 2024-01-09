using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GarageGroup.Infra;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Platform.Ping;

partial class PingHandler
{
    public async ValueTask<Result<Unit, Failure<HandlerFailureCode>>> HandleAsync(PingIn? input, CancellationToken cancellationToken)
    {
        if (input?.HealthCheckUrl is null)
        {
            return Failure.Create(HandlerFailureCode.Persistent, "HealthCheckUrl must be specified");
        }

        var retryDelayInSeconds = input.RetryDelayInSeconds > 0 ? input.RetryDelayInSeconds.Value : DefaultRetryDelayInSeconds;
        var maxAttempts = input.MaxAttempts > 0 ? input.MaxAttempts.Value : DefaultMaxAttempts;

        for (var i = 0; i < maxAttempts; i++)
        {
            logger.LogInformation("Attempt {attemmpt} of {maxAttempts}", i + 1, maxAttempts);
            var result = await PingAsync(input, cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Result.Success<Unit>(default);
            }

            var failure = result.FailureOrThrow();
            logger.LogError(failure.SourceException, "{failureMessage}", failure.FailureMessage);

            logger.LogInformation("Delay {delay}s", retryDelayInSeconds);
            await Task.Delay(retryDelayInSeconds * 1000, cancellationToken).ConfigureAwait(false);
        }

        return Failure.Create(HandlerFailureCode.Transient, "All attempts were unsuccessful");
    }

    private async ValueTask<Result<Unit, Failure<Unit>>> PingAsync(PingIn input, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = new HttpClient(httpMessageHandler, disposeHandler: false);
            using var httpResponse = await httpClient.GetAsync(input.HealthCheckUrl, cancellationToken).ConfigureAwait(false);

            var httpResponseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("{responseBody}", httpResponseBody);

            if (httpResponse.IsSuccessStatusCode is false)
            {
                return Failure.Create("An unsuccessful HTTP request");
            }

            if (string.IsNullOrWhiteSpace(input.ContainedMessage))
            {
                return Result.Success<Unit>(default);
            }

            if (httpResponseBody.Contains(input.ContainedMessage, StringComparison.InvariantCulture))
            {
                return Result.Success<Unit>(default);
            }

            return Failure.Create($"Response does not contain required message '{input.ContainedMessage}'");
        }
        catch (OperationCanceledException ex)
        {
            return ex.ToFailure("An HTTP request was canceled");
        }
    }
}