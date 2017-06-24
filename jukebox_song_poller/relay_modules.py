import time

################
# TODO: Need to turn this into a class and send it the gpio
#################


def one(gpio, logger=None):
    if logger:
        logger.info('sending number 1')
    gpio.setup(2, gpio.OUT)
    gpio.output(2, gpio.HIGH)
    time.sleep(1)
    gpio.setup(2, gpio.IN)


def two(gpio, logger=None):
    if logger:
        logger.info('sending number 2')
    gpio.setup(3, gpio.OUT)
    gpio.output(3, gpio.HIGH)
    time.sleep(1)
    gpio.setup(3, gpio.IN)


def three(gpio, logger=None):
    if logger:
        logger.info('sending number 3')
    gpio.setup(4, gpio.OUT)
    gpio.output(4, gpio.HIGH)
    time.sleep(1)
    gpio.setup(4, gpio.IN)


def four(gpio, logger=None):
    if logger:
        logger.info('sending number 4')
    gpio.setup(17, gpio.OUT)
    gpio.output(17, gpio.HIGH)
    time.sleep(1)
    gpio.setup(17, gpio.IN)


def five(gpio, logger=None):
    if logger:
        logger.info('sending number 5')
    gpio.setup(27, gpio.OUT)
    gpio.output(27, gpio.HIGH)
    time.sleep(1)
    gpio.setup(27, gpio.IN)


def six(gpio, logger=None):
    if logger:
        logger.info('sending number 6')
    gpio.setup(22, gpio.OUT)
    gpio.output(22, gpio.HIGH)
    time.sleep(1)
    gpio.setup(22, gpio.IN)


def seven(gpio, logger=None):
    if logger:
        logger.info('sending number 7')
    gpio.setup(10, gpio.OUT)
    gpio.output(10, gpio.HIGH)
    time.sleep(1)
    gpio.setup(10, gpio.IN)


def eight(gpio, logger=None):
    if logger:
        logger.info('sending number 8')
    gpio.setup(9, gpio.OUT)
    gpio.output(9, gpio.HIGH)
    time.sleep(1)
    gpio.setup(9, gpio.IN)


def nine(gpio, logger=None):
    if logger:
        logger.info('sending number 9')
    gpio.setup(11, gpio.OUT)
    gpio.output(11, gpio.HIGH)
    time.sleep(1)
    gpio.setup(11, gpio.IN)


def ten(gpio, logger=None):
    if logger:
        logger.info('sending number 10')
    gpio.setup(5, gpio.OUT)
    gpio.output(5, gpio.HIGH)
    time.sleep(1)
    gpio.setup(5, gpio.IN)


def eleven(gpio, logger=None):
    if logger:
        logger.info('sending number 11')
    gpio.setup(5, gpio.OUT)
    gpio.output(6, gpio.HIGH)
    time.sleep(1)
    gpio.setup(6, gpio.IN)


def twelve(gpio, logger=None):
    if logger:
        logger.info('sending number 12')
    gpio.setup(13, gpio.OUT)
    gpio.output(13, gpio.HIGH)
    time.sleep(1)
    gpio.setup(13, gpio.IN)


def thirteen(gpio, logger=None):
    if logger:
        logger.info('sending number 13')
    gpio.setup(19, gpio.OUT)
    gpio.output(19, gpio.HIGH)
    time.sleep(1)
    gpio.setup(19, gpio.IN)


def fourteen(gpio, logger=None):
    if logger:
        logger.info('sending number 14')
    gpio.setup(26, gpio.OUT)
    gpio.output(26, gpio.HIGH)
    time.sleep(1)
    gpio.setup(26, gpio.IN)


def fifteen(gpio, logger=None):
    if logger:
        logger.info('sending number 15')
    gpio.setup(21, gpio.OUT)
    gpio.output(21, gpio.HIGH)
    time.sleep(1)
    gpio.setup(21, gpio.IN)


def sixteen(gpio, logger=None):
    if logger:
        logger.info('sending number 16')
    gpio.setup(20, gpio.OUT)
    gpio.output(20, gpio.HIGH)
    time.sleep(1)
    gpio.setup(20, gpio.IN)