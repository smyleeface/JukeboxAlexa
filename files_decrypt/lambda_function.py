import json
import logging

import boto3

logging.basicConfig()
logging.Logger('decrypt_to_public')
logger = logging.getLogger('decrypt_to_public')
logger.setLevel(logging.INFO)

s3 = boto3.client('s3')


def lambda_handler(event, context):

    logger.info(json.dumps(event))

    s3_event = event['Records'][0]

    key = s3_event["s3"]["object"]["key"]
    bucket = s3_event["s3"]["bucket"]["name"]

    response = s3.get_object(
        Bucket=bucket,
        Key=key
    )

    s3.put_object(
        Bucket=bucket,
        Key=key.replace("CodeBuildArtifacts/", ""),
        Body=response["Body"].read()
    )

    return 'Success'
