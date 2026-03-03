FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY *.sln .
COPY BL/BL.csproj ./BL/
COPY DAL/DAL.csproj ./DAL/
COPY Domain/Domain.csproj ./Domain/
COPY REST/REST.csproj ./REST/

RUN dotnet restore

EXPOSE 5000

CMD ["dotnet", "watch", "--project", "REST/REST.csproj", "run", "--urls", "http://0.0.0.0:5000"]
