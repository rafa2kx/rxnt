FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RXNT.API.csproj", "."]
RUN dotnet restore "RXNT.API.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "RXNT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RXNT.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RXNT.API.dll"]
