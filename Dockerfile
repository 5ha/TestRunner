FROM microsoft/dotnet-framework
WORKDIR /app
COPY SystemUnderTest/bin/debug/ .
WORKDIR /tester
COPY TestRunner/bin/debug/ .