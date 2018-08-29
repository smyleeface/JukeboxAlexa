#!/usr/bin/env bash

for directory in JukeboxAlexa/JukeboxAlexa.*/ ; do
    echo "coverlet ${directory}"
    /project/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll --target dotnet --targetargs "test ${directory} --no-build" --format opencover
done

echo "uploading coverage"
coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query Parameter.Value | sed -e 's/^"//' -e 's/"$//')
/project/tools/csmacnz.Coveralls --opencover -i coverage.opencover.xml --basePath JukeboxAlexa --repoToken ${coverallsToken}
