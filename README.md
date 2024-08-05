# CupheadArchipelago Mod

*Mod Created by JKLeckr*

A mod for Cuphead! 

Cuphead Game: ([Steam](http://store.steampowered.com/app/268910/Cuphead/), [GOG](https://www.gog.com/game/cuphead))

Archipelago World: [Archipelago Cuphead](https://github.com/JKLeckr/Archipelago-cuphead)

This mod is designed to work with Archipelago, but it also adds extra features to the game as well. Parts can be enabled or disabled in the config.

This mod is currently in the development phase and it is very very incomplete, so do not expect to make it out alive if you use it. Caveat Emptor!

## Install
*Note: The install process is WIP, so it is not the most user-friendly. Also, during this stage of development there are no binary builds. See "Building."*

### Prerequisites
- [BepInEx](https://github.com/BepInEx/BepInEx/releases) 5.x

### Instructions
1. Extract BepInEx 5.x x64 for your OS (PC or mac) into the Cuphead installation folder.

2. Place the contents of the extracted CupheadArchipelagoMod folder into the BepInEx/plugins folder.

3. Launch game.

### Extra Notes
- macOS isn't tested
- If you are on Linux using Wine/Proton, use the Windows build of BepInEx.

## Building
*Note: These instructions assume you know what you are doing with building projects and terminals and stuff.*

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/en-us/download) 8 or greater. You need the `dotnet` program in your PATH.

### Instructions
#### 1. Install BepInEx:
Extract BepInEx 5.x x64 for your OS (PC or mac) into the Cuphead installation folder. (Note from the install instructions above)

#### 2. Clone the project:
`git clone https://github.com/JKLeckr/CupheadArchipelagoMod.git -b main`
    
(Replace `main` with `dev` if you want more cutting edge but definately more broken changes included)

#### 3. Build the project:
In the directory, on the terminal:

`dotnet build`

The built DLL's are in `bin/Debug/net35`.

#### 4. Copy the result:
In the game directory:
1. In the `BepInEx/plugins` folder, create a `CupheadArchipelago` folder.

2. Copy the following newly built DLL's to the newly created folder:
    - Newtonsoft.Json.dll
    - websocket-sharp.dll
    - Archipelago.MultiClient.Net.dll
    - CupheadArchipelago.dll

3. Launch Game.

## Configuring
The config file is in the game directory's `BepInEx/config` folder. The file is called `com.JKLeckr.CupheadArchipelago.cfg`. It might be useful for debugging to add more verbose logging flags in the config.

## Setting up Archipelago
*Note: This is the temporary method while the mod is WIP. For now, just deal with this until the legit way is added.*

1. Make sure the game isn't running.
2. Go to the Cuphead saves folder (on Windows it should be in `%AppData%`).
3. Open the apdata save file for the save slot you want to use.
4. In the beginning of the file:
    - Set `"enabled":"true"`
    - Set `"address":"URL"` where `URL` is the URL to connect to Archipelago excluding the port.
    - Set `"port":PORT` where `PORT` is the port of the Archipelago server. (Note: no quotes around `PORT`)
    - Set `"slot":"PLAYER"` where `PLAYER` is your player slot name.
    - Set `"password":"PASSWD"` where `PASSWD` is your player.
5. Save the file and launch the game.
6. Pick the save slot you set up Archipelago with. (Note it says "AP" in the corner of the save file if it's enabled.)
7. Have fun, and watch out for bugs!
