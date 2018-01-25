# JukeboxAlexa

Alexa skill used to send song requests to a jukebox.

## Build/Deploy

### CodeBuild

* `buildspec.package.sh` used in CodeBuild's buildspec.yml when an update from Github triggers a build. 
* CodeBuild uses the `Docker.build` image from ECR `jukebox_alexa_build` to build the files in this project then publish and zip them.
* To update ECR `jukebox_alexa_build` image:
    * make changes to `Dockerfile.build`
    * locally run `dockerfile.build.sh` to build and push `Dockerfile.build` to ECR
* CodeBuild packages files in *.zip to public bucket defined in [cloudformation](../cloudformation)

### Lambda Development

Local building and deploying the lambda function.

```shell
dotnet restore
dotnet build
dotnet lambda deploy-function jukebox_alexa --region us-west-2
```
