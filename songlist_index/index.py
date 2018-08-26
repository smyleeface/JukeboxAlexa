import json
import logging

import boto3

from song_index_data import SongIndexData

logging.basicConfig()
logging.Logger('jukebox_song_indexer')
logger = logging.getLogger('jukebox_song_indexer')
logger.setLevel(logging.INFO)

dynamodb_rclient = boto3.client('dynamodb', region_name='us-west-2')


def lambda_handler(event, context):

    logger.info(json.dumps(event))

    for event_data in event['Records']:

        action = event_data['eventName']
        logger.info(f'*** INFO: running {action}')

        if action == 'INSERT':
            request_item = event_data['dynamodb']['NewImage']
        if action == 'REMOVE':
            request_item = event_data['dynamodb']['OldImage']

        logger.info(f'*** INFO: request_item {request_item}')
        number = request_item['track_number']['S']
        artist = request_item['search_artist']['S']
        title = request_item['search_title']['S']

        song_index_data = SongIndexData(dynamodb_client=dynamodb_rclient, number=number, artist=artist, title=title,
                                        table_name='JukeboxSongsCache',
                                        logger=logger)

        if action == 'INSERT':
            song_index_data.insert_songs()
        if action == 'REMOVE':
            song_index_data.delete_songs()

    return 'finished indexing songs'
