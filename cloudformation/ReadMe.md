CloudFormation Templates
========================

Run `jukebox-main.yaml` in CloudFormation.

Nested CloudFormation templates are hosted in [an public S3 bucket](https://s3-us-west-2.amazonaws.com/smyleeface-public/JukeboxAlexa/cloudformation/jukebox-main.yaml).

Additionally, you could upload the CloudFormation templates to your own S3 bucket and change the paths in `jukebox-main.yaml`

### Troubleshooting

Issue: `Unable to validate the following destination configurations.`

Solution: 

Deploy CloudFormation three times with different settings for s3. `AWS::Lambda::Permission` & S3 `NotificationConfiguration` must be commented out on initial deploy, and uncomment in subsequent deploys to allow for circular dependencies to be in place.

Steps:

- Deploy with `Comment Block #1` and one `TemplateUrl` in `Comment Block #2` [in the main CloudFormation file](jukebox-main.yaml)

- After initial deploy, uncomment `Comment Block #1` in `jukebox-lambda-main.yaml` and re-deploy to add invoke permission to the lambda function

- After second deploy, swap the `TemplateURL` in `Comment Block #2` below and re-deploy to add event trigger to s3 bucket

https://aws.amazon.com/premiumsupport/knowledge-center/unable-validate-destination-s3/
https://stackoverflow.com/questions/36338890/enable-lambda-function-to-an-s3-bucket-using-cloudformation



Issue: Update lambda function's code

Solution:
To update a Lambda function whose source code is in an Amazon S3 bucket, you must trigger an update by updating the S3Bucket, S3Key, or S3ObjectVersion property. Updating the source code alone doesn't update the function.
https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/aws-properties-lambda-function-code.html