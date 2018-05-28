import unittest
from datetime import datetime

from mock import patch

import botocore.session
import botocore.response
from botocore.stub import Stubber

from jukebox_songs import JukeboxSongs
from song import Song
from tests.generate_stubber import GenerateStubber


class DeleteSongsTest(unittest.TestCase):

    def setUp(self):

        ####################
        # s3 stubber
        ####################
        self.s3_client = GenerateStubber().client(
            client_type='s3',
            method='get_object',
            response={},
            expected_params={
                'Bucket': 'foo-bucket',
                'Key': 'key/bar.baz'
            }
        )

        ####################
        # dynamo stubber
        ####################
        self.dynamo_client = GenerateStubber().client(
            client_type='dynamodb',
            method='batch_write_item',
            response={
                'UnprocessedItems': {}
            },
            expected_params={
                'RequestItems': {
                    'JukeboxSongs': [
                        {
                            'DeleteRequest': {
                                'Key': {
                                    'artist': {'S': 'The Jeff Healy Band'},
                                    'title': {'S': 'Angel Eyes'}
                                }
                            }
                        }
                    ]
                }
            }
        )

    def test_delete_song_ok(self):

        # Arrange
        delete_request = [
            {
                'DeleteRequest': {
                    'Key': {
                        'artist': {'S': 'The Jeff Healy Band'},
                        'title': {'S': 'Angel Eyes'}
                    }
                }
            }
        ]
        dynamo_client = GenerateStubber().client(
            client_type='dynamodb',
            method='batch_write_item',
            response={
                'UnprocessedItems': {}
            },
            expected_params={
                'RequestItems': {
                    'JukeboxSongs': delete_request
                }
            }
        )
        jukebox_songs = JukeboxSongs(
            dynamo_client=dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )
        jukebox_songs.delete_songs = [
            Song(
                artist='The Jeff Healy Band',
                number='117',
                search_artist='the jeff healy band',
                search_title='angel eyes',
                song='Angel Eyes'
            )
        ]

        # Act
        jukebox_songs._delete_songs()

        # Assert
        self.assertEqual(delete_request, jukebox_songs.batch_dynamodb_values_list[0])
