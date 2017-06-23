import boto3

import RPi.GPIO as GPIO
from song_poller import SongPoller

if __name__ == '__main__':
    boto_session = boto3.session.Session()
    song_poller = SongPoller(boto_session=boto_session, gpio=GPIO)
    song_poller.execute()
