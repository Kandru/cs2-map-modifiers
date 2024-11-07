
# CounterstrikeSharp - Map Modifiers

[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-map-modifiers?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-map-modifier/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-map-modifier](https://img.shields.io/github/issues/Kandru/cs2-map-modifiers)](https://github.com/Kandru/cs2-map-modifier/issues)

The MapModifiers plugin is a powerful tool designed to enhance and customize your Counter-Strike 2 game server. It provides a range of functionalities to manage and modify various aspects of the map, making it ideal for creating dynamic and engaging gameplay experiences.

Key Features
1. Spawn Point Management
Add custom spawn points to your map to introduce new gameplay scenarios. Whether you need additional spawn locations for special events or unique game modes, MapModifiers has you covered.

2. Client Commands
Execute specific client commands when a player joins the game. This feature allows you to customize the player's experience right from the start, ensuring that necessary configurations and settings are applied immediately upon joining.

3. Server Commands
Run server commands automatically when the map launches. This ensures that the server is properly configured and ready for gameplay, with all necessary settings and adjustments applied as soon as the map is loaded.


## Authors

- [@derkalle4](https://www.github.com/derkalle4)
- [@jmgraeffe](https://www.github.com/jmgraeffe)


## Commands

There are some commands avaible to work this this plugin. You'll need the permission @mapmodifiers/spawnpoints to be able to execute them. Please refer to the CounterstrikeSharp documentation on how to grant them.

### !addspawn [ct/t] [name?]
Adds a new spawn at your current player position with an optional name. The name is purely for identification of this spawn point in the configuration file later on.

Can be either executed in the chat (with ! before addspawn) or inside the client command line.

### !delspawn
Deletes the nearest custom spawn point. Your player needs to be at least around 2m nearby. Deleted spawn points will be completely removed after map reload.

### !showspawns
Shows all spawn points and hides them if executed a second time. Good to know all original spawn points and add custom ones. Custom spawn points will have a slightly lighter color depending on the team the spawn point does belong to.

## Configuration

This Plugin does automatically create a readable JSON configuration file. This configuration file can be found in /addons/counterstrikesharp/configs/plugins/MapModifiers/MapModifiers.json.

```json
{
  "maps": {
    "de_dust2": {
      "server_cmds": [
        "mp_buy_anywhere 1"
      ],
      "client_cmds": [
          "play sounds/vo/announcer/cs2_classic/bombpl.vsnd"
      ],
      "remove_original_spawns": false,
      "statck_original_t_spawns": false,
      "statck_original_ct_spawns": false,
      "t_spawns": [
        {
          "name": "Test1",
          "origin": [
            -1723.3846,
            -815.19,
            115.46165
          ],
          "angle": [
            0,
            164.21198,
            0
          ]
        }
      ],
      "ct_spawns": [
        {
          "name": "Test2",
          "origin": [
            297.18787,
            2288.9773,
            -118.96875
          ],
          "angle": [
            0,
            -81.64387,
            0
          ]
        }
      ]
    },
  },
  "ConfigVersion": 1
}
```

### server_cmds
Executes server side commands on each map load.

### client_cmds
Executes client side commands on player join.

### remove_original_spawns
Removes the original map-specific spawn points when each side has at least one custom spawn point.

### statck_original_t_spawns / statck_original_ct_spawns (not implemented yet)
Stacks another spawn point on each original map-specific spawn point. This doubles the spawn points. Make sure there is enough space above each spawn point.

### t_spawns / ct_spawns
List of custom spawn points for each team. Includes an optional name, the origin (position) and the angle (looking direction of a player on spawn) of each spawn.

## Installation

Simply unzip the latest release and put the Folder MapModifiers into /addons/counterstrikesharp/configs/plugins/ and restart your server afterwards. Updating is easier: simply overwrite all plugin files and it will be reloaded automatically.

## Compile yourself

Clone the project

```bash
  git clone https://github.com/Kandru/cs2-map-modifiers.git
```

Go to the project directory

```bash
  cd cs2-map-modifiers
```

Install dependencies

```bash
  dotnet restore
```

Build debug files (to use on a development game server)

```bash
  dotnet build
```

Build release files (to use on a production game server)

```bash
  dotnet publish
```

## FAQ

#### Not all player commands are not executed

Counter-Strike 2 forbids some commands to be executed and there is no way around this.

## License

Released under [GPLv3](/LICENSE) by [@Kandru](https://github.com/Kandru).