import unittest
from datetime import datetime

from mock import patch

import botocore.session
import botocore.response
from botocore.stub import Stubber

from jukebox_songs import JukeboxSongs
from tests.generate_stubber import GenerateStubber


class ReadUploadedSongsTest(unittest.TestCase):
    song_list = [
        ['7', '19', 'Pink Floyd', 'Shine On You Crazy Diamonds', 'Pink Floyd', '7', '19']
    ]
    song_list_multi = [
        ['1', '17', 'The Jeff Healy Band', 'Angel Eyes', 'The Jeff Healy Band', '1', '17'],
        ['7', '19', 'Pink Floyd', 'Shine On You Crazy Diamonds', 'Pink Floyd', '7', '19'],
        ['4', '83', 'Tina Turner', "What's Love Got to Do With It", 'Tina Turner', '4', '83']
    ]
    song_list_missing_fields = [
        ['1', '17', '', '', '', '', '']
    ]
    song_list_empty = [
        []
    ]
    song_list_heading = [
        ['foo', 'bar', 'baz', 'foo', 'bar', 'foo', 'bar']
    ]

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
                'Items': []
            },
            expected_params={'Bucket': 'test-bucket'}
        )

    @patch('jukebox_songs.csv.reader', return_value=song_list)
    def test_read_uploaded_song_ok(self, csv_reader_mock):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_uploaded_songs()

        # Assert
        self.assertEqual(jukebox_songs.new_songs[0].artist, 'Pink Floyd')
        self.assertEqual(jukebox_songs.new_songs[0].number, '719')

    @patch('jukebox_songs.csv.reader', return_value=song_list_multi)
    def test_read_uploaded_song_ok_multi(self, csv_reader_mock):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_uploaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.new_songs), 3)

    @patch('jukebox_songs.csv.reader', return_value=song_list_missing_fields)
    def test_read_uploaded_song_missing_fields(self, csv_reader_mock):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_uploaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.new_songs), 0)

    @patch('jukebox_songs.csv.reader', return_value=song_list_empty)
    def test_read_uploaded_song_list_empty(self, csv_reader_mock):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_uploaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.new_songs), 0)

    @patch('jukebox_songs.csv.reader', return_value=song_list_heading)
    def test_read_uploaded_song_list_heading(self, csv_reader_mock):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )

        # Act
        jukebox_songs._read_uploaded_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.new_songs), 0)
