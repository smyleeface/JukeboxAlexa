Update Songlist
===============

Updates the songs in the database.

```
docker run --rm -it -v $PWD:/app -v $HOME/.aws:/root/.aws amazonlinux:2017.12-with-sources python /app/update_songs.py --profile default /app/JukeboxSongs.csv
```
