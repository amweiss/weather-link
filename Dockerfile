FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.sln .
COPY WeatherLink/*.csproj ./WeatherLink/
COPY WeatherLink.Tests/*.csproj ./WeatherLink.Tests/
RUN dotnet restore

# copy everything else and build app
COPY WeatherLink/. ./WeatherLink/
WORKDIR /src/WeatherLink
RUN dotnet build -c Release -o app

FROM build as test
WORKDIR /src/WeatherLink.Tests
RUN dotnet test -c Release

FROM build as publish
WORKDIR /src/WeatherLink
RUN dotnet publish -c Release -o app

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=publish /app ./
ENTRYPOINT ["dotnet", "WeatherLink.dll"]

