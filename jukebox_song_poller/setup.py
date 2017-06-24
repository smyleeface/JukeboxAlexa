from setuptools import find_packages, setup

setup(
    name='jukebox_song_poller',
    author='PattyR',
    url='http://smylee.com/',
    download_url='http://github.com/jukebox_song_poller',
    install_requires=['boto3>=1.4.4'],
    tests_require=['mock>=2.0.0'],
    packages=find_packages('jukebox_song_poller', 'tests.RPi'),
    test_suite="tests.__init__"
)
