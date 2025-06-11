FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM --platform=${BUILDPLATFORM} mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /src

COPY ["src/SD.OrderProcessing.Orders.Domain/SD.OrderProcessing.Orders.Domain.csproj", "SD.OrderProcessing.Orders.Domain/"]
COPY ["src/SD.OrderProcessing.Orders.Infrastructure/SD.OrderProcessing.Orders.Infrastructure.csproj", "SD.OrderProcessing.Orders.Infrastructure/"]
COPY ["src/SD.OrderProcessing.Orders.Api/SD.OrderProcessing.Orders.Api.csproj", "SD.OrderProcessing.Orders.Api/"]

RUN dotnet restore "SD.OrderProcessing.Orders.Api/SD.OrderProcessing.Orders.Api.csproj" --arch ${TARGETARCH}
COPY src/. .
RUN dotnet build "SD.OrderProcessing.Orders.Api/SD.OrderProcessing.Orders.Api.csproj" --arch ${TARGETARCH} -c Release --no-restore -o /app/build

WORKDIR "/src/SD.OrderProcessing.Orders.Api"
FROM build AS publish
RUN dotnet publish "SD.OrderProcessing.Orders.Api.csproj" --arch ${TARGETARCH}  -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app    
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SD.OrderProcessing.Orders.Api.dll"]