FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY ["src/SD.OrderProcessing.Payment.Domain/SD.OrderProcessing.Payment.Domain.csproj", "SD.OrderProcessing.Payment.Domain/"]
COPY ["src/SD.OrderProcessing.Payment.Infrastructure/SD.OrderProcessing.Payment.Infrastructure.csproj", "SD.OrderProcessing.Payment.Infrastructure/"]
COPY ["src/SD.OrderProcessing.Payment.Api/SD.OrderProcessing.Payment.Api.csproj", "SD.OrderProcessing.Payment.Api/"]

RUN dotnet restore "SD.OrderProcessing.Payment.Api/SD.OrderProcessing.Payment.Api.csproj" --arch ${TARGETARCH}
COPY src/. .
RUN dotnet build "SD.OrderProcessing.Payment.Api/SD.OrderProcessing.Payment.Api.csproj" --arch ${TARGETARCH} -c Release --no-restore -o /app/build

WORKDIR "/src/SD.OrderProcessing.Payment.Api"
FROM build AS publish
RUN dotnet publish "SD.OrderProcessing.Payment.Api.csproj" --arch ${TARGETARCH}  -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app    
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SD.OrderProcessing.Payment.Api.dll"]