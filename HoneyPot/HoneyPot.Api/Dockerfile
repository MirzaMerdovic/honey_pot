#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["HoneyPot.Api/HoneyPot.Api.csproj", "HoneyPot.Api/"]
RUN dotnet restore "HoneyPot.Api/HoneyPot.Api.csproj"
COPY . .
WORKDIR "/src/HoneyPot.Api"
RUN dotnet build "HoneyPot.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HoneyPot.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HoneyPot.Api.dll"]