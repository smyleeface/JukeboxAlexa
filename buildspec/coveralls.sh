#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    ls -la ${CODEBUILD_SRC_DIR}

    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa

    echo "***INFO: Restoring packages"
    dotnet restore

    echo "***INFO: Building packages"
    dotnet build

    echo "***INFO: Testing solution"
    dotnet test

    coverallsToken=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')
    codeCovToken=$(aws ssm get-parameter --name /general/codecov/token  --with-decryption --query  Parameter | jq -r '.Value')

    for directory in ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/JukeboxAlexa.*/ ; do

        ls -la ${directory}
        ls -la ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll

        cd ${CODEBUILD_SRC_DIR}
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

        echo "***INFO: uploading Coveralls"
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
            
        echo "***INFO: uploading CodeCov"
        tools/codecov \
            -f "${directory}coverage.xml" \
            -t ${codeCovToken} \
            -B ${GIT_BRANCH} \
            -C ${GITSHA} \
            -b "${CODEBUILD_BUILD_ID}"
    done

#    S3_BUCKET="dev-smyleegithubeventroutes-codecoveragereports-loog4breq9fw"
#
#    source buildspec/env_vars.sh
#
#    # download files from S3
#    mkdir coverage
#    cd coverage
#    aws s3 cp --recursive s3://${S3_BUCKET}/${REPO}/${GIT_BRANCH}/${GITSHA} ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/
#
#    TOKEN=$(aws ssm get-parameter --name /general/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')
#
#    for COVFILES in ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa/*/coverage.xml; do
#        echo "***INFO: uploading coverage"
#        echo ${COVFILES}
#        ${CODEBUILD_SRC_DIR}/tools/csmacnz.Coveralls \
#            --basePath ${CODEBUILD_SRC_DIR} \
#            --commitId ${GITSHA} \
#            --commitBranch "${GIT_BRANCH}" \
#            --commitAuthor "${GIT_AUTHOR_NAME}" \
#            --commitEmail "${GIT_AUTHOR_EMAIL}" \
#            --commitMessage "${GIT_COMMIT_MESSAGE}" \
#            --jobId "${CODEBUILD_BUILD_ID}" \
#            --useRelativePaths \
#            --opencover \
#            -i ${COVFILES} \
#            --repoToken ${TOKEN}
#    done
fi
