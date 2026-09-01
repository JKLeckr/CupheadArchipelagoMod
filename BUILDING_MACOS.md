# Building on macOS

*Note: These instructions assume you know what you are doing with building projects and terminals and stuff.*

This is the macOS companion to [BUILDING.md](BUILDING.md). The mod ships prebuilt binaries for Windows only, so on macOS you must build from source. These steps were verified on **Apple Silicon (M-2 2023)** running Cuphead's native Steam (macOS) build with **Archipelago 0.6.7**.

*Note: This is **NOT** a guide on how to setup Archipelago or a custom server. All proceeding steps relating to **server setup** and **APWorld creation** can be found in the **APWorld repository** and **Archipelago Instructions**.*

## Prerequisites

- A legal copy of Cuphead (Steam macOS version recommended)
- [.NET SDK](https://dotnet.microsoft.com/en-us/download) 10 or greater. You need the `dotnet` program in your path.
- [just](https://github.com/casey/just?tab=readme-ov-file#installation). You need the `just` program in your path.
- [Rust](https://rustup.rs) with both macOS targets (needed to build the native websocket library):
  ```
  rustup target add x86_64-apple-darwin aarch64-apple-darwin
  ```
- Xcode Command Line Tools (`xcode-select --install`)
- [Rosetta 2](https://support.apple.com/en-us/102527) (`softwareupdate --install-rosetta`). Cuphead's macOS binary and BepInEx are x64, so they run translated.

### Directory conventions used below

| Variable | Path |
|---|---|
| `BUILD_DIR` | your scratch build directory, e.g. `~/Projects` |
| `NATIVEWS_DIR` | `BUILD_DIR/native-websocket-sharp/` |
| `MOD_DIR` | `BUILD_DIR/CupheadArchipelagoMod/` |
| `MOD_REF_DIR` | `MOD_DIR/CupheadArchipelago/ref/` |
| `MOD_OUT_DIR` | `MOD_DIR/CupheadArchipelago/bin/Debug/CupheadArchipelago/` |
| `GAME_DIR` | Steam: `~/Library/Application Support/Steam/steamapps/common/Cuphead/` (the folder containing `Cuphead.app`) |
| `GAME_DATA_DIR` | `GAME_DIR/Cuphead.app/Contents/Resources/Data/Managed/` |

## 1. Install BepInEx

The macOS build of BepInEx is distributed as `BepInEx_macos_universal_5.4.23.x.zip`. Download it from the [BepInEx releases page](https://github.com/BepInEx/BepInEx/releases) and extract it into `GAME_DIR`, next to `Cuphead.app`.

Then:

1. Make the launcher executable:
   ```
   cd "$HOME/Library/Application Support/Steam/steamapps/common/Cuphead"
   chmod u+x run_bepinex.sh
   ```
2. Configure Steam to launch the game through BepInEx:
   - Steam → right-click **Cuphead** → **Properties → General → Set Launch Options…**
   - Enter the path to the script: `"/path/to/Cuphead/run_bepinex.sh" %command%`
3. Launch and close Cuphead once through Steam so BepInEx generates its config (`BepInEx/config/BepInEx.cfg` and `BepInEx/LogOutput.log`).
4. If Gatekeeper blocks the unsigned Doorstop files, clear their quarantine:
   ```
   cd "$HOME/Library/Application Support/Steam/steamapps/common/Cuphead"
   xattr -dr com.apple.quarantine run_bepinex.sh libdoorstop.dylib
   ```

## 2. Clone and build native-websocket-sharp

Working directory: `NATIVEWS_DIR`

```bash
cd ~/Projects
git clone --recurse-submodules https://github.com/JKLeckr/native-websocket-sharp.git
cd native-websocket-sharp
just setup
```

> `nativews/lib/tungstenite` is a **git submodule**. If you already cloned without `--recurse-submodules` (or the module is empty), run:
> ```
> git submodule update --init --recursive
> ```

Build the **net35** target (flag avoids a lipo race between parallel framework builds)

```bash
dotnet build WebSocketSharp/websocket-sharp.csproj -c Release -f net35 -m:1
```

Verify the universal dylib was produced:

```bash
lipo -info WebSocketSharp/bin/Release/net35/nativews-macos-universal.dylib
```
  Expected Output ^
  ```bash
  Architectures in the fat file: ... are: x86_64 arm64
  ```

The three files you need from `NATIVEWS_DIR/WebSocketSharp/bin/Release/net35/` for the mod's `ref/` folder are:
- `websocket-sharp.dll`
- `websocket-sharp.pdb`
- `nativews-macos-universal.dylib`

## 3. Clone and build the mod

Working directory: `MOD_DIR`

```bash
cd ~/Projects
git clone https://github.com/JKLeckr/CupheadArchipelagoMod.git
cd CupheadArchipelagoMod
# Check out the release you intend to use, e.g. git checkout alpha03g.2
```

### Copy required files into `MOD_REF_DIR`

Game DLLs — from `GAME_DATA_DIR`:

```bash
cd ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/ref
cp "$HOME/Library/Application Support/Steam/steamapps/common/Cuphead/Cuphead.app/Contents/Resources/Data/Managed/Assembly-CSharp.dll" .
cp "$HOME/Library/Application Support/Steam/steamapps/common/Cuphead/Cuphead.app/Contents/Resources/Data/Managed/UnityEngine.dll" .
cp "$HOME/Library/Application Support/Steam/steamapps/common/Cuphead/Cuphead.app/Contents/Resources/Data/Managed/UnityEngine.UI.dll" .
```

Native websocket files — from `NATIVEWS_DIR`'s net35 output:

```bash
cd ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/ref
cp ~/Projects/native-websocket-sharp/WebSocketSharp/bin/Release/net35/websocket-sharp.dll .
cp ~/Projects/native-websocket-sharp/WebSocketSharp/bin/Release/net35/websocket-sharp.pdb . 2>/dev/null || true
cp ~/Projects/native-websocket-sharp/WebSocketSharp/bin/Release/net35/nativews-macos-universal.dylib .
```

### Prepare and build

```bash
cd ~/Projects/CupheadArchipelagoMod
just setup
just build
```

Output lands in `MOD_OUT_DIR`.

## 4. Copy the result into the game

Working directory: `GAME_DIR`

```bash
cd "$HOME/Library/Application Support/Steam/steamapps/common/Cuphead"
mkdir -p BepInEx/plugins/CupheadArchipelago
cp ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/bin/Debug/CupheadArchipelago/CupheadArchipelago.dll           BepInEx/plugins/CupheadArchipelago/
cp ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/bin/Debug/CupheadArchipelago/Archipelago.MultiClient.Net.dll   BepInEx/plugins/CupheadArchipelago/
cp ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/bin/Debug/CupheadArchipelago/Newtonsoft.Json.dll               BepInEx/plugins/CupheadArchipelago/
cp ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/bin/Debug/CupheadArchipelago/websocket-sharp.dll               BepInEx/plugins/CupheadArchipelago/
cp ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/bin/Debug/CupheadArchipelago/nativews-macos-universal.dylib    BepInEx/plugins/CupheadArchipelago/
cp ~/Projects/CupheadArchipelagoMod/CupheadArchipelago/bin/Debug/CupheadArchipelago/FVerParser.dll                     BepInEx/plugins/CupheadArchipelago/
```

> **Note** :`FVerParser.dll` is required but is missing from the copy list in BUILDING.md.** Without it, the plugin throws `FileNotFoundException: FVerParser` during type initialization. That error is NOT written to `BepInEx/LogOutput.log`. It only appears in Unity's log at `~/Library/Logs/Unity/Player.log`. To see Unity errors in `LogOutput.log`, set `[Logging.Disk] WriteUnityLog = true` in `BepInEx/config/BepInEx.cfg`.

## 5. Run the game

Launch Cuphead **through Steam** (so the launch option runs BepInEx). The mod name + version appear in the **top-right corner of the save-select screen** (not the title menu).
