import unittest
from datetime import datetime

from mock import patch

import botocore.session
import botocore.response
from botocore.stub import Stubber

from jukebox_songs import JukeboxSongs
from tests.generate_stubber import GenerateStubber


class ReadDownloadedSongsTest(unittest.TestCase):

    def setUp(self):

        class ReadUploadedSongs(object):
            def read(self):
                song_record = "1,17,The Jeff Healy Band,Angel Eyes,The Jeff Healy Band,1,17\n7,19,Pink Floyd,Shine On You Crazy Diamonds,Pink Floyd,7,19\n4,83,Tina Turner,What's Love Got to Do With It,Tina Turner,4,83"
                return song_record.encode('utf-8')

        ####################
        # s3 stubber
        ####################
        self.s3_client = GenerateStubber().client(
            client_type='s3',
            method='get_object',
            response={
                'Body': ReadUploadedSongs()
            },
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
            method='scan',
            response={
                'Items': [
                    {
                        'title': {'S': 'Hold On'},
                        'artist': {'S': 'Wilson Phillips'},
                        'track_number': {'S': '495'},
                        'search_title': {'S': 'hold on'},
                        'search_artist': {'S': 'wilson phillips'}
                    }
                ]
            },
            expected_params={'TableName': 'JukeboxSongs'}
        )

    def test_read_downloaded_song_ok(self):

        # Arrange
        dynamo_client = GenerateStubber().client(
            client_type='dynamodb',
            method='scan',
            response={
                'Items': [
                    {
                        'title': {'S': 'Hold On'},
                        'artist': {'S': 'Wilson Phillips'},
                        'track_number': {'S': '495'},
                        'search_title': {'S': 'hold on'},
                        'search_artist': {'S': 'wilson phillips'}
                    }
                ]
            },
            expected_params={'TableName': 'JukeboxSongs'}
        )
        jukebox_songs = JukeboxSongs(
            dynamo_client=dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_downloaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.new_songs), 0)
        self.assertEqual(jukebox_songs.existing_songs[0].song, 'Hold On')
        self.assertEqual(jukebox_songs.existing_songs[0].number, '495')

    def test_read_downloaded_song_multi(self):

        # Arrange
        dynamo_client = GenerateStubber().client(
            client_type='dynamodb',
            method='scan',
            response={
                'Items': [
                    {
                        'title': {'S': 'Hold On'},
                        'artist': {'S': 'Wilson Phillips'},
                        'track_number': {'S': '495'},
                        'search_title': {'S': 'hold on'},
                        'search_artist': {'S': 'wilson phillips'}
                    },
                    {
                        'title': {'S': 'Baby, I Love Your Way'},
                        'artist': {'S': 'Will To Power'},
                        'track_number': {'S': '455'},
                        'search_title': {'S': 'baby, i love your way'},
                        'search_artist': {'S': 'will to power'}
                    },
                    {
                        'title': {'S': 'Song'},
                        'artist': {'S': 'Band'},
                        'track_number': {'S': '140'},
                        'search_title': {'S': 'song'},
                        'search_artist': {'S': 'band'}
                    }
                ]
            },
            expected_params={'TableName': 'JukeboxSongs'}
        )
        jukebox_songs = JukeboxSongs(
            dynamo_client=dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_downloaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.existing_songs), 3)

    def test_read_downloaded_song_empty(self):

        # Arrange
        dynamo_client = GenerateStubber().client(
            client_type='dynamodb',
            method='scan',
            response={
                'Items': []
            },
            expected_params={'TableName': 'JukeboxSongs'}
        )
        jukebox_songs = JukeboxSongs(
            dynamo_client=dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_downloaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.existing_songs), 0)
