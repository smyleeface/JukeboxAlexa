![AWS Codebuild Status](https://codebuild.us-west-2.amazonaws.com/badges?uuid=eyJlbmNyeXB0ZWREYXRhIjoiaXlCTjg1bTFCMDlyRjhaWWRkT3BET0RsbVo5UEVXMkpxNnBMd0tvQk1SNEwzbnlSV2dCR0RTYTZyR2lWSmtLQnY5MndybzBWUFBHYUpyTFByNStCei9rPSIsIml2UGFyYW1ldGVyU3BlYyI6Im5MK2VLb0FQaGhjU21hTkkiLCJtYXRlcmlhbFNldFNlcmlhbCI6MX0%3D&branch=master)
[![Coverage Status](https://coveralls.io/repos/github/smyleeface/JukeboxAlexa/badge.svg?branch=master)](https://coveralls.io/github/smyleeface/JukeboxAlexa?branch=master)
[![codecov](https://codecov.io/gh/smyleeface/JukeboxAlexa/branch/master/graph/badge.svg)](https://codecov.io/gh/smyleeface/JukeboxAlexa)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/718db1a14d7643de8ad8f4e035d961dc)](https://www.codacy.com/app/smyleeface/JukeboxAlexa?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=smyleeface/JukeboxAlexa&amp;utm_campaign=Badge_Grade)
[![StackShare](https://img.shields.io/badge/tech-stack-0690fa.svg?style=flat)](https://stackshare.io/smyleeface/jukeboxalexa)

Alexa Jukebox
=============
Alexa skill that will play the song asked on the jukebox, by triggering a relay connected to a number keypad on a jukebox.

![Alexa Jukebox workflow diagram](images/jukebox_diagram.png "Alexa Jukebox workflow diagram")

## Prerequisites
* Setup AWS Account
* Create a user, give permissions, and create user keys
* Jukebox can only be installed in regions that support:
    * SQS FIFO
    * Alexa Skill (Lambda Function)
    * DynamoDB
    * [See list of services and supported regions](https://aws.amazon.com/about-aws/global-infrastructure/regional-product-services/)
* [Docker](https://docker.com/) (optional)

## Setup

### RaspberryPi

* RaspberryPi with Raspbian installed and connected to a 16 port relay
* On both local and RaspberryPi: 
    * Download and install [python3](https://www.python.org/downloads/)
    * Install virutal env (optional)
    * Install AWS CLI `sudo pip install awscli`
    * (later) Configure AWS CLI with User keys generated by the CloudFormation template using `aws configure`

### Alexa Developer Portal

* Start a new Alexa Skill - see steps in [alexa_skill_configuration ReadMe](alexa_skill_configuration/ReadMe.md)

## Development/Deploy

### Local dotnet installation

```bash
cd <PROJECT/ROOT>
lash deploy --tier <TIER>
```

### Docker container

Use the docker container at [smyleeface/lambdasharprunner](https://hub.docker.com/r/smyleeface/lambdasharprunner/)

```bash
cd <PROJECT/ROOT>
docker pull smyleeface/lambdasharprunner
docker run -it --rm --name lambdasharprunner -v $PWD:/project -v $HOME/.aws:/root/.aws smyleeface/lambdasharprunner:latest /bin/bash lash deploy --tier <TIER>
```

## File Structure

### `alexa_skill_configuration`

Setup the Alexa skill in the developer portal.

### `cicd`

Build spec and bash files for CI/CD.

### `cloudformation`

CloudFormation templates to build the cicd infrastructure.

### `song_poller`

Python application running on the Raspberry Pi that polls for song in a queue. 

### `src/JukeboxAlexa`

C# Lambda function Alexa skill and songlist importer.


## Existing Parameters

The following parameters need to exist in the account and region before running `lash`.

* /staging/JukeboxAlexa/deploy/accountid (only on staging account)

* /production/JukeboxAlexa/deploy/accountid (only on prod account)

* /dev/JukeboxAlexa/account/production (only on deploy account)
* /dev/JukeboxAlexa/account/staging (only on deploy account)
* /general/coveralls/token (only on deploy account)



