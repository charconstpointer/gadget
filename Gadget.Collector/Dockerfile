﻿FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Gadget.Collector/Gadget.Collector.csproj", "Gadget.Collector/"]
RUN dotnet restore "Gadget.Collector/Gadget.Collector.csproj"
COPY . .
WORKDIR "/src/Gadget.Collector"
RUN dotnet build "Gadget.Collector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gadget.Collector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gadget.Collector.dll"]
