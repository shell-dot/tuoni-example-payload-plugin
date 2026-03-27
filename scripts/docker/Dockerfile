# Stage 1: Build the .NET agent with Mono
FROM mono:6 AS csharp-build
WORKDIR /build/DotNetAgent
COPY DotNetAgent/ .
RUN msbuild DotNetAgent.csproj /p:Configuration=Release

# Stage 2: Build the Gradle plugin JAR (embeds the EXE)
FROM eclipse-temurin:21-jdk AS java-build
WORKDIR /build

# Place EXE where Gradle's processResources expects it (../DotNetAgent/bin/Release/)
COPY --from=csharp-build /build/DotNetAgent/bin/Release/DotNetAgent.exe \
     DotNetAgent/bin/Release/DotNetAgent.exe

# Copy Gradle project
COPY DotNetAgentPlugin/ DotNetAgentPlugin/

WORKDIR /build/DotNetAgentPlugin
RUN chmod +x gradlew && ./gradlew build --no-daemon

# Stage 3: Minimal image with just the artifacts for extraction
FROM scratch AS artifacts
COPY --from=csharp-build /build/DotNetAgent/bin/Release/DotNetAgent.exe \
     /DotNetAgent.exe
COPY --from=java-build \
     /build/DotNetAgentPlugin/build/libs/tuoni-example-plugin-dotnet-payload-0.0.1.jar \
     /tuoni-example-plugin-dotnet-payload-0.0.1.jar
