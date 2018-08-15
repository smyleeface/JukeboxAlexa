#!/usr/bin/env bash

project=$1
#
########################
# songlist_index
########################
#
if [[ "${project}" == "songlist_index" ]]; then
    cd /project/songlist_index
#    python /project/songlist_index/setup.py test
#
########################
# songlist_upload
########################
#
elif [[ "${project}" == "songlist_upload" ]]; then
    cd /project/songlist_upload

    echo "***run tests"
    python /project/songlist_upload/setup.py test

    echo "package"

    echo "upload to s3"

    echo "update lambda function"
#
########################
# song_poller
########################
#
elif [[ "${project}" == "song_poller" ]]; then
    cd /project/song_poller

    echo "***run tests"
    python /project/song_poller/setup.py test

    echo "package"

    echo "upload to s3"

    echo "update lambda function"
#
########################
# default
########################
#
else
    $@
fi
