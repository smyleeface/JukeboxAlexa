#!/usr/bin/env bash

set -e

coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query Parameter.Value | sed -e 's/^"//' -e 's/"$//')

for directory in JukeboxAlexa/JukeboxAlexa.*/ ; do

    echo "coverlet ${directory}"
    tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll && \
        --output "${directory}/coverage.xml" && \
        --target dotnet && \
        --targetargs "test ./${directory} --no-build" && \
        --format opencover && \
        --exclude-by-file "**/obj/**" && \
        --exclude-by-file "**/bin/**"

    echo "uploading coverage"
    tools/csmacnz.Coveralls && \
        --commitId $GITSHA && \
        --commitBranch $GIT_BRANCH && \
        --commitAuthor $GIT_AUTHOR&& \
        --commitEmail $GIT_AUTHOR_EMAIL && \
        --commitMessage $GIT_COMMIT_MESSAGE && \
        --jobId $CODEBUILD_BUILD_ID && \
        --useRelativePaths && \
        --opencover && \
        -i ${directory}/coverage.xml && \
        --repoToken ${coverallsToken}
done

