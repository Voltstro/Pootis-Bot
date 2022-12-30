ARG ARCH=amd64
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY src/ src
RUN dotnet restore src/Pootis-Bot.sln

COPY LICENSE.md LICENSE.md
COPY thirdpartycredits.txt thirdpartycredits.txt
RUN dotnet publish src/Pootis-Bot.sln -c Release

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine3.17-${ARCH}
ENV PB_NAME="Pootis-Bot Docker"
WORKDIR /app
COPY --from=build /app/src/bin/Release/publish .
ENTRYPOINT ["dotnet", "Pootis-Bot.dll", "--headless"]