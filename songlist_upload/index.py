import json
import logging

import boto3

from jukebox_songs import JukeboxSongs

logging.basicConfig()
logging.Logger('jukebox_song_update')
logger = logging.getLogger('jukebox_song_update')
logger.setLevel(logging.INFO)

session = boto3.session.Session()
dynamo_client = session.client('dynamodb')
s3_client = session.client('s3')


def lambda_handler(event, context):
    print(f'*** INFO: got event{json.dumps(event, indent=2)}')
    record = event['Records'][0]
    bucket = record["s3"]["bucket"]["name"]
    file = record["s3"]["object"]["key"]
    jukebox_songs = JukeboxSongs(
        dynamo_client=dynamo_client,
        s3_client=s3_client,
        bucket=bucket,
        key=file,
        logger=logger
    )
    jukebox_songs.run_update()
    return "Update complete"
