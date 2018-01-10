import unittest

from mock import MagicMock
from mock import call
from mock import patch

from song_poller import SongPoller
import tests.RPi.GPIO as GPIO


class JukeboxSongPollerTest(unittest.TestCase):

    @staticmethod
    def gpio_setup(gpio):
        gpio.BCM = MagicMock(return_value=gpio.BCM)
        gpio.OUT = MagicMock(return_value=gpio.OUT)
        gpio.IN = MagicMock(return_value=gpio.IN)
        gpio.HIGH = MagicMock(return_value=gpio.HIGH)
        gpio.LOW = MagicMock(return_value=gpio.LOW)
        gpio.setmode = MagicMock(return_value=gpio.setmode(gpio.BCM))
        gpio.cleanup = MagicMock(return_value=gpio.cleanup)
        gpio.setup = MagicMock(return_value=gpio.setup(1, 'string'))
        gpio.output = MagicMock(return_value=gpio.output(2, 'string'))
        return gpio

    def setUp(self):
        with patch('tests.RPi.GPIO') as rpi_library:
            gpio = rpi_library.return_value
            self.rpi_gpio_expected = gpio
            self.rpi_gpio_actual = gpio
            self.gpio_setup(self.rpi_gpio_expected)
            self.gpio_setup(self.rpi_gpio_actual)

        with patch('boto3.client') as boto_session:
            self.boto_session = boto_session.return_value
        self.song_poller = SongPoller(boto_session=self.boto_session, gpio=self.rpi_gpio_actual, logger=MagicMock())

    def test_handle_good_message_GetSongRequested(self):

        # Arrange
        message_body = {
            'request_type': 'GetSongRequested',
            'parameters': {
                'key': '135',
                'message_body': "Sending song number 135, walk away, to the jukebox."
            }
        }
        output_calls = [
            call(2, self.rpi_gpio_expected.HIGH),
            call(4, self.rpi_gpio_expected.HIGH),
            call(27, self.rpi_gpio_expected.HIGH)
        ]
        setup_calls = [
            call(2, self.rpi_gpio_expected.OUT),
            call(2, self.rpi_gpio_expected.IN),
            call(4, self.rpi_gpio_expected.OUT),
            call(4, self.rpi_gpio_expected.IN),
            call(27, self.rpi_gpio_expected.OUT),
            call(27, self.rpi_gpio_expected.IN)
        ]

        # Act
        self.song_poller.handle_message(message_body, "receipt_handle_foo_bar")

        # Assert
        self.rpi_gpio_actual.output.assert_has_calls(output_calls)
        self.rpi_gpio_actual.setup.assert_has_calls(setup_calls)

    def test_handle_good_message_GetSongIdRequested(self):

        # Arrange
        message_body = {
            'request_type': 'GetSongIdRequested',
            'parameters': {
                'key': '135',
                'message_body': "Sending song number 135, walk away, to the jukebox."
            }
        }
        output_calls = [
            call(2, self.rpi_gpio_expected.HIGH),
            call(4, self.rpi_gpio_expected.HIGH),
            call(27, self.rpi_gpio_expected.HIGH)
        ]
        setup_calls = [
            call(2, self.rpi_gpio_expected.OUT),
            call(2, self.rpi_gpio_expected.IN),
            call(4, self.rpi_gpio_expected.OUT),
            call(4, self.rpi_gpio_expected.IN),
            call(27, self.rpi_gpio_expected.OUT),
            call(27, self.rpi_gpio_expected.IN)
        ]

        # Act
        self.song_poller.handle_message(message_body, "receipt_handle_foo_bar")

        # Assert
        self.rpi_gpio_actual.output.assert_has_calls(output_calls)
        self.rpi_gpio_actual.setup.assert_has_calls(setup_calls)

    def test_handle_bad_message(self):

        # Arrange
        message_body = {
            'request_type': 'foo-bar',
            'parameters': {
                'key': '135',
                'message_body': "Sending song number 135, walk away, to the jukebox."
            }
        }
        output_calls = []
        setup_calls = []

        # Act
        self.song_poller.handle_message(message_body, "receipt_handle_foo_bar")

        # Assert
        self.rpi_gpio_actual.output.assert_has_calls(output_calls)
        self.rpi_gpio_actual.setup.assert_has_calls(setup_calls)

    def test_handle_speaker_request_on(self):

        # Arrange
        message_body = {
            'request_type': 'SpeakerRequest',
            'parameters': {
                'key': 'on',
                'message_body': "Turning jukebox speaker on."
            }
        }
        output_calls = [
            call(13, self.rpi_gpio_expected.HIGH)
        ]
        setup_calls = [
            call(13, self.rpi_gpio_expected.OUT)
        ]

        # Act
        self.song_poller.handle_message(message_body, "receipt_handle_foo_bar")

        # Assert
        self.rpi_gpio_actual.output.assert_has_calls(output_calls)
        self.rpi_gpio_actual.setup.assert_has_calls(setup_calls)

    def test_handle_speaker_request_off(self):

        # Arrange
        message_body = {
            'request_type': 'SpeakerRequest',
            'parameters': {
                'key': 'off',
                'message_body': "Turning jukebox speaker off."
            }
        }
        output_calls = []
        setup_calls = [
            call(13, self.rpi_gpio_expected.IN)
        ]

        # Act
        self.song_poller.handle_message(message_body, "receipt_handle_foo_bar")

        # Assert
        self.rpi_gpio_actual.output.assert_has_calls(output_calls)
        self.rpi_gpio_actual.setup.assert_has_calls(setup_calls)
