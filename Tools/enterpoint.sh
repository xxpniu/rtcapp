
cp -af /var/xnet/* $CSHARP_DLL_OUT_PATH

PROTO_OUT=./RTC.ProtoGrpc

if [ ! -d $PROTO_OUT ];then 
   mkdir $PROTO_OUT
   echo $PROTO_OUT
fi

if [ ! -d $CSHARP_DLL_OUT_PATH ];then 
   mkdir $CSHARP_DLL_OUT_PATH
   echo $CSHARP_DLL_OUT_PATH
fi

protoc ./proto/*.proto -I=./proto \
    --csharp_out=$PROTO_OUT \
    --plugin=protoc-gen-grpc=$GPRC_PLUGIN \
    --grpc_out=$PROTO_OUT 

if [ "$?" -ne "0" ]; then
  echo "Failure  check proto files"
  exit 1
fi

cd $PROTO_OUT

dotnet new classlib --language C#  --framework "netstandard2.1"
rm Class1.cs 
dotnet add package Google.Protobuf -v 3.15.2
dotnet add package Grpc -v 2.40.0
 
dotnet publish  -o $CSHARP_DLL_OUT_PATH
if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi

cd ../



if [ ! -d $PROJECT_COPY_DIR ];then 
   mkdir $PROJECT_COPY_DIR
   echo $PROJECT_COPY_DIR
fi

cp -af $PROTO_OUT/ $PROJECT_COPY_DIR
if [ "$?" -ne "0" ]; then
  echo "Failure copy projects"
  exit 1
fi

