FROM mcr.microsoft.com/dotnet/sdk:3.1 as sdk

RUN ["apt" ,"update"]
RUN ["apt", "install", "-y", "protobuf-compiler"]
RUN ["apt","install","-y","protobuf-compiler-grpc"]

CMD [ "/bin/bash" ]
