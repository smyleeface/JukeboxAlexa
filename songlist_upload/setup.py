#!/usr/bin/env python

from setuptools import setup

setup(
    name='SongListUpload',
    version='1.0',
    description='Adds songs from a CSV to a dynamodb',
    author='PattyR (smyleeface)',
    author_email='patty.ramert@gmail.com',
    install_requires=[
        'boto3>=1.5.4',
        'argparse>=1.4.0'
    ],
    tests_require=[
        'mock>=2.0.0,<3.0.0'
    ],
    test_suite='tests.songlist_upload_testsuite',
)


