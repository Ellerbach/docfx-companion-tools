FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
ARG tool
WORKDIR /app

# Copy project file
COPY . ./
# Restore as distinct layers
RUN dotnet restore "src/${tool}/${tool}.csproj"
# Build and publish a release
RUN dotnet publish "src/${tool}/${tool}.csproj" -c Release -r linux-musl-x64 -o out /p:PublishSingleFile=true /p:CopyOutputSymbolsToPublishDirectory=false /p:AssemblyName=docfx-companion-tools-entrypoint --self-contained false

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
RUN adduser -D docfx-companion-tools
COPY --from=build-env /app/out /usr/bin/
USER docfx-companion-tools
ENTRYPOINT ["docfx-companion-tools-entrypoint"]