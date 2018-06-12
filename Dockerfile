FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY WeatherLink/*.csproj ./WeatherLink/
COPY WeatherLink.Tests/*.csproj ./WeatherLink.Tests/
RUN dotnet restore

# copy everything else and build app
COPY WeatherLink/. ./WeatherLink/
WORKDIR /app/WeatherLink
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=build /app/WeatherLink/out ./
ENTRYPOINT ["dotnet", "WeatherLink.dll"]