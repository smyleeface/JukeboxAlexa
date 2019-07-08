import json
import time
from datetime import datetime

from relay_modules import RelayModules
from utility_modules import logger_output


class SongPoller(object):
    def __init__(self, boto_session, gpio, logger=None):
        self.queue_speed = 1
        self.queue_name_prefix = 'production-jukebox_request_queue.fifo'
        self.sqs_client = boto_session.client('sqs')
        self.gpio_pin_list = [2, 3, 4, 17, 27, 22, 10, 9, 11, 5, 6, 13, 19, 26, 21, 20]
        self.gpio = gpio
        self.logger = logger
        self.queue_url = None
        self.request_type_function_mapping = {
            'PlaySongTitleRequest': self.get_song_requested,
            'PlaySongTitleArtistRequest': self.get_song_requested,
            'PlaySongNumberRequest': self.get_song_requested,
            'SpeakerRequest': self.get_speaker_request
        }
        relay = RelayModules(gpio=self.gpio, speaker=True, logger=self.logger)
        self.relay_selections = {
            '1': relay.one,
            '2': relay.two,
            '3': relay.three,
            '4': relay.four,
            '5': relay.five,
            '6': relay.six,
            '7': relay.seven,
            '8': relay.eight,
            '9': relay.nine,
            '10': relay.ten,
            '11': relay.eleven,
            '12': relay.twelve,
            '13': relay.thirteen,
            '14': relay.fourteen,
            '15': relay.fifteen,
            '16': relay.sixteen
        }
        self.gpio.setmode(self.gpio.BCM)

    def execute(self):
        """Processes the messages from the queue"""
        self.queue_url = self.resolve_queue_url()
        try:
            while True:
                messages = self.get_queue_messages(self.queue_url)

                # there were no messages returned
                if 'Messages' in messages:

                    # loop through all the messages received
                    for message in messages['Messages']:
                        self.handle_message(json.loads(message.get('Body')), message.get('ReceiptHandle'))

                # It is June 24, 2017 08:06:35AM
                right_now = datetime.now().strftime('It is %B %d, %Y %I:%m%p')
                logger_output(self.logger, 'info', '{0}: No songs in queue.'.format(right_now))
                time.sleep(self.queue_speed)

        except KeyboardInterrupt:
            logger_output(self.logger, 'info', '  Quit')
            self.gpio.cleanup()

    def handle_message(self, message_body, receipt_handle):
        """Handles processing message from the queue"""
        message_type = message_body['request_type']
        message_kargs = {
            'message_body': message_body
        }

        if message_type in self.request_type_function_mapping:
            self.request_type_function_mapping[message_type](**message_kargs)

        self.delete_queue_messages(self.queue_url, receipt_handle)

    def resolve_queue_url(self):
        """Gets the list of queues based on a queue name prefix

        :rtype str
        :return URL of the first queue info returned
        """
        try:
            list_of_queues = self.sqs_client.list_queues(
                QueueNamePrefix=self.queue_name_prefix
            )
            return list_of_queues['QueueUrls'][0]
        except Exception as e:
            raise Exception('Issue with resolving the Queue URL for {0}: {1}'.format(self.queue_name_prefix, e))

    def get_queue_messages(self, queue_url):
        """Get the message from the queue

        :rtype dict
        :return Info with the messages in the queue
        """
        try:
            return self.sqs_client.receive_message(
                QueueUrl=queue_url,
                MaxNumberOfMessages=10,
                VisibilityTimeout=30,
                WaitTimeSeconds=20
            )
        except Exception as e:
            raise Exception('Issue with resolving the getting messages in {0}: {1}'.format(queue_url, e))

    def delete_queue_messages(self, queue_url, receipt_handle):
        """Deletes the message from the queue

        :rtype dict
        :return Info with the messages in the queue
        """
        try:
            return self.sqs_client.delete_message(
                QueueUrl=queue_url,
                ReceiptHandle=receipt_handle
            )
        except Exception as e:
            raise Exception('Issue with deleting the messages in {0} for receipt handle {1}: {2}'.format(queue_url, receipt_handle, e))

    def get_song_requested(self, message_body):
        """Parses the song id and sends each number individually.
        Sending the message body because different requests will parse differently
        """
        song_id = message_body['key']
        for individual_number in song_id:
            self.relay_selections[individual_number]()
            time.sleep(0.5)

    def get_speaker_request(self, song_number):
        """Gets the speaker request and processes"""
        speaker_action = song_number
        message_kargs = {
            'speaker_action': speaker_action
        }
        self.relay_selections['13'](**message_kargs)


