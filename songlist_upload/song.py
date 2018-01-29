class Song(object):
    def __init__(self, number, song, artist, search_title, search_artist):
        self.number = number
        self.song = song
        self.artist = artist
        self.search_title = search_title.lower()
        self.search_artist = search_artist.lower()
