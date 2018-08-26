from tests.generate_stubber import GenerateStubber


dynamo_client = GenerateStubber().client(
    client_type='dynamodb',
    method='get_item',
    response={
        'Attributes': {
            'string': {
                'S': 'string',
                'NULL': False,
                'BOOL': False
            }
        },
        'ConsumedCapacity': {
            'TableName': 'string',
            'CapacityUnits': 123.0,
            'Table': {
                'CapacityUnits': 123.0
            },
            'LocalSecondaryIndexes': {
                'string': {
                    'CapacityUnits': 123.0
                }
            },
            'GlobalSecondaryIndexes': {
                'string': {
                    'CapacityUnits': 123.0
                }
            }
        },
        'ItemCollectionMetrics': {
            'ItemCollectionKey': {
                'string': {
                    'S': 'string',
                    'NULL': False,
                    'BOOL': False
                }
            },
            'SizeEstimateRangeGB': [
                123.0,
            ]
        }
    },
    expected_params={
        'RequestItems': {
            'Key': {
                'word': 'key'
            },
            'UpdateExpression': 'SET songs[2] = :n',
            'ExpressionAttributeValues': {
                ':n': {
                    'word': 'angel',
                    'songs': [
                        {
                            'search_artist': 'the jeff healy band',
                            'search_title': 'angel eyes',
                            'track_number': '117'
                        }
                    ]
                }
            },
            'ReturnValues': 'NONE'
        }
    }
)

