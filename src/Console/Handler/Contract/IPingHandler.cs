using System;
using GarageGroup.Infra;

namespace GarageGroup.Platform.Ping;

internal interface IPingHandler : IHandler<PingIn, Unit>;