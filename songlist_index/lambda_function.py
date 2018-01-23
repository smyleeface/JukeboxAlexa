import json
import logging

import boto3

logging.basicConfig()
logging.Logger('jukebox_song_indexer')
logger = logging.getLogger('jukebox_song_indexer')
logger.setLevel(logging.INFO)

dynamodb_rclient = boto3.resource('dynamodb', region_name='us-west-2')
dynamodb_table = dynamodb_rclient.Table('JukeboxSongsCache')

def insert_item(key, song):
    # Add the word to the list
    dynamodb_table.put_item(
        Item={
            'word': key,
            'songs': song
        }
    )


def insert_song(key, number, artist, title):

        total_songs = 0

        # Get existing items
        try:
            found = False

            # Get existing items
            db_items = dynamodb_table.get_item(
                Key={
                    'word': key
                }
            )

            # find this song in existing items
            if db_items.get('Item'):
                for id, song in enumerate(db_items['Item']['songs']):
                    if song['title'] == title and song['artist'] == artist and song['track_number'] == number:
                        found = True

            if not found:

                # COUNT NUMBER OF EXISTING ITEMS FOR A WORD
                if db_items.get('Item'):
                    total_songs = len(db_items['Item']['songs'])

                # ADD ITEM TO A LIST
                set_string = f"SET songs[{total_songs}] = :n"
                dynamodb_table.update_item(
                    Key={
                        'word': key
                    },
                    UpdateExpression=set_string,
                    ExpressionAttributeValues={
                        ':n': {
                                'artist': artist,
                                'track_number': number,
                                'title': title
                            }
                    },
                    ReturnValues="NONE"
                )

        except Exception as e:
            song = [{
                'artist': artist,
                'track_number': number,
                'title': title
            }]
            insert_item(key, song)


def delete_song(key, number, artist, title):

    try:
        # Get existing items
        db_items = dynamodb_table.get_item(
            Key={
                'word': key
            }
        )
        # find this song in existing items
        if db_items.get('Item'):
            for id, song in enumerate(db_items['Item']['songs']):
                if song['title'] == title and song['artist'] == artist and song['track_number'] == number:

                    # REMOVE ITEM FROM LIST
                    set_string = f'REMOVE songs[{id}]'
                    dynamodb_table.update_item(
                        Key={
                            'word': key
                        },
                        UpdateExpression=set_string,
                        ReturnValues="NONE"
                    )
    except Exception as e:
        pass


def lambda_handler(event, context):

    logger.info(json.dumps(event))

    for event_data in event['Records']:

        action = event_data['eventName']
        logger.info(f'*** INFO: running {action}')

        if action == 'INSERT':
            request_item = event_data['dynamodb']['NewImage']

            number = request_item['track_number']['S']
            artist = request_item['artist']['S']
            title = request_item['title']['S']

            # split the song name
            title_words = request_item['title']['S'].lower().split(' ')

            for title_word in title_words:
                insert_song(title_word, number, artist, title)

        if action == 'REMOVE':
            request_item = event_data['dynamodb']['OldImage']

            number = request_item['track_number']['S']
            artist = request_item['artist']['S']
            title = request_item['title']['S']

            # split the song name
            title_words = request_item['title']['S'].lower().split(' ')

            for title_word in title_words:
                delete_song(title_word, number, artist, title)

    return 'Hello from Lambda'


if __name__ == '__main__':
    delete_event = {
        "Records": [
            {
                "eventID": "322ab57ddfb24bcdb8d12b3a969c63f7",
                "eventName": "REMOVE",
                "eventVersion": "1.1",
                "eventSource": "aws:dynamodb",
                "awsRegion": "us-west-2",
                "dynamodb": {
                    "ApproximateCreationDateTime": 1514110200,
                    "Keys": {
                        "title": {
                            "S": "Alone"
                        },
                        "artist": {
                            "S": "Heart"
                        }
                    },
                    "OldImage": {
                        "title": {
                            "S": "Alone"
                        },
                        "number": {
                            "S": "301"
                        },
                        "artist": {
                            "S": "Heart"
                        }
                    },
                    "SequenceNumber": "6858100000000001369768583",
                    "SizeBytes": 49,
                    "StreamViewType": "NEW_AND_OLD_IMAGES"
                },
                "eventSourceARN": "arn:aws:dynamodb:us-west-2:952671759649:table/JukeboxSongs/stream/2017-12-24T09:57:02.722"
            }
        ]
    }
    add_event = {
        "Records": [
            {
                "eventID": "99e21ba54e7d7f4becbf5f707acdb834",
                "eventName": "INSERT",
                "eventVersion": "1.1",
                "eventSource": "aws:dynamodb",
                "awsRegion": "us-west-2",
                "dynamodb": {
                    "ApproximateCreationDateTime": 1514110200,
                    "Keys": {
                        "title": {
                            "S": "Alone"
                        },
                        "artist": {
                            "S": "Heart"
                        }
                    },
                    "NewImage": {
                        "title": {
                            "S": "Alone"
                        },
                        "number": {
                            "S": "301"
                        },
                        "artist": {
                            "S": "Heart"
                        }
                    },
                    "SequenceNumber": "6858200000000001369780780",
                    "SizeBytes": 49,
                    "StreamViewType": "NEW_AND_OLD_IMAGES"
                },
                "eventSourceARN": "arn:aws:dynamodb:us-west-2:952671759649:table/JukeboxSongs/stream/2017-12-24T09:57:02.722"
            }
        ]
    }
    lambda_handler(delete_event, {})

