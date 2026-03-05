FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dev

WORKDIR /src

EXPOSE 5000
CMD ["sh", "-c", "dotnet restore && dotnet watch --project REST/REST.csproj run --urls http://0.0.0.0:5000"]
