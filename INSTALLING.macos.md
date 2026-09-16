# Installing on macOS

_This is for running on the native macOS version of Cuphead. If you are running on Wine/Proton, refer to the Windows instructions._
_There is an assumption you know how to use a terminal and get around a mac. If you don't you will not have much fun doing these steps._

### Prerequisites
- A legal copy of Cuphead
- [BepInEx](https://github.com/BepInEx/BepInEx/releases) 5.x (macos universal)

### Instructions
1. Download the CupheadArchipelago macos version of the mod from the [releases page](https://github.com/JKLeckr/CupheadArchipelagoMod/releases).

2. Place the extracted contents of BepInEx 5.x macos into the Cuphead installation folder (the folder with `Cuphead.app` in it). There should be a `run_bepinex.sh` and a `libdoorstop.dylib` along with a `BepInEx` folder and some other files in the same directory as `Cuphead.app`.

3. Launch a terminal in the same directory as `Cuphead.app`. There will be several commands you will be running in the next few steps.

4. Make `run_bepinex.sh` executable by running `chmod u+x run_bepinex.sh` in the same directory as it.

5. Allow `libdoorstop.dylib` for gatekeeper by running `xattr -dr com.apple.quarantine run_bepinex.sh libdoorstop.dylib` in the same directory as it.

6. Extract the CupheadArchipelago folder from the CupheadArchipelago mod zip and place it into the `BepInEx/plugins` folder. There should be a `CupheadArchipelago` folder `BepInEx/plugins` that contains several DLL's like `CupheadArchipelago.dll`.

7. Copy `nativews-macos-universal.dylib` into the game directory. Allow the file for gatekeeper by running `xattr -dr com.apple.quarantine run_bepinex.sh libdoorstop.dylib` in the same directory.

8. If **using Steam**, you must modify the launch options by going into properties for that game and setting it to `"/whatever/the/path/is/to/Cuphead/run_bepinex.sh" %command%`. Include the quotes. You can use "Get Info" on `run_bepinex.sh` to get the full path and paste it in. Make sure that path is inside the quotes, but not the command. If you do not do this step right, it will fail to launch.

9. Launch the game. If using Steam, launch it through Steam. Otherwise, launch `run_bepinex.sh` directly. The mod name and version should show in the main menu.

### Extra Notes
- Make sure you are installing the binary build of the mod and not the source code. Don't be one of those fellas!
- If `BepInEx/plugins` does not exist, you can launch the game with BepInEx once, or create the folder yourself.
- If you are on the Steam version, and the mod does not load, launch the game directly from Steam.
- On macOS, make sure you do step 7 and copy `nativews-macos-universal.dylib` into the game directory (the folder with `Cuphead.app` in it) and allow it with Gatekeeper. This is needed if you want to connect to anything!
