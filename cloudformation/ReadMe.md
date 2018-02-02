CloudFormation Templates
========================

Run `jukebox-main.yaml` in CloudFormation.

Nested CloudFormation templates are hosted in [an public S3 bucket](https://s3-us-west-2.amazonaws.com/smyleeface-public/JukeboxAlexa/cloudformation/jukebox-main.yaml).

Additionally, you could upload the cloudformation templates to your own s3 bucket and change the paths in `jukebox-main.yaml`

### Troubleshooting

Issue: `Unable to validate the following destination configurations.`

Solution: 

[NotificationConfiguration section in the cloudformation-s3](jukebox-s3.yaml) must be commented out on first deploy/creation of cloudformation

After first deploy, uncomment and re-deploy to update cloudformation

https://aws.amazon.com/premiumsupport/knowledge-center/unable-validate-destination-s3/