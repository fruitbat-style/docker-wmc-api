FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY WMCApi.csproj ./
RUN dotnet restore WMCApi.csproj
COPY . .
RUN dotnet publish WMCApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5232
EXPOSE 5232
ENTRYPOINT ["dotnet", "WMCApi.dll"]
