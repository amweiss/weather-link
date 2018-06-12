FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY WeatherLink/bin/Release/netcoreapp2.1/publish/ ./WeatherLink
ENTRYPOINT ["dotnet", "WeatherLink.dll"]