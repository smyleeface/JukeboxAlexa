import boto3
from song_poller import SongPoller


if __name__ == '__main__':
    boto_session = boto3.session.Session()
    song_poller = SongPoller(boto_session=boto_session)
    song_poller.initialize()
    song_poller.execute()
