FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /usr/src/app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /usr/src/app
COPY --from=build-env /usr/src/app/out .
ENTRYPOINT ["dotnet", "WeatherForecastHTTP.dll"]