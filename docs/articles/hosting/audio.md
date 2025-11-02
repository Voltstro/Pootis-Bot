# Audio

If you want to use the audio module, you will also need to setup [Lavalink](https://lavalink.dev/). Lavalink acts as the player of audio from an audio source (E.G YouTube). Pootis-Bot currently only supports the YouTube source in Lavalink.

## Download

You can download Lavalink from [their GitHub](https://github.com/lavalink-devs/Lavalink/releases). You will also need [Java installed](https://adoptium.net/en-GB). You can optionally run Lavalink [using Docker](https://lavalink.dev/getting-started/docker). 

## Configure Lavalink

You will need to enable and configure the [YouTube plugin](https://github.com/lavalink-devs/youtube-source#plugin). The final config should look something similar to below:

```yaml
 
server:
  port: 2333
  address: 0.0.0.0
  http2:
    enabled: false
plugins:
  youtube:
    oauth:
      enabled: true

lavalink:
  plugins:
    # Replace VERSION with the current version as shown by the Releases tab or a long commit hash for snapshots.
    - dependency: "dev.lavalink.youtube:youtube-plugin:1.15.0"
      snapshot: false # Set to true if you want to use a snapshot version.
  server:
    password: "youshallnotpass"
    sources:
      # The default Youtube source is now deprecated and won't receive further updates. Please use https://github.com/lavalink-devs/youtube-source#plugin instead.
      youtube: false
      bandcamp: false
      soundcloud: false
      twitch: false
      vimeo: false
      nico: false
      http: false # warning: keeping HTTP enabled without a proxy configured could expose your server's IP address.
      local: false
    filters: # All filters are enabled by default
      volume: true
      equalizer: true
      karaoke: true
      timescale: true
      tremolo: true
      vibrato: true
      distortion: true
      rotation: true
      channelMix: true
      lowPass: true
    bufferDurationMs: 400 # The duration of the NAS buffer. Higher values fare better against longer GC pauses. Duration <= 0 to disable JDA-NAS. Minimum of 40ms, lower values may introduce pauses.
    frameBufferDurationMs: 5000 # How many milliseconds of audio to keep buffered
    opusEncodingQuality: 10 # Opus encoder quality. Valid values range from 0 to 10, where 10 is best quality but is the most expensive on the CPU.
    resamplingQuality: LOW # Quality of resampling operations. Valid values are LOW, MEDIUM and HIGH, where HIGH uses the most CPU.
    trackStuckThresholdMs: 10000 # The threshold for how long a track can be stuck. A track is stuck if does not return any audio data.
    useSeekGhosting: true # Seek ghosting is the effect where whilst a seek is in progress, the audio buffer is read from until empty, or until seek is ready.
    youtubePlaylistLoadLimit: 6 # Number of pages at 100 each
    playerUpdateInterval: 5 # How frequently to send player updates to clients, in seconds
    youtubeSearchEnabled: true
    soundcloudSearchEnabled: true
    gc-warnings: true

metrics:
  prometheus:
    enabled: false
    endpoint: /metrics

logging:
  file:
    path: ./logs/

  level:
    root: INFO
    lavalink: INFO

  request:
    enabled: true
    includeClientInfo: true
    includeHeaders: false
    includeQueryString: true
    includePayload: true
    maxPayloadLength: 10000

  logback:
    rollingpolicy:
      max-file-size: 1GB
      max-history: 30

  moe.kyokobot.koe: TRACE
```

## Configure Pootis-Bot

You will need to enable the audio module and service by setting `Config.EnableAudioServices` to true. Pootis-Bot uses [Victoria](https://github.com/Yucked/Victoria) as its wrapper to Lavalink. You can configure it using `Config.VictoriaConfig`. The full [config object can be found here](https://github.com/Yucked/Victoria/blob/v7/src/Configuration.cs).
