using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GarageGroup.Infra;
using Microsoft.Extensions.Logging;

namespace GarageGroup.Platform.Ping;

partial class PingHandler
{
    public async ValueTask<Result<Unit, Failure<HandlerFailureCode>>> HandleAsync(
        PingIn? input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input?.HealthCheckUrl))
        {
            return Failure.Create(HandlerFailureCode.Persistent, "HealthCheckUrl must be specified.");
        }

        var retryDelayInSeconds = input.RetryDelayInSeconds > 0 ? input.RetryDelayInSeconds.Value : DefaultRetryDelayInSeconds;
        var maxAttempts = input.MaxAttempts > 0 ? input.MaxAttempts.Value : DefaultMaxAttempts;

        for (var i = 0; i < maxAttempts; i++)
        {
            logger.LogInformation("Attempt {attemmpt} of {maxAttempts}.", i + 1, maxAttempts);
            var result = await InnerPingAsync(input, cancellationToken);

            if (result.IsSuccess)
            {
                return Result.Success<Unit>(default);
            }

            var failure = result.FailureOrThrow();
            logger.LogError(failure.SourceException, "{failureMessage}", failure.FailureMessage);

            logger.LogInformation("Delay {delay}s.", retryDelayInSeconds);
            await Task.Delay(retryDelayInSeconds * 1000, cancellationToken);
        }

        return Failure.Create(HandlerFailureCode.Transient, "All attempts were unsuccessful.");
    }

    private ValueTask<Result<Unit, Failure<Unit>>> InnerPingAsync(PingIn input, CancellationToken cancellationToken)
        =>
        AsyncPipeline.Pipe(
            input, cancellationToken)
        .Pipe(
            static @in => new HttpSendIn(HttpVerb.Get, @in.HealthCheckUrl.OrEmpty())
            {
                Headers = @in.Headers.SplitToFlatArray(';', HeadersSplitOptions).Map(ParseHeaderValue)
            })
        .PipeValue(
            httpApi.SendAsync)
        .Map(
            static success => (success.Body.Content?.ToString()).OrEmpty(),
            static failure => failure.ToStandardFailure("An unsuccessful HTTP request:").WithFailureCode<Unit>(default))
        .OnSuccess(
            response => logger.LogInformation("{body}", response))
        .Forward(
            response => VerifyResponse(response, input));

    private static KeyValuePair<string, string> ParseHeaderValue(string source)
    {
        var parts = source.Split(':', 2);
        return new(parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }

    private static Result<Unit, Failure<Unit>> VerifyResponse(string response, PingIn input)
    {
        if (string.IsNullOrWhiteSpace(input.ContainedMessage))
        {
            return Result.Success<Unit>(default);
        }

        if (input.IsEndOfLineMatch is not true)
        {
            if (response.Contains(input.ContainedMessage, StringComparison.InvariantCultureIgnoreCase))
            {
                return Result.Success<Unit>(default);
            }

            return Failure.Create($"Response does not contain required message '{input.ContainedMessage}'.");
        }

        var pattern = Regex.Escape(input.ContainedMessage) + "$";
        if (Regex.IsMatch(response, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return Result.Success<Unit>(default);
        }

        return Failure.Create($"Response does not contain strict required message '{input.ContainedMessage}'.");
    }
}