# DevelopmentStartup
Skip all the useless startup menus and jump right into a game! Tool for developers. <br>
Big thanks to [Kittenji](https://thunderstore.io/c/lethal-company/p/Kittenji/) for the contributions to this mod!

![Startup preview gif](https://raw.githubusercontent.com/CTN-Originals/LethalCompany-DevelopmentStartup/main/resources/DevelopmentStartup-preview.gif)

## Installation
### Manual
1. Download the latest version from the [releases page](https://github.com/CTN-Originals/LethalCompany-DevelopmentStartup/releases).
2. Extract the zip file.
3. Move the `BepInEx/plugins/DevelopmentStartup.dll` file to `BepInEx/plugins` folder.
4. Move the `BepInEx/config/DevelopmentStartup.cfg` file to `BepInEx/config` folder.
5. Launch the game and start wondering why it took you so long to find this tool!
### Thunderstore
Install using the [Thunderstore](https://thunderstore.io/c/lethal-company/p/ebkr/r2modman/) Mod Manager: https://thunderstore.io/c/lethal-company/p/CTNOriginals/DevelopmentStartup/

---

# Configuration
| Option | Description | Default |
| ------ | ----------- | ------- |
| **AutoJoinLAN** | Automatically join LAN lobbies when game is launched more than once. | `true` |
| **AutoPullLever** | Automatically pull the ship's lever on startup. | `false` |
| **TeleportToEntrance** | Automatically teleports you to the main entrance on level load (Requires 'AutoPullLever' enabled). | `false` |
| **TeleportInside** | Teleports you inside the facility instead (Requires 'TeleportToEntrance' enabled). | `false` |

## To-Do
- [x] Add support for joining local games on a second instance of the game. (Added by [Kittenji](https://thunderstore.io/c/lethal-company/p/Kittenji/))
- [x] Add the option to pull the ship lever once you join the lobby. (Added by [Kittenji](https://thunderstore.io/c/lethal-company/p/Kittenji/))
- [ ] Add a config option to choose the launch method (online or lan) and have it override FastStartup's setting/method.
- [ ] Add a config option to turn on debug mode to allow the user to see the logs in the console.

## Changelog
See [CHANGELOG.md](https://github.com/CTN-Originals/LethalCompany-DevelopmentStartup/blob/main/CHANGELOG.md) for the full changelog.