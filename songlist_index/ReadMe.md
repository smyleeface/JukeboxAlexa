Songlist Index
==============

Indexes individual words from song titles and aggregates the info about the songs that contain the word.

Container
=========

## Build

```bash
cd jukebox_alexa
docker build -t jukebox-python -f Dockerfile-python .
```

## Run Tests

```bash
docker run -it --rm --name songlist_index -v $HOME/.aws:/root/.aws -v $PWD/songlist_index:/project jukebox-python entrypoint tests
```

## Debugging

```bash
docker run -it --rm --name songlist_index -v $HOME/.aws:/root/.aws -v $PWD/songlist_index:/project jukebox-python
```

## Objects

### Event data

#### Insert

```json

{
  "Records" : [
    {
      "eventName": "INSERT",
      "dynamodb" : {
        "NewImage" : {
          "track_number" : "number",
          "song" : "world map",
          "artist" : "artist",
          "search_title" : "search_title.lower()",
          "search_artist" : "search_artist.lower()"
        }
      }
    }
  ]
}
```

#### Remove

```json
{
  "Records" : [
    {
      "eventName": "REMOVE",
      "dynamodb" : {
        "NewImage" : {
          "track_number" : "number",
          "song" : "world map",
          "artist" : "artist",
          "search_title" : "search_title.lower()",
          "search_artist" : "search_artist.lower()"
        }
      }
    }
  ]
}

```

### Song

```json
{
  "track_number" : "number",
  "song" : "world map",
  "artist" : "artist",
  "search_title" : "search_title.lower()",
  "search_artist" : "search_artist.lower()"
}
```

### Song Index

```json
{
  "world" : [
    {
      "number" : "number",
      "song" : "world map",
      "artist" : "artist",
      "search_title.lower()" : "search_title.lower()",
      "search_artist.lower()" : "search_artist.lower()"
    },
    {
      "number" : "number",
      "song" : "hello world",
      "artist" : "artist",
      "search_title.lower()" : "search_title.lower()",
      "search_artist.lower()" : "search_artist.lower()"
    },
    {
      "number" : "number",
      "song" : "top flight security of the world",
      "artist" : "artist",
      "search_title.lower()" : "search_title.lower()",
      "search_artist.lower()" : "search_artist.lower()"
    }
  ]
}
```
