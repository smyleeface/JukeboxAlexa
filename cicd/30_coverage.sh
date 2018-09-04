#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    source cicd/10_envvars.sh

    coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')

    for directory in ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/JukeboxAlexa.*/ ; do

        ls -la ${directory}
        ls -la ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll

        echo "***INFO: coverlet ${directory}"
        tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
            --output ${directory}coverage.xml \
            --target /usr/bin/dotnet \
            --targetargs "test ${directory} --no-build" \
            --format opencover \
            --exclude-by-file "**/obj/**" \
            --exclude-by-file "**/bin/**"

        ls -la ${directory}
        ls -la ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll

        echo "***INFO: uploading coverage"
        tools/csmacnz.Coveralls \
            --commitId ${GITSHA} \
            --commitBranch "${GIT_BRANCH}" \
            --commitAuthor "${GIT_AUTHOR_NAME}" \
            --commitEmail "${GIT_AUTHOR_EMAIL}" \
            --commitMessage "${GIT_COMMIT_MESSAGE}" \
            --jobId "${CODEBUILD_BUILD_ID}" \
            --useRelativePaths \
            --opencover \
            -i ${directory}coverage.xml \
            --repoToken ${coverallsToken}
    done
fi
