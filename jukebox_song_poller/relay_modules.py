import time

################
# TODO: Need to turn this into a class and send it the gpio
#################


def one(GPIO):
    print('sending number 1')
    GPIO.setup(2, GPIO.OUT)
    GPIO.output(2, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(2, GPIO.IN)


def two(GPIO):
    print('sending number 2')
    GPIO.setup(3, GPIO.OUT)
    GPIO.output(3, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(3, GPIO.IN)


def three(GPIO):
    print('sending number 3')
    GPIO.setup(4, GPIO.OUT)
    GPIO.output(4, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(4, GPIO.IN)


def four(GPIO):
    print('sending number 4')
    GPIO.setup(17, GPIO.OUT)
    GPIO.output(17, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(17, GPIO.IN)


def five(GPIO):
    print('sending number 5')
    GPIO.setup(27, GPIO.OUT)
    GPIO.output(27, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(27, GPIO.IN)


def six(GPIO):
    print('sending number 6')
    GPIO.setup(22, GPIO.OUT)
    GPIO.output(22, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(22, GPIO.IN)


def seven(GPIO):
    print('sending number 7')
    GPIO.setup(10, GPIO.OUT)
    GPIO.output(10, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(10, GPIO.IN)


def eight(GPIO):
    print('sending number 8')
    GPIO.setup(9, GPIO.OUT)
    GPIO.output(9, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(9, GPIO.IN)


def nine(GPIO):
    print('sending number 9')
    GPIO.setup(11, GPIO.OUT)
    GPIO.output(11, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(11, GPIO.IN)


def ten(GPIO):
    print('sending number 10')
    GPIO.setup(5, GPIO.OUT)
    GPIO.output(5, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(5, GPIO.IN)


def eleven(GPIO):
    print('sending number 11')
    GPIO.setup(5, GPIO.OUT)
    GPIO.output(6, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(6, GPIO.IN)


def twelve(GPIO):
    print('sending number 12')
    GPIO.setup(13, GPIO.OUT)
    GPIO.output(13, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(13, GPIO.IN)


def thirteen(GPIO):
    print('sending number 13')
    GPIO.setup(19, GPIO.OUT)
    GPIO.output(19, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(19, GPIO.IN)


def fourteen(GPIO):
    print('sending number 14')
    GPIO.setup(26, GPIO.OUT)
    GPIO.output(26, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(26, GPIO.IN)


def fifteen(GPIO):
    print('sending number 15')
    GPIO.setup(21, GPIO.OUT)
    GPIO.output(21, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(21, GPIO.IN)


def sixteen(GPIO):
    print('sending number 16')
    GPIO.setup(20, GPIO.OUT)
    GPIO.output(20, GPIO.HIGH)
    time.sleep(1)
    GPIO.setup(20, GPIO.IN)