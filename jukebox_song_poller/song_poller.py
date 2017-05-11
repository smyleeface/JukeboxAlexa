import boto3
import RPi.GPIO as GPIO
import time
import json


def relay_zero():
    print('sending number 0')
    GPIO.setup(2, GPIO.OUT)
    GPIO.output(2, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(2, GPIO.IN)


def relay_one():
    print('sending number 1')
    GPIO.setup(3, GPIO.OUT)
    GPIO.output(3, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(3, GPIO.IN)


def relay_two():
    print('sending number 2')
    GPIO.setup(4, GPIO.OUT)
    GPIO.output(4, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(4, GPIO.IN)


def relay_three():
    print('sending number 3')
    GPIO.setup(17, GPIO.OUT)
    GPIO.output(17, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(17, GPIO.IN)


def relay_four():
    print('sending number 4')
    GPIO.setup(27, GPIO.OUT)
    GPIO.output(27, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(27, GPIO.IN)


def relay_five():
    print('sending number 5')
    GPIO.setup(22, GPIO.OUT)
    GPIO.output(22, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(22, GPIO.IN)


def relay_six():
    print('sending number 6')
    GPIO.setup(10, GPIO.OUT)
    GPIO.output(10, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(10, GPIO.IN)


def relay_seven():
    print('sending number 7')
    GPIO.setup(9, GPIO.OUT)
    GPIO.output(9, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(9, GPIO.IN)


def relay_eight():
    print('sending number 8')
    GPIO.setup(11, GPIO.OUT)
    GPIO.output(11, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(11, GPIO.IN)


def relay_nine():
    print('sending number 9')
    GPIO.setup(5, GPIO.OUT)
    GPIO.output(5, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(5, GPIO.IN)


def relay_ten():
    print('sending number 10')
    GPIO.setup(5, GPIO.OUT)
    GPIO.output(6, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(6, GPIO.IN)


def relay_eleven():
    print('sending number 11')
    GPIO.setup(13, GPIO.OUT)
    GPIO.output(13, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(13, GPIO.IN)


def relay_twelve():
    print('sending number 12')
    GPIO.setup(19, GPIO.OUT)
    GPIO.output(19, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(19, GPIO.IN)


def relay_thirteen():
    print('sending number 13')
    GPIO.setup(26, GPIO.OUT)
    GPIO.output(26, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(26, GPIO.IN)


def relay_fourteen():
    print('sending number 14')
    GPIO.setup(21, GPIO.OUT)
    GPIO.output(21, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(21, GPIO.IN)


def relay_fifteen():
    print('sending number 15')
    GPIO.setup(20, GPIO.OUT)
    GPIO.output(20, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(20, GPIO.IN)


def get_song_requested(message_body):
    song_id = message_body['parameters']['key']
    list_of_numbers = [int(num) for num in str(song_id)]
    for individual_number in list_of_numbers:
        options[individual_number]()

options = {
    0: relay_zero,
    1: relay_one,
    2: relay_two,
    3: relay_three,
    4: relay_four,
    5: relay_five,
    6: relay_six,
    7: relay_seven,
    8: relay_eight,
    9: relay_nine
}

gpio_pin_list = [2, 3, 4, 17, 27, 22, 10, 9, 11, 5, 6, 13, 19, 26, 21, 20]

request_type_function_mapping = {
    'GetSongRequested': 'get_song_requested',
    'GetSongIdRequested': 'get_song_requested'
}

# number of seconds between polls
queue_speed = 1

# get the queue info
queue_name_prefix = 'jukebox_request_queue.fifo'
sqs_client = boto3.client('sqs')
list_of_queues = sqs_client.list_queues(
    QueueNamePrefix=queue_name_prefix
)
queue_url = list_of_queues['QueueUrls'][0]

# initialize
GPIO.setmode(GPIO.BCM)

for i in gpio_pin_list:
    GPIO.setup(i, GPIO.OUT)
    GPIO.output(i, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(i, GPIO.IN)

for i in gpio_pin_list:
    GPIO.setup(i, GPIO.OUT)
    GPIO.output(i, GPIO.HIGH)

GPIO.cleanup()

try:
    while True:

        # get messages for the queue url
        messages = sqs_client.receive_message(
            QueueUrl=queue_url,
            MaxNumberOfMessages=10,
            VisibilityTimeout=30
        )

        # if there were no messages returned stop run
        while 'Messages' in messages:

            # loop through all the messages received
            for message in messages['Messages']:

                message_str = json.loads(message['Body'])
                message_type = message_str['parameters']['request_type']

                if message_type in request_type_function_mapping:
                    message_type(message_str)

                receipt_handle = message['ReceiptHandle']

                # delete message; message processed ok.
                response = sqs_client.delete_message(
                    QueueUrl=queue_url,
                    ReceiptHandle=receipt_handle
                )

        print('No songs in queue.')
        time.sleep(queue_speed)

except KeyboardInterrupt as e:
    print("  Quit")
    GPIO.cleanup()
