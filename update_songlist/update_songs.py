import boto3
import csv

# make record values for dynambo
batch_dynamodb_values_list = []
dynamodb_values = []
batch_counter = 0
with open('./JukeboxSongs.csv', 'r') as csvfile:
    song_reader = csv.reader(csvfile, delimiter=',')
    for song_info in song_reader:
        if song_info[0].isdigit() and len(song_info[0]) > 0 and len(song_info[2]) > 0:
            db_record = {
                'PutRequest': {
                    'Item': {
                        'disk_track_number': {
                            'N': song_info[0] + song_info[1],
                        },
                        'song': {
                            'S': song_info[2],
                        },
                        'artist': {
                            'S': song_info[3]
                        }
                    }
                }
            }
            dynamodb_values.append(db_record)
            batch_counter += 1
            if batch_counter == 25:
                batch_dynamodb_values_list.append(dynamodb_values)
                batch_counter = 0
                dynamodb_values = []

# save to dynamo
session = boto3.session.Session(profile_name='default', region_name='us-east-1')
client = session.client('dynamodb')
for each in batch_dynamodb_values_list:
    response = client.batch_write_item(
        RequestItems={'JukeboxSongs': each}
    )
