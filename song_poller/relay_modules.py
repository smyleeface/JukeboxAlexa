import time


class RelayModules(object):

    def __init__(self, gpio, speaker, logger):
        self.gpio = gpio
        self.speaker_on_status = speaker
        self.logger = logger

    def one(self):
        if self.logger:
            self.logger.info('sending number 1')
        self.gpio.setup(2, self.gpio.OUT)
        self.gpio.output(2, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(2, self.gpio.IN)

    def two(self):
        if self.logger:
            self.logger.info('sending number 2')
        self.gpio.setup(3, self.gpio.OUT)
        self.gpio.output(3, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(3, self.gpio.IN)

    def three(self):
        if self.logger:
            self.logger.info('sending number 3')
        self.gpio.setup(4, self.gpio.OUT)
        self.gpio.output(4, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(4, self.gpio.IN)

    def four(self):
        if self.logger:
            self.logger.info('sending number 4')
        self.gpio.setup(17, self.gpio.OUT)
        self.gpio.output(17, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(17, self.gpio.IN)

    def five(self):
        if self.logger:
            self.logger.info('sending number 5')
        self.gpio.setup(27, self.gpio.OUT)
        self.gpio.output(27, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(27, self.gpio.IN)

    def six(self):
        if self.logger:
            self.logger.info('sending number 6')
        self.gpio.setup(22, self.gpio.OUT)
        self.gpio.output(22, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(22, self.gpio.IN)

    def seven(self):
        if self.logger:
            self.logger.info('sending number 7')
        self.gpio.setup(10, self.gpio.OUT)
        self.gpio.output(10, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(10, self.gpio.IN)


    def eight(self):
        if self.logger:
            self.logger.info('sending number 8')
        self.gpio.setup(9, self.gpio.OUT)
        self.gpio.output(9, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(9, self.gpio.IN)

    def nine(self):
        if self.logger:
            self.logger.info('sending number 9')
        self.gpio.setup(11, self.gpio.OUT)
        self.gpio.output(11, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(11, self.gpio.IN)

    def ten(self):
        if self.logger:
            self.logger.info('sending number 10')
        self.gpio.setup(5, self.gpio.OUT)
        self.gpio.output(5, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(5, self.gpio.IN)

    def eleven(self):
        if self.logger:
            self.logger.info('sending number 11')
        self.gpio.setup(5, self.gpio.OUT)
        self.gpio.output(6, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(6, self.gpio.IN)

    def twelve(self):
        if self.logger:
            self.logger.info('sending number 12')
        self.gpio.setup(13, self.gpio.OUT)
        self.gpio.output(13, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(13, self.gpio.IN)

    def thirteen(self):
        if self.logger:
            self.logger.info('sending number 13')
        self.gpio.setup(19, self.gpio.OUT)
        self.gpio.output(19, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(19, self.gpio.IN)

    def fourteen(self):
        if self.logger:
            self.logger.info('sending number 14')
        self.gpio.setup(26, self.gpio.OUT)
        self.gpio.output(26, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(26, self.gpio.IN)

    def fifteen(self):
        if self.logger:
            self.logger.info('sending number 15')
        self.gpio.setup(21, self.gpio.OUT)
        self.gpio.output(21, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(21, self.gpio.IN)

    def sixteen(self):
        if self.logger:
            self.logger.info('sending number 16')
        self.gpio.setup(20, self.gpio.OUT)
        self.gpio.output(20, self.gpio.HIGH)
        time.sleep(1)
        self.gpio.setup(20, self.gpio.IN)
