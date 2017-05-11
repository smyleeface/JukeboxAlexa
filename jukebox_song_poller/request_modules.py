def get_song_requested(message_body, options):
    song_id = message_body['parameters']['key']
    list_of_numbers = [int(num) for num in str(song_id)]
    for individual_number in list_of_numbers:
        options[individual_number]()
