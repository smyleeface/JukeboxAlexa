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
