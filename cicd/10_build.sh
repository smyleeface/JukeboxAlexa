#!/usr/bin/env bash

for directory in ../JukeboxAlexa.*/ ; do
    echo "coverlet $directory"
    coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll --target dotnet --targetargs "test ${directory} --no-build" --format opencover
done

coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query Parameter.Value | sed -e 's/^"//' -e 's/"$//')
csmacnz.coveralls --opencover -i coverage.opencover.xml --basePath JukeboxAlexa --repoToken ${coverallsToken}
