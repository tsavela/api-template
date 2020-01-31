FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app

# Visual Studio container debugger uses port 80
EXPOSE 80 8080

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["Api/Api.csproj", "Api/"]
COPY ["Database/Database.csproj", "Database/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "Api/Api.csproj"
COPY . .

WORKDIR "/src"
RUN dotnet test

WORKDIR "/src/Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Api.dll"]