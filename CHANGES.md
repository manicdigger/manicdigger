# 2014-08-05

* Some serverside stability improvements
* Being disconnected from a server no longer closes the game
* Better multiplayer serverlist
* Small fixes in text rendering
* Links posted in chat are now clickable
* Made TextRenderer actually respect given Font Type
* **Added asset caching** (*this means you'll join servers much faster after your first visit*)
* Client window size is now sent to server replacing the old static value
* Added descriptions to some Mod-API methods (WIP)
* **Added player redirection from one server to another**
* Added server queries
* Maximum distance that players are visible can now be set in server config
* Fixed hitboxes of spectators not disappearing
* New Mod-API function: IsPlayerSpectator
* Updated Doors Mod
* **Fixed an IndexOutOfRangeException that occured relatively often**
* Login fields are now switchable using the keyboard
* **Fixed memory leak on reconnect**
* Fixed generation of bedrock at the bottom of the map
* Fixed sound playback on client
* Improved FillStart/FillEnd - bandwidth usage is now much lower than before
* Added RememberPosition Mod that spawns players where they left the game
* **Added /shutdown command**
* Fixed display of chat messages longer than 3 lines
* Slight color changes in join/leave messages
* Fixed the compass (and its inventory image not showing up)
* Added possibility to change language in client
* **Fixed player orientation being buggy until they turn their head**
* **Fixed possible crash when connecting to a server via web browser**
* Added simple singleplayer server for mobile devices
* Added new model format for player models
* Fixed Font selection in options menu
* Don't send bot names to playerlist and don't count them as players
* [*War Mod*] Fixed players being able to walk around / shoot while dead
* [*War Mod*] Fixed bug that caused grenades to have unlimited ammunition
* [*War Mod*] Fixed bug that occured when players killed themselves with grenades
* [*War Mod*] Fixed dead players being able to hurt others


# 2014-04-17

* **Fixed crafting**
* **Fixed player hitboxes**
* Don't show menu when quitting a dialog
* Fixed crash when server restarts
* Fixed crash at top of map


# 2014-04-16

* Fixed player not being able to build in 2 blocks high tunnels sometimes
* Added "Press R to Reload" message in War Mod
* Added possibility to answer to the last received PM using /re
* Added Modding API function to change display color of player names (the ones above the players)
* Added extra texture for HalfStair sides. Workaround for image being stretched
* Added player drowning
* **Improved terrain renderer. It is now far more efficient on multi-core systems**
* Added some basic functions to control bots
* **Heavily improved character animation (patch by koalala)**
* Introduced new privilege "use"
* Fixed bug that allowed players to jump 2 blocks high
* Only allow walking over a blocks edge when shift key is pressed
* **Added basic server setup. Server asks for parameters at first start**
* Added "FastBuild" project configuration
* **Fixed HalfStair physics**
* Fixed rails not being revertable
* Fixed Leaves, Apples and Water disappearing from inventory on season change
* Fixed a crash that could appear on a Linux server (without X11)
* Fixed player skin not loading after a player rejoins the game
* More data displayed in .pos command
* Added function GetAutoRestartInterval() to Modding API
* Enhanced banning. Players now stored in separate file. Banning player and reason stored
* War mod: Fixed a bug that allowed spectators to get weapons
* **Fixed grenade kills not being counted in War Mod**
* New packet: ClientDeath
* Added new modding function RegisterOnPlayerDeath()
* (Re-)Added WhenPlayerPlacesGetsConvertedTo to block definition
* **Localization support!**
* Stop jumping on trampoline block when shift key is held down
* Increased inventory size from 3 to 6 pages
* AutoRestartInterval in ServerConfig is now actually used by server
* Fixed flickering of plant DrawType
* Play sound when player gets a PM
* Support for maps bigger than 9984x9984 (maximum is now 32768x32768)
* **Added timebans (automatically expire after a specified interval)**
* New main menu
* Optional AutoJump (jumping over 1 block high obstacles automatically)
* Various multithreading optimizations
* Improved joining (less data transferred)


# 2014-02-01

* Fixed Apple transparency
* Changed default model. Added support for hats. Made by cybrminer7
* Fixed door and TNT IDs which caused display issues
* Fixed server console crash on Windows
* Added HTTP server to Manic Digger (allows display of Mod-defined webpages)
* Added possibility of Javascript modding
* Added .reconnect command. Pressing F6 when disconnected from a server will reconnect you.
* Added Lava screen color when swimming in Lava
* Fixed restart interval for automatic restarts
* Server now sends current gamemode to serverlist (instead of always "Fortress")
* New functions in Mod API. ServerClient Class is now exposed (allowing group changes by mods)
* Fixed stair display in War Mod
* Fixes some gameplay issues in War Mod
* Improved installer (added custom images)
* Modified PlayerList.cs to show rank names and colors
* Fixed crash when displaying unknown block in inventory
* Disabled Fluid.cs for performance reasons
* Added AutoCamera client mod. Try .cam to get a command list
* Fixed water display issue (still minor rendering bug)
* Fixed flying upwards in unloaded chunks
* Fixed HalfStair and Rail display when a solid block is above
* Made "Nice" font the default one
* Changed default resolution to 1280x720
* Fixed some crashes


# 2014-01-17

* Smooth shadows and Ambient Occlusion (patch by koalala)
* Simple grenades in War Mod
* More block types available
* Fixed drawing of doors
* New Mod-API functions
* Better message when getting disconnected/kicked from server
* Fixed display of IP addresses on Linux machines
* Fixed drawing of sun and moon (no longer visible through players)
