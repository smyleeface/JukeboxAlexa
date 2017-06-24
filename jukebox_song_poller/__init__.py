import logging

import boto3

import RPi.GPIO as GPIO
from song_poller import SongPoller

if __name__ == '__main__':

    # logging setup
    logging.basicConfig()
    logging.Logger('song_poller')
    logger = logging.getLogger('song_poller')
    logger.setLevel(logging.INFO)

    # run song poller
    boto_session = boto3.session.Session()
    song_poller = SongPoller(boto_session=boto_session, gpio=GPIO, logger=logger)
    song_poller.execute()
