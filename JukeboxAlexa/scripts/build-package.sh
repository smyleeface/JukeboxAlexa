#!/usr/bin/env bash

BASE_DIR=${CODEBUILD_SRC_DIR}

echo '[INFO] change to the project directory'
cd ${BASE_DIR}/JukeboxAlexa

echo '[INFO] restore package and publish'
dotnet restore
dotnet publish

echo '[INFO] zip the compilied files'
cd ${BASE_DIR}/JukeboxAlexa/bin/Debug/netcoreapp2.0/publish
zip -r jukeboxAlexa.zip .