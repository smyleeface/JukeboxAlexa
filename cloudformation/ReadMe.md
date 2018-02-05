CloudFormation Templates
========================

Run `jukebox-main.yaml` in CloudFormation.

Nested CloudFormation templates are hosted in [an public S3 bucket](https://s3-us-west-2.amazonaws.com/smyleeface-public/JukeboxAlexa/cloudformation/jukebox-main.yaml).

Additionally, you could upload the CloudFormation templates to your own S3 bucket and change the paths in `jukebox-main.yaml`

### Troubleshooting

Issue: `Unable to validate the following destination configurations.`

Solution: 

Deploy CloudFormation three times with different settings for s3. [In the CloudFormation S3 File](jukebox-s3.yaml):

- `BucketPermission` & `NotificationConfiguration` must be commented out on initial to allow for circular dependencies to be in place.

- After initial deploy, uncomment `Comment Block #1` and re-deploy to add permission to the lambda function to the bucket

- After second deploy, uncomment `Comment Block #2` and re-deploy to add event trigger to s3 bucket

https://aws.amazon.com/premiumsupport/knowledge-center/unable-validate-destination-s3/
https://stackoverflow.com/questions/36338890/enable-lambda-function-to-an-s3-bucket-using-cloudformation