import boto3

from jukebox_songs import JukeboxSongs

session = boto3.session.Session()


def lambda_handler(event, context):

    record = event['Records'][0]
    bucket = record["s3"]["bucket"]["name"]
    file = record["s3"]["object"]["key"]
    jukebox_songs = JukeboxSongs(
        session=session,
        bucket=bucket,
        key=file
    )
    jukebox_songs.run_update()
    return "Update complete"


if __name__ == '__main__':
    event = {
        "Records": [
            {
                "eventVersion": "2.0",
                "eventTime": "1970-01-01T00:00:00.000Z",
                "requestParameters": {
                    "sourceIPAddress": "127.0.0.1"
                },
                "s3": {
                    "configurationId": "testConfigRule",
                    "object": {
                        "eTag": "0123456789abcdef0123456789abcdef",
                        "sequencer": "0A1B2C3D4E5F678901",
                        "key": "JukeboxSongs.csv",
                        "size": 1024
                    },
                    "bucket": {
                        "arn": "arn:aws:s3:::s3-jukebox-test-bucket",
                        "name": "s3-jukebox-test-bucket",
                        "ownerIdentity": {
                            "principalId": "EXAMPLE"
                        }
                    },
                    "s3SchemaVersion": "1.0"
                },
                "responseElements": {
                    "x-amz-id-2": "EXAMPLE123/5678abcdefghijklambdaisawesome/mnopqrstuvwxyzABCDEFGH",
                    "x-amz-request-id": "EXAMPLE123456789"
                },
                "awsRegion": "us-east-1",
                "eventName": "ObjectCreated:Put",
                "userIdentity": {
                    "principalId": "EXAMPLE"
                },
                "eventSource": "aws:s3"
            }
        ]
    }
    lambda_handler(event, {})
