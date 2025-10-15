FROM mcr.microsoft.com/dotnet/sdk:8.0

ARG LBHPACKAGESTOKEN
ENV LBHPACKAGESTOKEN=$LBHPACKAGESTOKEN
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./TenureInformationApi/TenureInformationApi.csproj ./
COPY /nuget.config /root/.nuget/NuGet/NuGet.Config
RUN dotnet restore ./TenureInformationApi.csproj


# Copy everything else and build
COPY ./TenureInformationApi ./
RUN dotnet build -c Release -o out ./TenureInformationApi.csproj
RUN dotnet publish -c Release -o out ./TenureInformationApi.csproj

EXPOSE ${PORT:-3000}
CMD ASPNETCORE_URLS=http://+:${PORT:-3000} dotnet ./out/TenureInformationApi.dll
