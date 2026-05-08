namespace GarageGroup.Platform.Ping;

internal sealed record class PingIn
{
    public string? HealthCheckUrl { get; init; }

    public string? Headers { get; init; }

    public string? ContainedMessage { get; init; }

    public bool? IsEndOfLineMatch { get; init; }

    public int? RetryDelayInSeconds { get; init; }

    public int? MaxAttempts { get; init; }
}