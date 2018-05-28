Update Songlist
===============

1. Login to AWS
1. Go to the `S3` Service and find the bucket name that was entered in the CloudFormation.
1. Upload the jukebox CSV file.
    1. File name can be named anything with no spaces as long as it's a CSV
    1. Comma delimited
    1. Song Titles or Artists with a comma need to be in double quotes
    1. Including a heading is ok and optional
    1. Format: 
        ```
        Disc,Track#,Extra,Song,Artist
        3,19,Neon Trees,Animals,Neon Trees
        ```
    1. Extra data after the artist will be ignored as long as a comma is after the `Artist`
        ```
        Disc,Track#,Extra,Song,Artist
        3,19,Neon Trees,Animals,Neon Trees,3,19,,,,,,
        ```
1. The upload will trigger the update process and should take no more than one minute.

## Troubleshooting

> Cannot find `index.py` when handler is `lambda_function.lambda_handler` and file is named `lambda_function.py`.

Use the handler `index.lambda_handler`. Since there is no deployment package name when submitting in-line code via ZipFile and CloudFormation, the prefix index is used instead. [Source](https://stackoverflow.com/questions/47106571/bad-handler-when-deploying-lambda-function-via-cloudformation-zipfile)

> Run Logs

For run logs look in `CloudWatch > Log` for Log Group `/aws/lambda/jukebox_song_list_upload`
[Find the log stream with the latest run.](https://us-west-2.console.aws.amazon.com/cloudwatch/home?region=us-west-2#logStream:group=/aws/lambda/jukebox_song_list_upload)
