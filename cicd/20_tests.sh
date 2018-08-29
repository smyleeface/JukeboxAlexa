#!/usr/bin/env bash

set -e

cd JukeboxAlexa
dotnet restore
dotnet build
dotnet test