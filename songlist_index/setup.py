#!/usr/bin/env python

from setuptools import setup

setup(
    name='SongListIndex',
    version='1.0',
    description='Index words from song title from a dynamodb stream to another dynamodb',
    author='PattyR (smyleeface)',
    author_email='patty.ramert@gmail.com',
    install_requires=[
        'boto3>=1.5.4'
    ],
    tests_require=[
        'mock>=2.0.0,<3.0.0'
    ],
    test_suite='tests.songlist_index_testsuite',
)


