import unittest
from datetime import datetime

from mock import patch

import botocore.session
import botocore.response
from botocore.stub import Stubber

from jukebox_songs import JukeboxSongs
from song import Song
from tests.generate_stubber import GenerateStubber


class FilterAddSongsTest(unittest.TestCase):
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

    def test_filter_added_song_ok(self):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )
        jukebox_songs.existing_songs = [
            Song(
                artist='Tina Turner',
                number='483',
                search_artist='tina turner',
                search_title="what's love got to do with it",
                song="What's Love Got to Do With It",
            )
        ]
        jukebox_songs.new_songs = [
            Song(
                artist='The Jeff Healy Band',
                number='117',
                search_artist='the jeff healy band',
                search_title='angel eyes',
                song='Angel Eyes'
            )
        ]

        # Act
        jukebox_songs._filter_add_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.added_songs), 1)
        self.assertEqual(jukebox_songs.added_songs[0].number, '117')

    def test_filter_added_song_existing_update(self):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )
        jukebox_songs.existing_songs = [
            Song(
                artist='Tina Turner',
                number='483',
                search_artist='tina turner',
                search_title="what's love got to do with it",
                song="What's Love Got to Do With It",
            )
        ]
        jukebox_songs.new_songs = [
            Song(
                artist='Tina Turner',
                number='333',
                search_artist='tina turner',
                search_title="what's love",
                song="What's Love",
            )
        ]

        # Act
        jukebox_songs._filter_add_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.added_songs), 1)
        self.assertEqual(jukebox_songs.added_songs[0].number, '333')
        self.assertEqual(jukebox_songs.added_songs[0].artist, 'Tina Turner')
        self.assertEqual(jukebox_songs.added_songs[0].song, "What's Love")

    def test_filter_added_song_existing_no_update(self):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )
        jukebox_songs.existing_songs = [
            Song(
                artist='Tina Turner',
                number='483',
                search_artist='tina turner',
                search_title="what's love got to do with it",
                song="What's Love Got to Do With It",
            )
        ]
        jukebox_songs.new_songs = [
            Song(
                artist='Tina Turner',
                number='483',
                search_artist='tina turner',
                search_title="what's love got to do with it",
                song="What's Love Got to Do With It",
            )
        ]

        # Act
        jukebox_songs._filter_add_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.added_songs), 0)

    def test_filter_added_song_existing_not_in_added(self):

        # Arrange
        jukebox_songs = JukeboxSongs(
            dynamo_client=self.dynamo_client,
            s3_client=self.s3_client,
            bucket='foo-bucket',
            key='key/bar.baz'
        )
        jukebox_songs.existing_songs = [
            Song(
                artist='Wilson Phillips',
                number='495',
                search_artist='wilson phillips',
                search_title='hold on',
                song='Hold On'
            ),
            Song(
                artist='Tina Turner',
                number='483',
                search_artist='tina turner',
                search_title="what's love got to do with it",
                song="What's Love Got to Do With It",
            )
        ]
        jukebox_songs.new_songs = [
            Song(
                artist='Tina Turner',
                number='483',
                search_artist='tina turner',
                search_title="what's love got to do with it",
                song="What's Love Got to Do With It",
            )
        ]

        # Act
        jukebox_songs._filter_add_songs()

        # Assert
        self.assertEqual(len(jukebox_songs.added_songs), 0)