#!/usr/bin/env bash

set -e

if [[ ${CODEBUILD_BUILD_SUCCEEDING} ]]; then

    cd ${CODEBUILD_SRC_DIR}/src/JukeboxAlexa
    
    echo "***INFO: Building packages"
    dotnet build
    
    cd ${CODEBUILD_SRC_DIR}
    
    coverallsToken=$(aws ssm get-parameter --name /account/coveralls/token  --with-decryption --query  Parameter | jq -r '.Value')
    codeCovToken=$(aws ssm get-parameter --name /account/codecov/token  --with-decryption --query  Parameter | jq -r '.Value')
    codacyToken=$(aws ssm get-parameter --name /account/codacy/token  --with-decryption --query  Parameter | jq -r '.Value')

    for directory in ./src/JukeboxAlexa/JukeboxAlexa.*/ ; do

        echo "****PROCESSING DIRECTORY: ${directory}"

        cd ${CODEBUILD_SRC_DIR}

        # GENERATE REPORTS - opencover
        echo "***INFO: coverlet ${directory}"
        /opt/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
            --output ${directory}coverage-opencover.xml \
            --target /usr/bin/dotnet \
            --targetargs "test ${directory} --no-build" \
            --format opencover \
            --exclude-by-file "**/obj/**" \
            --exclude-by-file "**/bin/**"

        # GENERATE REPORTS - lcov
        /opt/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
            --output ${directory}coverage-lcov.json \
            --target /usr/bin/dotnet \
            --targetargs "test ${directory} --no-build" \
            --format lcov \
            --exclude-by-file "**/obj/**" \
            --exclude-by-file "**/bin/**"  

#        # GENERATE REPORTS - cobertura
#        ${CODEBUILD_SRC_DIR}/tools/coverlet ${directory}bin/Debug/netcoreapp2.1/xunit.runner.visualstudio.dotnetcore.testadapter.dll \
#            --output ${directory}coverage-cobertura.xml \
#            --target /usr/bin/dotnet \
#            --targetargs "test ${directory} --no-build" \
#            --format cobertura \
#            --exclude-by-file "**/obj/**" \
#            --exclude-by-file "**/bin/**"

        ############################
        # UPLOAD REPORT - Coveralls
        echo "***INFO: uploading Coveralls"
        /opt/tools/csmacnz.Coveralls \
            --commitId ${GITSHA} \
            --commitBranch "${GIT_BRANCH}" \
            --commitAuthor "${GIT_AUTHOR_NAME}" \
            --commitEmail "${GIT_AUTHOR_EMAIL}" \
            --commitMessage "${GIT_COMMIT_MESSAGE}" \
            --jobId "${CODEBUILD_BUILD_ID}" \
            --useRelativePaths \
            --opencover \
            -i ${directory}coverage-opencover.xml \
            --repoToken "${coverallsToken}" 

        # UPLOAD REPORT - CodeCov
        echo "***INFO: uploading CodeCov"
        /opt/tools/codecov \
            -f ${directory}coverage-lcov.json \
            -t ${codeCovToken} \
            -B ${GIT_BRANCH} \
            -C ${GITSHA} \
            -b ${directory}coverage-lcov.json

#        # UPLOAD REPORT - Codacy
#        echo "***INFO: uploading Codacy"
#        python-codacy-coverage \
#            -r ${directory}coverage-cobertura.xml \
#            -t ${codacyToken} \
#            -c ${GITSHA} \
#            -d ${CODEBUILD_SRC_DIR}
    done
fi
