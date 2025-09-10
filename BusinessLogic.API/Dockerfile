FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BusinessLogic.API/BusinessLogic.API.csproj", "BusinessLogic.API/"]
COPY ["Shared.Models/Shared.Models.csproj", "Shared.Models/"]
RUN dotnet restore "BusinessLogic.API/BusinessLogic.API.csproj"
COPY . .
WORKDIR "/src/BusinessLogic.API"
RUN dotnet build "BusinessLogic.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BusinessLogic.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BusinessLogic.API.dll"]
