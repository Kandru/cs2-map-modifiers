
# CounterstrikeSharp - Map Modifiers

[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-map-modifiers?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-map-modifier/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-map-modifier](https://img.shields.io/github/issues/Kandru/cs2-map-modifiers)](https://github.com/Kandru/cs2-map-modifier/issues)
[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=C2AVYKGVP9TRG)

The MapModifiers plugin is a powerful tool designed to enhance and customize your Counter-Strike 2 game server. It provides a range of functionalities to manage and modify various aspects of the map, making it ideal for fixing half-broken maps where the mapper itself does not want to change certain things.

*Hint: you can update this plugin easily with the [Update Manager Plugin](https://github.com/Kandru/cs2-update-manager/)*

Key Features
1. Spawn Point Management
Add custom spawn points to your map to introduce new gameplay scenarios. Whether you need additional spawn locations for special events or unique game modes, MapModifiers has you covered.

2. Entity Management
Add or remove entities on round start. E.g. add or remove hostages, sound entities, ...

3. Client Commands
Execute specific client commands when a player joins the game. This feature allows you to customize the player's experience right from the start, ensuring that necessary configurations and settings are applied immediately upon joining.

4. Server Commands
Run server commands automatically when the map launches. This ensures that the server is properly configured and ready for gameplay, with all necessary settings and adjustments applied as soon as the map is loaded.

5. Spectator after Join
Moves a player to spectator after join if no team got picked. Will avoid spawning AFK players into teams automatically but still provide the team join overlay for active players.


## Authors

- [@derkalle4](https://www.github.com/derkalle4)
- [@jmgraeffe](https://www.github.com/jmgraeffe)


## Commands

There are some commands avaible to work this this plugin. You'll need the permission @mapmodifiers/spawnpoints to be able to execute them. Please refer to the CounterstrikeSharp documentation on how to grant them.

### !addspawn [ct/t/both] [name?]
Adds a new spawn at your current player position with an optional name. The name is purely for identification of this spawn point in the configuration file later on.

```
!addspawn ct
```

### !addentity [entity] [ct/t/spec/none] [permanent <true/false>?] [name?]
Adds a new entity at your current player position which is not permanent if not stated otherwise and with an optional name. The name is purely for identification of this spawn point in the configuration file later on.

```
!addentity hostage_entity ct true
```

### !delentity [delete <true/false>?]
Deletes the nearest custom entity. This does not delete other entities than custom entities in the config. Set the optional parameter "delete" to "true" and it will delete the entity instantly. Otherwise it will be deleted after reloading the map. Some entities like spawns to not like to be deleted instantly or the server will crash.

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
      "move_to_spectator_on_join": true,
      "entities": [
        {
          "type": 0,
          "name": "",
          "class_name": "info_player_terrorist",
          "team": 0,
          "origin": [
            -347.66858,
            1304.3175,
            162.79282
          ],
          "angle": [
            0,
            -98.82168,
            0
          ]
        },
        {
          "type": 0,
          "name": "",
          "class_name": "info_player_terrorist",
          "team": 0,
          "origin": [
            -339.65503,
            1234.3063,
            162.73746
          ],
          "angle": [
            0,
            -109.169426,
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

### move_to_spectator_on_join
Per default players that are connecting will join a team after 15 seconds even if they're AFK. This will provide the player with 14.9 seconds join time per default and move him to spectator if he did not choose a team yet. AFK players from the last map will not be moved into a team with this feature.

### entities
List of entities to create or remove on round start. Can be spawn points as well as hostages or other stuff.

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