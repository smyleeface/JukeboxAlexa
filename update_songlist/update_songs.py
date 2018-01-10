#########
# update_songs.py ./JukeboxSongs.csv
#########
import argparse
import boto3
import csv
import logging
import os


class Song(object):
    def __init__(self, number, song, artist, search_title, search_artist):
        self.number = number
        self.song = song
        self.artist = artist
        self.search_title = search_title.lower()
        self.search_artist = search_artist.lower()


class JukeboxSongs(object):
    def __init__(self, path_to_file, region='us-west-2'):
        self.path_to_file = path_to_file
        self.new_songs = []
        self.existing_songs = []
        self.matching_songs = []
        self.added_songs = []
        self.delete_songs = []
        self.batch_dynamodb_values_list = []
        session = boto3.session.Session(profile_name='default', region_name=region)
        self.dynamodb_client = session.client('dynamodb')
        self.logger = self.set_logger()

    @staticmethod
    def set_logger():
        """Setup a logger

        :return logging
        """
        logging.basicConfig()
        logging.Logger('jukebox_song_update')
        logger = logging.getLogger('jukebox_song_update')
        logger.setLevel(logging.INFO)
        return logger

    def run_update(self):
        """Run the commands to update the songs in the database"""
        self._read_uploaded_songs()
        self._read_downloaded_songs()
        # self._filter_matching_songs()
        self._filter_add_songs()
        self._filter_delete_songs()
        self._delete_songs()
        self._add_songs()
        self._update_summary()

    def _read_uploaded_songs(self):
        """Read the songs from csv file"""
        with open(self.path_to_file, 'r') as csvfile:
            song_reader = csv.reader(csvfile, delimiter=',')
            for song_info in song_reader:
                if song_info[0].isdigit() and len(song_info[0]) > 0 and len(song_info[2]) > 0:
                    song = Song(
                        song=song_info[2],
                        artist=song_info[3],
                        number=song_info[0] + song_info[1],
                        search_title=song_info[2],
                        search_artist=song_info[3]
                    )
                    self.new_songs.append(song)

    def _read_downloaded_songs(self):
        """Read the songs from the db"""
        response = self.dynamodb_client.scan(TableName='JukeboxSongs')
        for item in response["Items"]:
            song = Song(
                song=item['title']['S'],
                artist=item['artist']['S'],
                number=item['track_number']['S'],
                search_title=item['search_title']['S'],
                search_artist=item['search_artist']['S']
            )
            self.existing_songs.append(song)

    def _filter_matching_songs(self):
        """Filter for songs that are in both the new list and old list"""
        for existing_song in self.existing_songs:
            for new_song in self.new_songs:
                if new_song.number == existing_song.number \
                        and new_song.song == existing_song.song \
                        and new_song.artist == existing_song.artist:
                    self.matching_songs.append(existing_song)

    def _filter_add_songs(self):
        """Filter for songs that are in the new list but not the old"""
        for new_song in self.new_songs:
            found = False
            for existing_song in self.existing_songs:
                if new_song.number == existing_song.number \
                        and new_song.song == existing_song.song \
                        and new_song.artist == existing_song.artist:
                    found = True
            if not found:
                self.added_songs.append(new_song)

    def _filter_delete_songs(self):
        """Filter for songs that are in the old list but not the new"""
        for existing_song in self.existing_songs:
            found = False
            for new_song in self.new_songs:
                if new_song.number == existing_song.number \
                        and new_song.song == existing_song.song \
                        and new_song.artist == existing_song.artist:
                    found = True
            if not found:
                self.delete_songs.append(existing_song)

    def _add_songs(self):
        """Add the songs in the add songs list. Updates are processed in batches of 25"""
        self.batch_dynamodb_values_list = []
        dynamodb_values = []
        batch_counter = 0
        for song_info in self.added_songs:
            db_record = {
                'PutRequest': {
                    'Item': {
                        'title': {
                            'S': song_info.song,
                        },
                        'artist': {
                            'S': song_info.artist
                        },
                        'track_number': {
                            'S': song_info.number,
                        },
                        'search_title': {
                            'S': song_info.song.lower(),
                        },
                        'search_artist': {
                            'S': song_info.artist.lower()
                        },
                    }
                }
            }
            dynamodb_values.append(db_record)
            batch_counter += 1
            if batch_counter % 25 == 0 or batch_counter == len(self.added_songs):
                self.batch_dynamodb_values_list.append(dynamodb_values)
                dynamodb_values = []
        self.__update_db()

    def _delete_songs(self):
        """Delete the songs in the delete songs list. Updates are processed in batches of 25"""
        self.batch_dynamodb_values_list = []
        dynamodb_values = []
        batch_counter = 0
        for song_info in self.delete_songs:
            db_record = {
                'DeleteRequest': {
                    'Key': {
                        'title': {
                            'S': song_info.song
                        },
                        'artist': {
                            'S': song_info.artist
                        }
                    }
                }
            }
            dynamodb_values.append(db_record)
            batch_counter += 1
            if batch_counter % 25 == 0 or batch_counter == len(self.delete_songs):
                self.batch_dynamodb_values_list.append(dynamodb_values)
                dynamodb_values = []
        self.__update_db()

    def __update_db(self):
        """Updates the songs in the database. Updates are processed in batches of 25"""
        for each in self.batch_dynamodb_values_list:
            self.dynamodb_client.batch_write_item(
                RequestItems={'JukeboxSongs': each}
            )

    def _update_summary(self):
        """Summary of the updates performed"""
        self.logger.info(' Added {0} Songs'.format(len(self.added_songs)))
        self.logger.info(' Deleted {0} Songs'.format(len(self.delete_songs)))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Description')
    parser.add_argument('path_to_file', type=str, help='Path to the file')
    args = parser.parse_args()
    path_to_file = os.path.abspath(args.path_to_file)
    jukebox_songs = JukeboxSongs(path_to_file)
    jukebox_songs.run_update()
