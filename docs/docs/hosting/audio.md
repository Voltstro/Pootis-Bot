# Music Services

Pootis-Bot features the ability to play songs through Discord in a voice chat.

## How does it work?

Pootis-Bot gets its songs from YouTube. It first searches its own folder with already downloaded songs. If it doesn't find a file that is similar to the search query, it will then download the video and converts it to a `.mp3` file.

## Setup

### Prerequisites

```
YouTube API setup done
```

### Enabling audio services
!!! warning
    Old versions of Pootis-Bot cannot download the required files, as my website's URL has changed since 1.0 to 1.1.

!!! bug
    Please note that older versions Pootis-Bot (pre 1.1) have a lot of issues when it comes to the audio services, as they were fairly unstable and typically failed after playing 2~3 songs.

    They also do not work on Linux or MacOS.

To enable the audio services, run the command in Pootis-Bot's console:

```
toggleaudio
```

This will:

1. Downloads needed files for audio services (if they are not their already)

2. Sets up the basic config for audio services (if not done already)

----

If you ever want to disable audio services, run the same command.