# CS2 Kill Sounds

A small [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) plugin that plays a sound to every player when someone gets a special kill:

| Kill type | Default sound event |
| --- | --- |
| Knife (any knife or bayonet) | `Kill.mcdonald` |
| Fire (molotov / incendiary) | `Kill.burn` |
| HE grenade | `Kill.kobe` |

Each player's volume is stored with [Clientprefs](https://github.com/Cruze03/Clientprefs), so players can turn it down or mute it (volume `0`).

> I wrote this for my own server and keep it here as a backup. It works for me, but I don't support it.

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) 1.0.361 or newer
- [Clientprefs](https://github.com/Cruze03/Clientprefs) v1.0.7
- [MVP-Anthem](https://github.com/T3Marius/MVP-Anthem) (optional, lets players change the volume, see [Player volume](#player-volume))
- A sound event file (`.vsndevts`) with the sounds above, shipped to clients through a workshop addon

## Build

```sh
dotnet build -c Release
```

`ClientprefsApi.dll` is in the repo root so the project builds without extra steps.

## Install

1. Copy the build output to `addons/counterstrikesharp/plugins/CS2KillSounds/`.
2. Install Clientprefs on the server.
3. Make sure clients download the workshop addon that contains your sound event file.

## Config

The config is created on first load at `addons/counterstrikesharp/configs/plugins/CS2KillSounds/CS2KillSounds.json`:

```json
{
  "SoundEventFiles": "soundevents/killsounds.vsndevts",
  "DefaultVolume": 0.4,
  "KnifeSound": "Kill.mcdonald",
  "FireSound": "Kill.burn",
  "HeGrenadeSound": "Kill.kobe",
  "ConfigVersion": 1
}
```

| Key | Description |
| --- | --- |
| `SoundEventFiles` | Sound event file to precache |
| `DefaultVolume` | Volume used when a player has no saved value (`0.0`–`1.0`) |
| `KnifeSound` / `FireSound` / `HeGrenadeSound` | Sound event name to play for each kill type |

## Player volume

The volume is stored in a Clientprefs cookie named `Volume` (public). If it is empty or not a number, `DefaultVolume` is used. A value of `0` mutes the sounds for that player.

This plugin has no volume command. It shares the `Volume` cookie with [MVP-Anthem](https://github.com/T3Marius/MVP-Anthem) on purpose. Both plugins store the volume as a decimal from `0.0` to `1.0`. When a player changes the volume in the MVP-Anthem menu, the kill sounds use the same volume, so players set one value for both plugins.

If a player has never set a volume, MVP-Anthem writes its own `DefaultVolume` into the cookie. After that, this plugin uses the cookie value, so its `DefaultVolume` only applies while the cookie is empty. Set `DefaultVolume` to the same value in both configs.
