# 概述
 
 > 基于rtc可以解决p2p通讯，基于GRPC的框架让编写服务器变得简单。

---

# 项目说明
## 基于Web RTC的unity App 
 - unity Web RTC 
 - coturn 服务
 - grpc 实现登陆逻辑
 - mongo DB
 - zookeeper 发现

## 下载GRPC库替换
  - 地址 ：
    > https://packages.grpc.io/ 

## 编译proto项目
 1. 安装docker :
    > https://www.docker.com    
 2. 构建编译环境镜像： 
    >`sh buildtoolsenv.sh`
 3. 编译proto&生成grpc service 
    >`sh runbuild.sh` 
 4. proto文件所在目录：
    >` ./Tools/proto`
 5. Grpc客户端dll输出目录
    > `./Packages/com.xsoft.rtc/Plugins`  
 6. C#源码 & 服务器引用项目 所在目录
    >` ./Server/Grpc/RTC.ProtoGrpc/`  
## 编译服务器
 - 编译服务器脚本所在目录
   >`cd Server` \
    `sh buildserver.sh` 

## 部署服务器
 ### 部署服务器需要的环境
  > `cd docker/server-env `\
  `docker-compose up -d `

 ### 启动服务器  
  1. 配置环境参数
  >`cd docker/server-de` 

  2. 创建文件 `.env`  
  ```shell 
  # 服务器的公网IP 
  HOSTIP=[ServerIP] #localhost
  # mongo db 的链接配置
  MONGO=mongodb://username:pwd@[ServerIP]:27017/
  # zk的服务器ip
  ZKSERVER_ADDR=[ServerIP]:2181
  #替换 Server IP 到对应的  
  ```
  3. 启动服务器
  > `docker-compose up -d`
   
