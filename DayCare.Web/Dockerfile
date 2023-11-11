#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src
COPY ["DayCare.Web/DayCare.Web.csproj", "DayCare.Web/"]
RUN dotnet restore "DayCare.Web/DayCare.Web.csproj"
COPY . .
WORKDIR "/src/DayCare.Web"
RUN dotnet build "DayCare.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DayCare.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet ef migrations bundle -o /app/publish/Migrations --self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DayCare.Web.dll"]