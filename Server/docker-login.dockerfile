FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

COPY . .

WORKDIR /src/LoginServer

RUN ["dotnet", "build","-c", "Release", "-o", "/app/build", "LoginServer.csproj" ]

FROM build AS publish


RUN ["dotnet", "publish","-c", "Release", "-o", "/app/publish", "LoginServer.csproj" ]

FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/LoginServer/enterpoint.sh .

ENV ADDRESS=127.0.0.1 
ENV PORT=3400
ENV DBSTR=127.0.0.1
ENV ZKROOT=/rtcloginserver
ENV DBNAME=rtc_account
EXPOSE ${PORT}

ENTRYPOINT ["sh", "enterpoint.sh"]