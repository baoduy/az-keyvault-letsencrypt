FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
ARG TARGETARCH
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Drunk.KeuVault.LetsEncrypt.csproj", "./"]
RUN dotnet restore "Drunk.KeuVault.LetsEncrypt.csproj"
COPY . .
WORKDIR "/src/"

RUN dotnet build "Drunk.KeuVault.LetsEncrypt.csproj" -c $BUILD_CONFIGURATION -o /app/build -a $TARGETARCH

FROM --platform=$BUILDPLATFORM build AS publish
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "Drunk.KeuVault.LetsEncrypt.csproj" -c $BUILD_CONFIGURATION \
    -o /app/publish /p:UseAppHost=true -a $TARGETARCH \
    --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=false

FROM --platform=$BUILDPLATFORM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID

ENTRYPOINT [ "./Drunk.KeuVault.LetsEncrypt"]
