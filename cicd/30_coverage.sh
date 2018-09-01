#!/usr/bin/env bash

set -e

#dotnet tool install coveralls.net --tool-path tools
#dotnet tool install coverlet.console --tool-path tools

dotnet restore JukeboxAlexa/
dotnet build JukeboxAlexa/

coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query Parameter.Value | sed -e 's/^"//' -e 's/"$//')

for directory in JukeboxAlexa/JukeboxAlexa.*/ ; do
    echo "coverlet ${directory}"
    tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll --output "${directory}/coverage.xml" --target dotnet --targetargs "test ./${directory} --no-build" --format opencover --exclude-by-file "**/obj/**"
    echo "uploading coverage"
    tools/csmacnz.Coveralls --commitId $CODEBUILD_SOURCE_VERSION --useRelativePaths --opencover -i ${directory}/coverage.xml --repoToken ${coverallsToken}
done

