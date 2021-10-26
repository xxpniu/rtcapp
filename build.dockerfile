FROM toolsenv:latest as sdk

WORKDIR /xnet/src

COPY Server/RTC.XNet  .

RUN ["dotnet", "publish",  "-c", "Release",  "-o", "/app/publish", "RTC.XNet.csproj"]

FROM sdk as build

ENV GPRC_PLUGIN="/usr/bin/grpc_csharp_plugin"
ENV OUTDIR="/var/output"
ENV WORKDIR="/var/workdir"
ENV PROTODIR=${WORKDIR}
ENV CSHARP_OUT_PATH=${WORKDIR}/csharp
ENV CSHARP_DLL_OUT_PATH=${OUTDIR}/dll
ENV PROJECT_COPY_DIR=${OUTDIR}/projects
ENV IMPORT_PATH=${WORKDIR}/proto

WORKDIR ${WORKDIR}

COPY ./Tools/proto/  ${WORKDIR}/proto
COPY --from=sdk /app/publish /var/xnet
COPY ./Tools/enterpoint.sh  .

#RUN ["ls"] 
ENTRYPOINT [ "sh", "enterpoint.sh" ]