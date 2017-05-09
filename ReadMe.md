Alexa Jukebox
=============
Alexa skill to trigger numbers from a relay to the jukebox.

## Prerequisites
* Setup AWS Account
* Create a user, give permissions, and create user keys
* Download and install [python3](https://www.python.org/downloads/)
* Install virutal env (optional)
* Install AWS CLI `sudo pip install awscli --ignore-installed six`
* Configure AWS CLI with User keys `aws configure`

##Setup Alexa Skill
On the [Alexa developer portal](https://developer.amazon.com):
1. Create a new Alexa Skill called `jukebox`
2. Use the `jukebox_alexa_skill/IntentSchema.json` and the `jukebox_alexa_skill/Utterances.txt` on the `Interaction Model` tab.
3. Stop on the `Configuration` tab.
4. Get the `Skill id`; it is needed during the `Setup AWS` section below.

##Setup AWS
From the root directory of the project you can run the following commands:

###Run install script
```bash
bash alexa-jukebox install-stacks
```

###Update stack with the latest changes
```bash
bash alexa-jukebox update-stacks
```

###Update songs in DyanmoDB
```bash
bash alexa-jukebox update-songs
```

###Get information from the stacks
```bash
bash alexa-jukebox info
```

###Delete stacks
```bash
bash alexa-jukebox delete-stacks
```

## Setup Alexa Skill (continued)

3. Back on the `Configuration` tab, add the `Lambda ARN` outputted during the `Setup AWS` section.
    1. Type `bash alexa-jukebox info` if this information is no longer on the screen.

## Setup Pi Song Poller

* On the Raspberry Pi, copy the files from the `jukebox_song_poller` directory to the root home directory.
* In the terminal, type
```
pip install -r requirements -U
```
* Copy the `song_poller_service.sh` to the `/etc/init.d/` directory.
```
cp song_poller_service.sh /etc/init.d/
```
* Change the permissions to the file.
```
chmod u+x /etc/init.d/song_poller_service.sh
```
* Add the file to local service start defaults
```
update-rc.local /etc/init.d/song_poller_service.sh defaults
```
* Restart pi