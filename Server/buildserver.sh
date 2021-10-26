

docker rmi signalserver:latest sessionserver:latest loginserver:latest

##build signalserver


docker build -f docker-signal.dockerfile . -t signalserver:latest


##build sessionserver
docker build -f docker-sso.dockerfile . -t sessionserver:latest


##build loginserver
docker build -f docker-login.dockerfile . -t loginserver:latest
