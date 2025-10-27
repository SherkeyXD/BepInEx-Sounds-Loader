# MiSide Sounds Loader

This mods allows you to replace sounds in the game by matching the filenames.

## Installation & Usage

1. Download [BepInEx 6](https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip) and extract it to game folder. Then run the game once to setup BepInEx
2. Go to [Release](https://github.com/SherkeyXD/BepInEx-Sounds-Loader/releases/latest), download `SherkeyXD.BepInEx-Sounds-Loader.dll` and place it under `BepInEx/plugins`.
3. Create a new folder named `CustomSounds` under `BepInEx/plugins`, or run the game once to let the mod create it automatically.
4. Place the files under `CustomSounds` folder


## FAQ

### How do I get the name of a sound?

Use tools like [UnityExplorer](https://github.com/yukieiji/UnityExplorer) and [AssetRipper](https://github.com/AssetRipper/AssetRipper) to track the audio used in the game, and take its filename.

## Note

This repository does not claim ownership of the DLLs located in the `3rdParty` folder, and they are included in the repository solely for the CI builds.

## Thanks

This mod is based on [bepinex-soundmod](https://github.com/Ol1vver/bepinex-soundmod) and has been adapted to migrate from BepInEx 5 to BepInEx 6. Special thanks to the original author.