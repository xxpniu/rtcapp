
## copy 

IMAGE=toolbuilder:latest

docker build . -f build.dockerfile -t ${IMAGE} 

if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi

cd ./Tools/

OUT_DIR=`pwd`/output

docker run -v $OUT_DIR:/var/output  -t  -i --rm ${IMAGE} 
if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi

#clear auto gen project
rm -rf ../Server/Grpc

cp -af output/projects ../Server/Grpc

## copy to client

CLIENT_OUT_DIR=../Packages/com.xsoft.rtc/Runtime/Plugins/

cp -af output/*.dll $CLIENT_OUT_DIR

cp -af output/*.pdb $CLIENT_OUT_DIR

## copy xnet sources

cp -af output/dll/RTC.XNet.* $CLIENT_OUT_DIR

docker rmi ${IMAGE}

