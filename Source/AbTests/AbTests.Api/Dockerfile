FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AbTests.Api/AbTests.Api.csproj", "AbTests.Api/"]
RUN dotnet restore "AbTests.Api/AbTests.Api.csproj"
COPY . .
WORKDIR "/src/AbTests.Api"
RUN dotnet build "AbTests.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AbTests.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AbTests.Api.dll"]
