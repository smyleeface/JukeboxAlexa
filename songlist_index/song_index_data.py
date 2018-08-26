import csv
import logging
import boto3

from song import Song


class SongIndexData(object):
    def __init__(self, dynamodb_client: boto3.Session.client, table_name: str, number: str, artist: str, title: str,
                 logger: logging = None):
        self.dynamodb_client = dynamodb_client
        self.table_name = table_name
        self.number = number
        self.artist = artist
        self.title = title
        self.title_words = title.lower().split(' ')
        self.logger = logger

    def log(self, message: str, log_type: str = 'info'):
        """Setup a logger

        :return logging
        """
        if self.logger:
            if log_type.lower() == 'error':
                self.logger.error(message)
            elif log_type.lower() == 'warn':
                self.logger.warn(message)
            else:
                self.logger.info(message)

    def _insert_item(self, key, song):

        # Add the word to the list
        self.dynamodb_client.put_item(
            Item={
                'word': key,
                'songs': song
            }
        )

    def _get_existing_items(self, key):
        try:
            return self.dynamodb_client.get_item(
                TableName=self.table_name,
                Key={
                    'word': key
                }
            )
        except Exception as e:
            if e.response['Error']['Code'] == 'ResourceNotFoundException':
                return False
            else:
                raise e

    def insert_songs(self):
        for title_word in self.title_words:
            self.insert_song(title_word)

    def insert_song(self, key):
        db_items = self._get_existing_items(key)
        if db_items:
            total_songs = self.get_number_of_songs_existing(db_items)
            if total_songs != 0:

                # ADD ITEM TO A LIST
                set_string = f"SET songs[{total_songs}] = :n"
                self.dynamodb_client.update_item(
                    Key={
                        'word': key
                    },
                    UpdateExpression=set_string,
                    ExpressionAttributeValues={
                        ':n': {
                            'search_artist': self.artist,
                            'track_number': self.number,
                            'search_title': self.title
                        }
                    },
                    ReturnValues="NONE"
                )
            else:
                song = [{
                    'search_artist': self.artist,
                    'track_number': self.number,
                    'search_title': self.title
                }]
                self._insert_item(key, song)

    def delete_songs(self):
        for title_word in self.title_words:
            self.delete_song(title_word)

    def delete_song(self, key):
        try:

            # Get existing items
            db_items = self.dynamodb_client.get_item(
                Key={
                    'word': key
                }
            )
            # find this song in existing items
            if db_items.get('Item'):
                for id, song in enumerate(db_items['Item']['songs']):
                    if song['search_title'] == self.title and song['search_artist'] == self.artist and song['track_number'] == self.number:

                        # REMOVE ITEM FROM LIST
                        set_string = f'REMOVE songs[{id}]'
                        self.dynamodb_client.update_item(
                            Key={
                                'word': key
                            },
                            UpdateExpression=set_string,
                            ReturnValues="NONE"
                        )
        except Exception as e:
            pass

    def get_number_of_songs_existing(self, db_items):

        # find this song in existing items
        if db_items.get('Item'):
            for id, song in enumerate(db_items['Item']['songs']):
                if song['search_title'] == self.title and song['search_artist'] == self.artist and song['track_number'] == self.number:
                    return len(db_items['Item']['songs'])
        return 0
