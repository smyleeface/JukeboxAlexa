Song Poller
===========

* On the Raspberry Pi, clone the repo in the root home directory.
```
git clone https://github.com/smyleeface/smylee_jukebox.git jukebox`
```

* Go into the `song_poller` directory to the root home directory.
```
cd /home/pi/jukebox/song_poller`
```

* In the terminal, type
```
pip install -r requirements.txt -U
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
update-rc.local song_poller_service.sh defaults
```
* Restart pi