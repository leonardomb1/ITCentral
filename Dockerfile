FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /app

COPY /src/* ./
RUN dotnet publish -c Release -r linux-musl-x64 --self-contained true /p:PublishSingleFile=true /p:DebugType=None -o out

FROM alpine:latest
WORKDIR /app
COPY --from=build /app/out/ITCentral .
RUN apk add --no-cache icu-libs libc6-compat tzdata && \
    cp /usr/share/zoneinfo/America/Sao_Paulo /etc/localtime && \ 
    echo "America/Sao_Paulo" > /etc/timezone && \
    mkdir db log && \
    chmod +x /app/ITCentral

ENTRYPOINT ["/app/ITCentral"]

