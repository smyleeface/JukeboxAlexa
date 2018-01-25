import json
from os import path

from copy import deepcopy


class GenerateSkillJson(object):
    def __init__(self):
        self.current_path = path.dirname(path.abspath(__file__))
        self.new_filename = f'{self.current_path}/skill.json'
        self.field_type_value_template = {
            "id": None,
            "name": {
                "value": "",
                "synonyms": []
            }
        }
        self.skill_template = self.load_skill_template()

    def load_skill_template(self):
        with open(f'{self.current_path}/skill_template.json', 'r') as f:
            values = json.load(f)
        return values

    def generate(self):
        self.add_track_numbers()
        self.add_artists()
        self.add_song_title()
        self.add_speaker_request_options()
        self._write_to_file()
        print("DONE")

    def add_track_numbers(self):
        track_numbers = self._load_source_file(f'{self.current_path}/custom_type_TRACKNUMBER.txt')
        track_numbers_json = self._populate_intent_type_template(track_numbers)
        self._add_to_skill_template(track_numbers_json, "TRACKNUMBER")

    def add_artists(self):
        artists = self._load_source_file(f'{self.current_path}/custom_type_ARTISTS.txt')
        artists_json = self._populate_intent_type_template(artists)
        self._add_to_skill_template(artists_json, "ARTISTS")

    def add_song_title(self):
        song_title = self._load_source_file(f'{self.current_path}/custom_type_SONGTITLE.txt')
        song_title_json = self._populate_intent_type_template(song_title)
        self._add_to_skill_template(song_title_json, "SONGTITLE")

    def add_speaker_request_options(self):
        speaker_request_options = self._load_source_file(f'{self.current_path}/custom_type_SPEAKER_REQUEST_OPTIONS.txt')
        speaker_request_options_json = self._populate_intent_type_template(speaker_request_options)
        self._add_to_skill_template(speaker_request_options_json, "SPEAKER_REQUEST_OPTIONS")

    def _load_source_file(self, filename, output_format='json'):
        with open(filename, 'r') as source_file:
            if output_format == 'json':
                return source_file.read().splitlines()
        raise NotImplementedError(f'{output_format} is not a supported type')

    def _populate_intent_type_template(self, source_data):
        json_output = []
        for record in source_data:
            field_type_value = deepcopy(self.field_type_value_template)
            field_type_value["name"]["value"] = record
            json_output.append(field_type_value)
        return json_output

    def _add_to_skill_template(self, json_data, intent_type):
        for index, model_type in enumerate(self.skill_template["languageModel"]["types"]):
            if model_type["name"] == intent_type:
                self.skill_template["languageModel"]["types"][index]["values"] = json_data
                break

    def _write_to_file(self):
        with open(f'{self.current_path}/skill.json', 'w') as skill_json:
            skill_json.writelines(json.dumps(self.skill_template, indent=2))


if __name__ == '__main__':
    generate_skill = GenerateSkillJson()
    generate_skill.generate()
