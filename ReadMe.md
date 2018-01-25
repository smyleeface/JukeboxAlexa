[![Build Status](https://travis-ci.org/smyleeface/smylee_jukebox.svg?branch=master)](https://travis-ci.org/smyleeface/smylee_jukebox)  [![StackShare](https://img.shields.io/badge/tech-stack-0690fa.svg?style=flat)](https://stackshare.io/smyleeface/smylee-jukebox)

Alexa Jukebox
=============
Alexa skill that will play the song asked on the jukebox, by triggering a relay connected to a number keypad on a jukebox.

## Prerequisites
* Setup AWS Account
* Create a user, give permissions, and create user keys
* RaspberryPi with Raspbian installed and connected to a 16 port relay
* On both local and RaspberryPi: 
    * Download and install [python3](https://www.python.org/downloads/)
    * Install virutal env (optional)
    * Install AWS CLI `sudo pip install awscli`
    * Configure AWS CLI with User keys `aws configure`

## alexa_skill_configuration

Information on how to setup the Alexa skill in the developer portal.

## cloudformation

CloudFormation templates to build the Jukebox infrastructure.

## files_decrypt

Helper function to decrypt files from CodeBuild.

## JukeboxAlexa

C# Lambda function Alexa skill.

## song_poller

Python application running on the Raspberry Pi that polls for song in a queue. 

## songlist_index

Manages the cached words of the song titles in the database.

## update_songlist

Run locally to update the songs in the database.