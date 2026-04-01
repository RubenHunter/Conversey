FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /backend

# Copy csproj first for caching restores
COPY *.sln ./
COPY BL/*.csproj BL/
COPY DAL/*.csproj DAL/
COPY Domain/*.csproj Domain/
COPY REST/*.csproj REST/

RUN dotnet restore

COPY . .

EXPOSE 5000

ENTRYPOINT ["dotnet", "watch", "run", "--project", "REST/REST.csproj", "--urls", "http://0.0.0.0:5000"]
