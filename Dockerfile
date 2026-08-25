ARG TARGETARCH
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
ARG tool
ARG TARGETARCH
WORKDIR /app

# Copy project file
COPY . ./
# Restore as distinct layers
RUN project="src/${tool}/${tool}.csproj"; \
	if [ ! -f "$project" ]; then project="src/${tool}/${tool}/${tool}.csproj"; fi; \
	case "$TARGETARCH" in \
		amd64) rid="linux-x64" ;; \
		arm64) rid="linux-arm64" ;; \
		*) echo "Unsupported target architecture: $TARGETARCH" >&2; exit 1 ;; \
	 esac; \
	dotnet restore "$project" --runtime "$rid"
# Build and publish a release
RUN project="src/${tool}/${tool}.csproj"; \
	if [ ! -f "$project" ]; then project="src/${tool}/${tool}/${tool}.csproj"; fi; \
	case "$TARGETARCH" in \
		amd64) rid="linux-x64" ;; \
		arm64) rid="linux-arm64" ;; \
		*) echo "Unsupported target architecture: $TARGETARCH" >&2; exit 1 ;; \
	 esac; \
	dotnet publish "$project" -c Release -r "$rid" -o out /p:PublishSingleFile=true /p:CopyOutputSymbolsToPublishDirectory=false /p:AssemblyName=docfx-companion-tools-entrypoint --self-contained false

# Build runtime image
FROM --platform=$TARGETPLATFORM mcr.microsoft.com/dotnet/runtime:10.0
COPY --from=build-env /app/out /usr/bin/
USER $APP_UID
ENTRYPOINT ["docfx-companion-tools-entrypoint"]