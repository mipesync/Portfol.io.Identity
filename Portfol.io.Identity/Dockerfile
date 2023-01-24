#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Portfol.io.Identity.csproj", ""]
RUN dotnet restore "Portfol.io.Identity.csproj"
COPY . .
WORKDIR "/src/Portfol.io.Identity"
RUN dotnet build "Portfol.io.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Portfol.io.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Portfol.io.Identity.dll"]