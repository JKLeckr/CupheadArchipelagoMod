# CupheadArchipelago

*Mod Created by JKLeckr*

A mod for Cuphead!

## Intro

This mod is designed to work with Archipelago, but it also adds extra features to the game as well. Parts can be enabled or disabled in the config.

This mod is currently in the alpha phase which means it is still missing things. So, do not expect a dreamy vacation on Inkwell Isle while immersing yourself in the mod. Caveat Emptor!

### Links

Cuphead Game: [Steam](http://store.steampowered.com/app/268910/Cuphead/), [GOG](https://www.gog.com/game/cuphead)

Archipelago: [GitHub](https://github.com/ArchipelagoMW/Archipelago)

APWorld: [Archipelago-cuphead](https://github.com/JKLeckr/Archipelago-cuphead)

Client Mod: [CupheadArchipelagoMod](https://github.com/JKLeckr/CupheadArchipelagoMod) (This project)

## Install
*Note: The install process is WIP, so it is not the most user-friendly.*

### Prerequisites
- A copy of Cuphead
- [BepInEx](https://github.com/BepInEx/BepInEx/releases) 5.x

### Instructions
1. Download the CupheadArchipelago mod from the [releases page](https://github.com/JKLeckr/CupheadArchipelagoMod/releases).

1. Extract BepInEx 5.x x64 for your OS into the Cuphead installation folder.

2. Place the contents of the extracted CupheadArchipelago folder into the `BepInEx/plugins` folder.

3. Launch game.

### Extra Notes
- Make sure you are installing the binary build of the mod and not the source code. Don't be one of those fellas!
- If `BepInEx/plugins` does not exist, you can launch the game with BepInEx once, or create the folder yourself.
- If you are on the Steam version, and the mod does not load, launch the game directly from Steam.
- If you are on Linux using Wine/Proton, use the Windows build of BepInEx.
- If you are using Steam on Linux or SteamOS, make sure to put `WINEDLLOVERRIDES="winhttp=n,b" %command%` in the launch arguments.
- There are no binary builds of CupheadArchipelago for macOS. You can build from source, but you are on your own. 

## Building
See [BUILDING.md](BUILDING.md)

## Setting up Archipelago

1. Launch Cuphead with CupheadArchipelago installed. It will create the config files.
2. Select an empty save slot. (Note the save slot must be empty to enable or disable Archipelago on it.)
3. Press the button combination shown in game to show the Archiepalago setup menu (if you are using a keyboard, it's C+Z by default).
4. Set it to enabled, and set all the required settings for connecting to Archipelago.
5. Once you are done, close the Archipelago setup menu and start the save slot. (Note it says "AP" in the corner of the save slot if Archipelago is enabled.)
6. Have fun, and watch out for bugs!

## Logs
If you want to see what is going on behind the scenes (useful for diagnosing problems), you should check the logs.
The logs are located in the `BepInEx` folder in the game directory. Logging can be configured in the config (See [Configuring](#configuring)).

## Configuring
The config files are in the game directory's `BepInEx/config` folder. The mod config file is called `com.JKLeckr.CupheadArchipelago.cfg`. It might be useful for debugging to add more verbose logging flags in the config. The game must be launched at least once for this to appear.

### Logging
Logging can be configured in the config.

The BepInEx console allows you to see what's going on in real time. By default, the BepInEx console is disabled.

These are notable config files and their settings for logging:

- `BepInEx.cfg`
    - Under `[Logging.Console]`, set `Enabled` to `true` to see the logging console window. Useful for seeing what's going on in real time. The log file might update regularly too, but it isn't as real time.
    - Under `[Logging]`, setting `UnityLogListening` to `true` helps with logging what Cuphead itself is logging.

- `com.JKLeckr.CupheadArchipelago.cfg`
    - Adding `Network` to `Logging` will show more verbose network action logging.
    - `Debug` is probably too verbose to be useful for most people currently. Logging is pretty verbose, even without `Debug` currently while the mod is in heavy development.
