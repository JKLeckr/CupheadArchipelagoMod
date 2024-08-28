# CupheadArchipelago

*Mod Created by JKLeckr*

A mod for Cuphead! 

Cuphead Game: ([Steam](http://store.steampowered.com/app/268910/Cuphead/), [GOG](https://www.gog.com/game/cuphead))

Archipelago World: [Archipelago Cuphead](https://github.com/JKLeckr/Archipelago-cuphead)

This mod is designed to work with Archipelago, but it also adds extra features to the game as well. Parts can be enabled or disabled in the config.

This mod is currently in the development phase and it is very very incomplete, so do not expect to make it out alive if you use it. Caveat Emptor!

## Install
*Note: The install process is WIP, so it is not the most user-friendly. Also, during this stage of development there are no binary builds other than previews. See "Building."*

### Prerequisites
- A copy of Cuphead
- [BepInEx](https://github.com/BepInEx/BepInEx/releases) 5.x

### Instructions
1. Extract BepInEx 5.x x64 for your OS into the Cuphead installation folder.

2. Place the contents of the extracted CupheadArchipelagoMod folder into the BepInEx/plugins folder.

3. Launch game.

### Extra Notes
- There are no binary builds of CupheadArchipelago for macOS. You can build from source, but you are on your own. 
- If you are on Linux using Wine/Proton, use the Windows build of BepInEx.

## Building
*Note: These instructions assume you know what you are doing with building projects and terminals and stuff.*

### Prerequisites
- A copy of Cuphead
- [.NET SDK](https://dotnet.microsoft.com/en-us/download) 8 or greater. You need the `dotnet` program in your path.
- [c-wspp websocket-sharp](https://github.com/black-sliver/c-wspp-websocket-sharp). Note that it must be the windows version (even on Wine/Proton). If you are running the macOS version, you have to build this yourself.

### Instructions
#### 1. Install BepInEx:
Extract BepInEx 5.x x64 for your OS (PC or mac) into the Cuphead installation folder. (Note from the install instructions above)

#### 2. Clone the project:
`git clone https://github.com/JKLeckr/CupheadArchipelagoMod.git -b main`
    
(Replace `main` with `dev` if you want more cutting edge but definately more broken changes included)

#### 3. Copy the required files
Note the `ref` folder in the project directory you just clone.

1. Copy the following dll files from `Cuphead_Data/Managed` in the game directory to the `ref` folder mentioned before:
    - `Assembly-CSharp.dll`
    - `UnityEngine.dll`
    - `UnityEngine.UI.dll`

2. Copy the dll files from the extracted `c-wspp-websocket-sharp_windows-clang64` to the `ref` folder.

#### 3. Build the project:
In the directory, on the terminal:

`dotnet build`

The built DLL's are in `bin/Debug/net35`.

#### 4. Copy the result:
In the game directory:
1. In the `BepInEx/plugins` folder, create a `CupheadArchipelago` folder.

2. Copy the following newly built DLL's to the newly created folder:
    - `Newtonsoft.Json.dll`
    - `Archipelago.MultiClient.Net.dll`
    - `CupheadArchipelago.dll`
    - `websocket-sharp.dll`
    - `c-wspp.dll` (or `c-wspp.dylib` if on macOS)

3. Launch Game.

## Setting up Archipelago
*Note: This is the temporary method while the mod is WIP. For now, just deal with this until the legit way is added.*

1. Make sure the game isn't running.
2. Go to the Cuphead saves folder (on Windows it should be in `%AppData%`).
3. Open the apdata save file for the save slot you want to use.
4. In the beginning of the file:
    - Set `"enabled":"true"`
    - Set `"address":"URL"` where `URL` is the URL to connect to Archipelago excluding the port.
    - Set `"port":PORT` where `PORT` is the port of the Archipelago server. (Note: no quotes around `PORT`)
    - Set `"player":"PLAYER"` where `PLAYER` is your player slot name.
    - Set `"password":"PASSWD"` where `PASSWD` is the server password.
5. Save the file and launch the game.
6. Pick the save slot you set up Archipelago with. (Note it says "AP" in the corner of the save file if it's enabled.)
7. Have fun, and watch out for bugs!

## Configuring
The config files are in the game directory's `BepInEx/config` folder. The mod config file is called `com.JKLeckr.CupheadArchipelago.cfg`. It might be useful for debugging to add more verbose logging flags in the config.

### Logging
If you want to see what is going on behind the scenes (useful for diagnosing problems), you should check the logs.
The logs are located in the `BepInEx` folder in the game directory.
By default, the BepInEx console is disabled.

These are notable config files and their settings for logging:

- `BepInEx.cfg`
    - Under `[Logging.Console]`, set `Enabled` to `true` to see the logging console window. Useful for seeing what's going on in real time. The log file might update regularly too, but it isn't as real time.
    - Under `[Logging]`, setting `UnityLogListening` to `true` helps with logging what Cuphead itself is logging.

- `com.JKLeckr.CupheadArchipelago.cfg`
    - Adding `Network` to `Logging` will show more verbose network action logging.
    - `Transpiler` and `Debug` are too verbose to be useful for most people currently. Logging is pretty verbose, even without `Debug` currently while the mod is in heavy development.
