FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app
COPY ./publish ./

ENTRYPOINT ["dotnet", "/app/GarageGroup.Platform.Ping.Console.dll"]
