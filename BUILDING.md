# Building
*Note: These instructions assume you know what you are doing with building projects and terminals and stuff.*

### Prerequisites
- A copy of Cuphead
- [.NET SDK](https://dotnet.microsoft.com/en-us/download) 8 or greater. You need the `dotnet` program in your path.
- [just](https://github.com/casey/just?tab=readme-ov-file#installation). You need the `just` program in your path.
- [c-wspp websocket-sharp](https://github.com/black-sliver/c-wspp-websocket-sharp). Note that it must be the windows version (even on Wine/Proton). If you are running the macOS version, you have to build this yourself.
- [c-wszig](https://github.com/black-sliver/c-wszig). This is a newer and better drop-in replacement for c-wspp.

### Notes

On UNIX systems (mac, Linux), forward slash `/` is used for paths, on Windows, backward slash `\` is used.

### Instructions
#### 1. Install BepInEx:
Extract BepInEx 5.x x64 for your OS (PC or mac) into the Cuphead installation folder. (Note the install instructions from README.md)

#### 2. Clone the project:
`git clone https://github.com/JKLeckr/CupheadArchipelagoMod.git -b main`
    
(Replace `main` with `dev` if you want more cutting edge but definately more broken changes included)

#### 3. Copy the required files:
Note the `ref` folder in the project directory (`CupheadArchipelago`) you just cloned.

1. Copy the following dll files from `Cuphead_Data/Managed` in the game directory to the `ref` folder mentioned before:
    - `Assembly-CSharp.dll`
    - `UnityEngine.dll`
    - `UnityEngine.UI.dll`

2. Copy the dll files from the extracted `c-wspp-websocket-sharp_windows-clang64` to the `ref` folder. Exclude `c-wspp.dll`.

3. Copy the `c-wspp-win64.dll` to the `ref` folder.

4. Delete the existing `c-wspp.dll` (if it exists) and rename `c-wspp-win64.dll` to `c-wspp.dll`.

#### 4. Prepare Project:
In the directory, on the terminal:

`just setup`

#### 5. Build the project:
In the directory, on the terminal:

`just build`

The built DLL's are in `CupheadArchipelago/bin/Debug/CupheadArchipelago`.

#### 6. Run Tests (Optional):
In the directory, on the terminal (you need .NET Framework/mono installed):

`just test`

Hopefully they pass!

#### 7. Copy the result:
In the game directory:
1. In the `BepInEx/plugins` folder, create a `CupheadArchipelago` folder.

2. Copy the following newly built DLL's to the newly created folder:
    - `Newtonsoft.Json.dll`
    - `Archipelago.MultiClient.Net.dll`
    - `CupheadArchipelago.dll`
    - `websocket-sharp.dll`
    - `c-wspp.dll` (or `c-wspp.dylib` if on macOS)

#### 8. Run Game:
Enjoy!
