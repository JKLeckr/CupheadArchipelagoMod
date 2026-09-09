# Installing

### Prerequisites
- A legal copy of Cuphead
- [BepInEx](https://github.com/BepInEx/BepInEx/releases) 5.x (x64)

### Instructions
1. Download the CupheadArchipelago mod from the [releases page](https://github.com/JKLeckr/CupheadArchipelagoMod/releases).

2. Place the extracted contents of BepInEx 5.x x64 for your OS into the Cuphead installation folder (the folder with `cuphead.exe` in it). There should be a `winhttp.dll` and a `doorstop_config` along with a `BepInEx` folder and some other files in the same directory as `cuphead.exe`. \[_[Screenshot](assets/doc/install_bepinex_example.png)_\]

3. Extract the CupheadArchipelago folder from the CupheadArchipelago mod zip and place it into the `BepInEx/plugins` folder. There should be a `CupheadArchipelago` folder `BepInEx/plugins` that contains several DLL's like `CupheadArchipelago.dll`. \[_[Screenshot](assets/doc/install_mod_example.png)_\]

4. Launch game. The mod name and version should show in the main menu. \[_[Screenshot](assets/doc/game_launched_example.jpg)_\]

### Extra Notes
- Make sure you are installing the binary build of the mod and not the source code. Don't be one of those fellas!
- If `BepInEx/plugins` does not exist, you can launch the game with BepInEx once, or create the folder yourself.
- If you are on the Steam version, and the mod does not load, launch the game directly from Steam.
- If you are on Linux using Wine/Proton, use the Windows build of BepInEx.
- If you are using Steam on Linux or SteamOS, make sure to put `WINEDLLOVERRIDES="winhttp=n,b" %command%` in the launch arguments.
- There are no binary builds of CupheadArchipelago for macOS. You can build from source, but you are on your own. There are known issues that prevent secured connections from working.
