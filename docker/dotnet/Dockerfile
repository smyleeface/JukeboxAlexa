# docker build -t jukebox_alexa_dotnet . -f Dockerfile-dotnet
# docker run --rm -it -v $HOME/.aws:/root/.aws -v ${PWD}:/project jukebox_alexa_dotnet

FROM microsoft/dotnet:2.1-sdk-alpine
WORKDIR /project

RUN apk update && apk upgrade && \
    apk add --no-cache bash git

RUN mkdir /Repos && \
    cd /Repos && \
    git clone https://github.com/LambdaSharp/LambdaSharpTool.git

ENV LAMBDASHARP=/Repos/LambdaSharpTool
RUN echo "dotnet run -p $LAMBDASHARP/src/MindTouch.LambdaSharp.Tool/MindTouch.LambdaSharp.Tool.csproj --" > /usr/bin/lash && \
    chmod +x /usr/bin/lash

RUN dotnet tool install --global coveralls.net
