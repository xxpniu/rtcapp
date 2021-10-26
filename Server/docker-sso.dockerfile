FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

COPY . .

WORKDIR /src/SessionServer

RUN ["dotnet", "build","-c", "Release", "-o", "/app/build", "SessionServer.csproj" ]

FROM build AS publish

#RUN dotnet publish "SessionServer.csproj"  -c Release  -o /app/publish 

RUN ["dotnet", "publish","-c", "Release", "-o", "/app/publish", "SessionServer.csproj" ]

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/SessionServer/enterpoint.sh .

ENV ADDRESS=127.0.0.1 
ENV PORT=3400
ENV DBSTR=127.0.0.1
ENV ZKROOT=/rtcsessionserver
ENV DBNAME=rtc_account
EXPOSE ${PORT}

ENTRYPOINT ["sh", "enterpoint.sh"]