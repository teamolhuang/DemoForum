FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DemoForum.csproj", "Server/"]
RUN dotnet restore "Server/DemoForum.csproj"
WORKDIR "/src/Server"
COPY . .

RUN dotnet build "DemoForum.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoForum.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY ["/wwwroot/.well-known/pki-validation/B93643B8882C17124A16C0C66CCF6AFB.txt", "wwwroot/.well-known/pki-validation/"]

ENTRYPOINT ["dotnet", "DemoForum.dll"]
