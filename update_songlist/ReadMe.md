Update Songlist
===============

```
docker run --rm -it -v $PWD:/app python:3.6.4-slim python /app/update_songs.py --profile default /app/JukeboxSongs.csv
```
