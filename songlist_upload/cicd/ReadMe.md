# CICD songlist_upload
This is code used to create the songlist_upload portion of the CICD pipeline.

* `deploy.sh`: Used locally to update the CodeBuild CloudFormation stack 
* `cf_codebuild.yaml`: The CloudFormation Template to create CodeBuild project for this function
* `../buildspec.yaml`: Packaging spec used in the CodeBuild project. Compresses and packages this project and uploads to S3 for lambda functions to import.