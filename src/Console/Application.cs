using System;
using GarageGroup.Infra;
using PrimeFuncPack;

namespace GarageGroup.Platform.Ping;

internal static class Application
{
    internal static Dependency<IHandler<PingIn, Unit>> UsePingHandler()
        =>
        PrimaryHandler.UseStandardSocketsHttpHandler()
        .UseLogging("PingHandler")
        .Map<IHandler<PingIn, Unit>>(PingHandler.Resolve);
}