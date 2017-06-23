import unittest

from mock import MagicMock
from mock import call
from mock import patch

from song_poller import SongPoller
import RPi.GPIO


class JukeboxSongPollerTest(unittest.TestCase):

    def setUp(self):
        with patch('RPi.GPIO') as rpi_library:
            self.rpi_gpio = rpi_library.return_value
            self.rpi_gpio.BCM = MagicMock(return_value=rpi_library.BCM)
            self.rpi_gpio.OUT = MagicMock(return_value=rpi_library.OUT)
            self.rpi_gpio.IN = MagicMock(return_value=rpi_library.IN)
            self.rpi_gpio.HIGH = MagicMock(return_value=rpi_library.HIGH)
            self.rpi_gpio.LOW = MagicMock(return_value=rpi_library.LOW)
            self.rpi_gpio.setmode = MagicMock(return_value=rpi_library.setmode(self.rpi_gpio.BCM))
            self.rpi_gpio.cleanup = MagicMock(return_value=rpi_library.cleanup)
            self.rpi_gpio.setup = MagicMock(return_value=rpi_library.setup(1, 'string'))
            self.rpi_gpio.output = MagicMock(return_value=rpi_library.output(2, 'string'))
        with patch('boto3.client') as boto_session:
            self.boto_session = boto_session.return_value
        self.song_poller = SongPoller(boto_session=self.boto_session, gpio=self.rpi_gpio)

    def test_handle_message(self):

        # Arrange
        message_body = {
            'request_type': 'GetSongRequested',
            'parameters': {
                'key': '135',
                'message_body': "Sending song number 135, walk away, to the jukebox."
            }
        }
        output_calls = [
            call(2, self.rpi_gpio.HIGH),
            call(4, self.rpi_gpio.HIGH),
            call(27, self.rpi_gpio.HIGH)
        ]
        setup_calls = [
            call(2, self.rpi_gpio.OUT),
            call(2, self.rpi_gpio.IN),
            call(4, self.rpi_gpio.OUT),
            call(4, self.rpi_gpio.IN),
            call(27, self.rpi_gpio.OUT),
            call(27, self.rpi_gpio.IN)
        ]

        # Act
        self.song_poller.handle_message(message_body, "receipt_handle_foo_bar")

        # Assert
        self.rpi_gpio.output.assert_has_calls(output_calls)
        self.rpi_gpio.setup.assert_has_calls(setup_calls)


if __name__ == '__main__':
    unittest.main()
