#!/usr/bin/env bash

echo "change to publish directory"
cd /app/publish

echo "zip directory"
zip -r jukeboxAlexa.zip .

echo "upload to s3"
aws s3 cp jukeboxAlexa.zip s3://smyleeface-public/JukeboxAlexa/LambdaJukeboxAlexa/jukeboxAlexa.zip