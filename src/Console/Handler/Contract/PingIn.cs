using System;

namespace GarageGroup.Platform.Ping;

internal sealed record class PingIn
{
    public Uri? HealthCheckUrl { get; init; }

    public string? ContainedMessage { get; init; }

    public int? RetryDelayInSeconds { get; init; }

    public int? MaxAttempts { get; init; }
}