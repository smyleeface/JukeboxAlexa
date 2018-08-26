import unittest
from copy import deepcopy
from song_index_data import SongIndexData
from tests.generate_stubber import GenerateStubber
from botocore.exceptions import ClientError
from song import Song


class AddSongsTest(unittest.TestCase):

    def setUp(self):
        dynamodb_trigger_payload = {
            "Records": [
                {
                    "eventID": "7de3041dd709b024af6f29e4fa13d34c",
                    "eventName": "INSERT",
                    "eventVersion": "1.1",
                    "eventSource": "aws:dynamodb",
                    "awsRegion": "us-west-2",
                    "dynamodb": {
                        "ApproximateCreationDateTime": 1479499740,
                        "Keys": {
                            "Timestamp": {
                                "S": "2016-11-18:12:09:36"
                            },
                            "Username": {
                                "S": "John Doe"
                            }
                        },
                        "NewImage": {
                            "Timestamp": {
                                "S": "2016-11-18:12:09:36"
                            },
                            "Message": {
                                "S": "This is a bark from the Woofer social network"
                            },
                            "Username": {
                                "S": "John Doe"
                            }
                        },
                        "SequenceNumber": "13021600000000001596893679",
                        "SizeBytes": 112,
                        "StreamViewType": "NEW_IMAGE"
                    },
                    "eventSourceARN": "arn:aws:dynamodb:us-east-1:123456789012:table/BarkTable/stream/2016-11-16T20:42:48.104"
                }
            ]
        }
        # Arrange
        put_request = [
            {
                'PutRequest': {
                    'Item': {
                        'artist': {'S': 'The Jeff Healy Band'},
                        'search_artist': {'S': 'the jeff healy band'},
                        'search_title': {'S': 'angel eyes'},
                        'title': {'S': 'Angel Eyes'},
                        'track_number': {'S': '117'}
                    }
                }
            }
        ]
        dynamodb_client = GenerateStubber().client(
            client_type='dynamodb',
            method='batch_write_item',
            response={
                'UnprocessedItems': {}
            },
            expected_params={
                'RequestItems': {
                    'JukeboxSongs': put_request
                }
            }
        )
        self.songIndexData = SongIndexData(
            dynamodb_client=dynamodb_client,
            table_name='foo-bar-table',
            number='117',
            artist='The Jeff Healy Band',
            title='Angel Eyes')

    def test_get_existing_items_found(self):

        # Arrange
        table_name = 'foo-bar-table'
        key = {'S': 'foo-bar'}
        expected_response = {
            'Item': {
                'search_artist': {'S': 'the jeff healy band'},
                'search_title': {'S': 'angel eyes'},
                'track_number': {'S': '117'}
            }
        }
        expected_params={
            'TableName': table_name,
            'Key': {
                'word': key
            }
        }
        dynamodb_get_item_client = GenerateStubber().client(
            client_type='dynamodb',
            method='get_item',
            response=expected_response,
            expected_params=expected_params
        )
        songIndexData = SongIndexData(
            dynamodb_client=dynamodb_get_item_client,
            table_name=table_name,
            number='117',
            artist='The Jeff Healy Band',
            title='Angel Eyes'
        )

        # Act
        response = songIndexData._get_existing_items(key)

        # Assert
        self.assertEqual(response, expected_response)

    def test_get_existing_items_not_found(self):

        # Arrange
        table_name = 'foo-bar-table'
        key = {'S': 'foo-bar'}
        expected_response = {
            'Item': {
                'search_artist': {'S': 'the jeff healy band'},
                'search_title': {'S': 'angel eyes'},
                'track_number': {'S': '117'}
            }
        }
        expected_params={
            'TableName': table_name,
            'Key': {
                'word': key
            }
        }
        dynamodb_get_item_client = GenerateStubber().client_error(
            client_type='dynamodb',
            method='get_item',
            error='ResourceNotFoundException'
        )
        songIndexData = SongIndexData(
            dynamodb_client=dynamodb_get_item_client,
            table_name=table_name,
            number='118',
            artist='The Jeff Healy Band',
            title='Angel Eyes'
        )
        songIndexData.dynamodb_client = dynamodb_get_item_client

        # Act
        response = songIndexData._get_existing_items(key)

        # Assert
        self.assertFalse(response)

    def test_get_number_of_songs_existing(self):
        db_items = {
            'Item': {
                'songs': [
                    {
                        'search_artist': 'The Jeff Healy Band',
                        'search_title': 'Angel Eyes',
                        'track_number': '117'
                    },
                    {
                        'search_artist': 'band yo',
                        'search_title': 'world eyes in your view',
                        'track_number': '216'
                    }
                ]
            }
        }

        # Act
        response = self.songIndexData.get_number_of_songs_existing(db_items)

        # Assert
        self.assertEqual(2, response)

        # TODO: test other cases for get_number_of_songs_existing
        # TODO: why is it comparing search_artist and not artist

    def test_insert_item_ok(self):

        # Arrange
        put_request = [
            {
                'PutRequest': {
                    'Item': {
                        'artist': {'S': 'The Jeff Healy Band'},
                        'search_artist': {'S': 'the jeff healy band'},
                        'search_title': {'S': 'angel eyes'},
                        'title': {'S': 'Angel Eyes'},
                        'track_number': {'S': '117'}
                    }
                }
            }
        ]
        dynamodb_client = GenerateStubber().client(
            client_type='dynamodb',
            method='batch_write_item',
            response={
                'UnprocessedItems': {}
            },
            expected_params={
                'RequestItems': {
                    'JukeboxSongs': put_request
                }
            }
        )
        jukebox_songs = SongIndexData(dynamodb_client=dynamodb_client, number='117', artist='The Jeff Healy Band',
                                      title='Angel Eyes')

        # Act
        jukebox_songs.insert_songs()

        # Assert
        self.assertEqual(put_request, jukebox_songs.batch_dynamodb_values_list[0])
