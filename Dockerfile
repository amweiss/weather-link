FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/WeatherLink/*.csproj ./src/WeatherLink/
COPY test/WeatherLink.Tests/*.csproj ./test/WeatherLink.Tests/
RUN dotnet restore

# copy everything else and build app
COPY src/WeatherLink/. ./src/WeatherLink/
WORKDIR /src/WeatherLink
RUN dotnet build -c Release -o app

FROM build as test
WORKDIR /test/WeatherLinks.Tests
COPY test/WeatherLink.Tests/. .
RUN dotnet build -c Release -o app
RUN dotnet test -c Release -o app

FROM build as publish
WORKDIR /src/WeatherLink
RUN dotnet publish -c Release -o app

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=publish /src/WeatherLink/app ./
ENTRYPOINT ["dotnet", "WeatherLink.dll"]