FROM mcr.microsoft.com/dotnet/sdk:9.0

WORKDIR /Conversey

# Copy csproj first for caching restores
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the code
COPY . ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:5000"]
