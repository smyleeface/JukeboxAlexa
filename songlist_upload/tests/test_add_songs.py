import unittest

from jukebox_songs import JukeboxSongs
from song import Song
from tests.generate_stubber import GenerateStubber


class AddSongsTest(unittest.TestCase):

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
                }
            }
        )

    def test_add_song_ok(self):

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
        dynamo_client = GenerateStubber().client(
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
        jukebox_songs = JukeboxSongs(
            dynamo_client=dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )
        jukebox_songs.added_songs = [
            Song(
                artist='The Jeff Healy Band',
                number='117',
                search_artist='the jeff healy band',
                search_title='angel eyes',
                song='Angel Eyes'
            )
        ]

        # Act
        jukebox_songs._add_songs()

        # Assert
        self.assertEqual(put_request, jukebox_songs.batch_dynamodb_values_list[0])
