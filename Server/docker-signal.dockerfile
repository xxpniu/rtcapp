FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

COPY . .

WORKDIR /src/SignalServer

RUN ["dotnet", "build", "-c", "Release", "-o", "/app/build","SignalServer.csproj" ]

FROM build AS publish

RUN ["dotnet", "publish", "-c", "Release", "-o", "/app/publish","SignalServer.csproj" ]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/SignalServer/enterpoint.sh .

ENV ADDRESS=127.0.0.1 
ENV PORT=3400
ENV ZKROOT=/rtcsignalserver
ENV ZKSSO=/rtcsessionserver
ENV DBNAMER=rtc_account

EXPOSE ${PORT}

ENTRYPOINT ["sh", "enterpoint.sh"]