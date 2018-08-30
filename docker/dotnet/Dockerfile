FROM amazonlinux:2018.03
WORKDIR /project

##########
# utils
##########
RUN yum install wget git -y

##########
# python
##########
RUN yum install python36 -y && \
    alternatives --set python /usr/bin/python3.6 && \
    python --version

###############
# Dotnet
###############
RUN wget https://download.microsoft.com/download/E/8/A/E8AF2EE0-5DDA-4420-A395-D1A50EEFD83E/dotnet-sdk-2.1.401-linux-x64.tar.gz
RUN mkdir -p /usr/local/lib/dotnet && \
    tar zxf dotnet-sdk-2.1.401-linux-x64.tar.gz -C /usr/local/lib/dotnet
RUN ln -s /usr/local/lib/dotnet/dotnet /usr/bin/dotnet && \
    dotnet --version
RUN rm dotnet-sdk-2.1.401-linux-x64.tar.gz

###############
# lambdasharp
###############
RUN mkdir /tools && \
    cd /tools && \
    git clone https://github.com/LambdaSharp/LambdaSharpTool.git
ENV LAMBDASHARP=/tools/LambdaSharpTool
RUN echo "dotnet run -p $LAMBDASHARP/src/MindTouch.LambdaSharp.Tool/MindTouch.LambdaSharp.Tool.csproj --" > /usr/bin/lash && \
    chmod +x /usr/bin/lash

###########
# coverage
###########
RUN dotnet tool install coveralls.net --tool-path tools
RUN dotnet tool install coverlet.console --tool-path tools

##########
# aws cli
##########
RUN pip install awscli
