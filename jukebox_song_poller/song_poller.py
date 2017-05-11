import json
import time

import RPi.GPIO as GPIO

import relay_modules as relay
from request_modules import get_song_requested


class SongPoller(object):
    def __init__(self, boto_session):
        self.queue_speed = 1
        self.queue_name_prefix = 'jukebox_request_queue.fifo'
        self.sqs_client = boto_session.client('sqs')
        self.gpio_pin_list = [2, 3, 4, 17, 27, 22, 10, 9, 11, 5, 6, 13, 19, 26, 21, 20]
        self.request_type_function_mapping = {
            'GetSongRequested': 'get_song_requested',
            'GetSongIdRequested': 'get_song_requested'
        }
        self.options = {
            0: relay.zero,
            1: relay.one,
            2: relay.two,
            3: relay.three,
            4: relay.four,
            5: relay.five,
            6: relay.six,
            7: relay.seven,
            8: relay.eight,
            9: relay.nine
        }

        GPIO.setmode(GPIO.BCM)

    def initialize(self):
        """initialize"""

        for i in self.gpio_pin_list:
            GPIO.setup(i, GPIO.OUT)
            GPIO.output(i, GPIO.HIGH)
            time.sleep(1)
            GPIO.setup(i, GPIO.IN)

        for i in self.gpio_pin_list:
            GPIO.setup(i, GPIO.OUT)
            GPIO.output(i, GPIO.HIGH)

        GPIO.cleanup()

    def execute(self):
        """Execute the song poller"""
        queue_url = self.resolve_queue_url()
        try:
            while True:
                messages = self.get_queue_messages(queue_url)

                # there were no messages returned
                if 'Messages' in messages:

                    # loop through all the messages received
                    for message in messages['Messages']:

                        message_str = json.loads(message['Body'])
                        message_type = message_str['parameters']['request_type']
                        message_kargs = {
                            'message_body': message_str,
                            'options': self.options
                        }

                        if message_type in self.request_type_function_mapping:
                            message_type(**message_kargs)

                        receipt_handle = message['ReceiptHandle']
                        self.delete_queue_messages(queue_url, receipt_handle)

                print('No songs in queue.')
                time.sleep(self.queue_speed)

        except KeyboardInterrupt as e:
            print("  Quit")
            GPIO.cleanup()

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
                VisibilityTimeout=30
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
