FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY src/ src
RUN dotnet restore src/Pootis-Bot.sln

COPY LICENSE.md LICENSE.md
COPY thirdpartycredits.txt thirdpartycredits.txt
RUN dotnet publish src/Pootis-Bot.sln --no-restore -c Release

FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime:7.0-alpine3.17
ENV PB_NAME="Pootis-Bot Docker"
WORKDIR /app
COPY --from=build /app/src/bin/Release/publish .
ENTRYPOINT ["dotnet", "Pootis-Bot.dll", "--headless"]