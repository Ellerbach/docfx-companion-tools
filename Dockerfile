FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
ARG tool
WORKDIR /app

# Copy project file
COPY . ./
# Restore as distinct layers
RUN project="src/${tool}/${tool}.csproj"; \
	if [ ! -f "$project" ]; then project="src/${tool}/${tool}/${tool}.csproj"; fi; \
	dotnet restore "$project"
# Build and publish a release
RUN project="src/${tool}/${tool}.csproj"; \
	if [ ! -f "$project" ]; then project="src/${tool}/${tool}/${tool}.csproj"; fi; \
	dotnet publish "$project" -c Release -r linux-x64 -o out /p:PublishSingleFile=true /p:CopyOutputSymbolsToPublishDirectory=false /p:AssemblyName=docfx-companion-tools-entrypoint --self-contained false

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0
COPY --from=build-env /app/out /usr/bin/
USER $APP_UID
ENTRYPOINT ["docfx-companion-tools-entrypoint"]