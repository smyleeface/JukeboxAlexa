CloudFormation Templates
========================

Run `jukebox-main.yaml` in CloudFormation.

Nested CloudFormation templates are hosted in [an public S3 bucket](https://s3-us-west-2.amazonaws.com/smyleeface-public/JukeboxAlexa/cloudformation/jukebox-main.yaml).

Additionally, you could upload the CloudFormation templates to your own S3 bucket and change the paths in `jukebox-main.yaml`

### Troubleshooting

Issue: `Unable to validate the following destination configurations.`

Solution: 

Deploy CloudFormation three times with different settings for s3. `AWS::Lambda::Permission` & S3 `NotificationConfiguration` must be commented out on initial deploy, and uncomment in subsequent deploys to allow for circular dependencies to be in place.

- Deploy with `Comment Block #1` [in the main CloudFormation file](jukebox-main.yaml)  & `Comment Block #2` [in the CloudFormation S3 file](jukebox-s3.yaml)

- After initial deploy, uncomment `Comment Block #1` [in the main CloudFormation file](jukebox-main.yaml) and re-deploy

- After second deploy, uncomment `Comment Block #2` [in the CloudFormation S3 file](jukebox-s3.yaml) and re-deploy

https://aws.amazon.com/premiumsupport/knowledge-center/unable-validate-destination-s3/
https://stackoverflow.com/questions/36338890/enable-lambda-function-to-an-s3-bucket-using-cloudformation