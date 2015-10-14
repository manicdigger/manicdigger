using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using System.Runtime.Serialization;

namespace ManicDigger
{
	public interface ModManager
	{
		/// <summary>
		/// Get the maximum number of blocks supported by the server (default: 1024)
		/// </summary>
		/// <returns>Maximum possible BlockTypes</returns>
		int GetMaxBlockTypes();
		
		/// <summary>
		/// Set a new BlockType
		/// </summary>
		/// <param name="id">ID of the new BlockType (has to be unique)</param>
		/// <param name="name">Name of the new block</param>
		/// <param name="block">BlockType to register</param>
		void SetBlockType(int id, string name, BlockType block);
		
		/// <summary>
		/// Set a new BlockType and automatically assign the next free ID
		/// </summary>
		/// <param name="name">Name of the new block</param>
		/// <param name="block">BlockType to register</param>
		void SetBlockType(string name, BlockType block);
		
		/// <summary>
		/// Get the ID of a certain BlockType
		/// </summary>
		/// <param name="name">Name of the BlockType</param>
		/// <returns>ID of the BlockType</returns>
		int GetBlockId(string name);
		
		/// <summary>
		/// Add the given block to inventory in creative mode
		/// </summary>
		/// <param name="blockType">Name of the BlockType</param>
		void AddToCreativeInventory(string blockType);

		/// <summary>
		/// Registers a method to be called every time a player places a block
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z)</param>
		void RegisterOnBlockBuild(ModDelegates.BlockBuild f);

		/// <summary>
		/// Registers a method to be called every time a player deletes a block
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z, int oldblock)</param>
		void RegisterOnBlockDelete(ModDelegates.BlockDelete f);

		/// <summary>
		/// Registers a method to be called every time a player uses a block
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z)</param>
		void RegisterOnBlockUse(ModDelegates.BlockUse f);

		/// <summary>
		/// Registers a method to be called every time a player uses a block while holding a tool in their hands
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z, int tool)</param>
		void RegisterOnBlockUseWithTool(ModDelegates.BlockUseWithTool f);

		int GetMapSizeX();
		int GetMapSizeY();
		int GetMapSizeZ();
		
		/// <summary>
		/// Get ID of a certain block
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <returns>ID of the block at the given position</returns>
		int GetBlock(int x, int y, int z);
		
		/// <summary>
		/// Get the name of a BlockType
		/// </summary>
		/// <param name="blockType">ID of the BlockType</param>
		/// <returns>Name of the BlockType</returns>
		string GetBlockName(int blockType);
		
		/// <summary>
		/// Get the name of a certain block
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <returns>Name of the block at the given position</returns>
		string GetBlockNameAt(int x, int y, int z);
		
		/// <summary>
		/// Set a block at the given position
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <param name="tileType">The block to place</param>
		void SetBlock(int x, int y, int z, int tileType);
		
		void SetSunLevels(int[] sunLevels);
		void SetLightLevels(float[] lightLevels);
		void AddCraftingRecipe(string output, int outputAmount, string Input0, int Input0Amount);
		void AddCraftingRecipe2(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount);
		void AddCraftingRecipe3(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount, string Input2, int Input2Amount);
		
		/// <summary>
		/// Sets the given string as translation
		/// </summary>
		/// <param name="language">Language code of the translated stirng</param>
		/// <param name="id">ID string for the translation (should be unique)</param>
		/// <param name="translation">The translation</param>
		void SetString(string language, string id, string translation);
		
		/// <summary>
		/// Get a certain translated string by ID
		/// </summary>
		/// <param name="id">ID string to look for</param>
		/// <returns>The translation if found. ID if no match could be found</returns>
		string GetString(string id);
		
		/// <summary>
		/// Checks if a given position is valid
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <returns>true if position is inside map bounds</returns>
		bool IsValidPos(int x, int y, int z);
		
		void RegisterTimer(ManicDigger.Action a, double interval);
		
		/// <summary>
		/// Plays a sound at the given position. Every player on the server will hear this sound. Only sounds that were present when the user joined will be played.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <param name="sound">Filename of the sound to play</param>
		void PlaySoundAt(int x, int y, int z, string sound);
		
		/// <summary>
		/// Plays a sound at the given position. Every player in the given range will hear the sound. Only sounds that were present when the user joined will be played.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <param name="sound">Filename of the sound to play</param>
		/// <param name="range">Range for the given sound to be heard</param>
		void PlaySoundAt(int x, int y, int z, string sound, int range);
		
		/// <summary>
		/// Find the nearest player to the given position
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		/// <returns>ID of the nearest player</returns>
		int NearestPlayer(int x, int y, int z);
		
		/// <summary>
		/// Give one block to the player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="block">ID of the block to give</param>
		void GrabBlock(int player, int block);
		
		/// <summary>
		/// Give a certain amount of blocks to the player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="block">ID of the block to give</param>
		/// <param name="amount">Amount to give</param>
		void GrabBlocks(int player, int block, int amount);
		
		/// <summary>
		/// Check if a player has the given privilege
		/// </summary>
		/// <param name="player"></param>
		/// <param name="p">The privilege to check</param>
		/// <returns>true if the player has the given privilege, false otherwise</returns>
		bool PlayerHasPrivilege(int player, string p);
		
		bool IsCreative();
		bool IsBlockFluid(int block);
		
		/// <summary>
		/// Mark the player's inventory as "dirty" so it is resent
		/// </summary>
		/// <param name="player"></param>
		void NotifyInventory(int player);
		
		/// <summary>
		/// Returns the server's color string for errors
		/// </summary>
		/// <returns>Color code for errors (by default "&4")</returns>
		string colorError();
		
		/// <summary>
		/// Sends a message to the given player. No formatting is done. Message is sent as given
		/// </summary>
		/// <param name="player"></param>
		/// <param name="p">Message to send</param>
		void SendMessage(int player, string p);
		
		/// <summary>
		/// Registers the given privilege with the server. This allows server console to have that privilege by default
		/// </summary>
		/// <param name="p">Privilege to register</param>
		void RegisterPrivilege(string p);
		
		void RegisterOnBlockUpdate(ModDelegates.BlockUpdate f);
		bool IsTransparentForLight(int p);
		void RegisterWorldGenerator(ModDelegates.WorldGenerator f);
		void RegisterOptionBool(string optionname, bool default_);
		int GetChunkSize();
		object GetOption(string optionname);
		
		/// <summary>
		/// Get the seed used to generate the current world
		/// </summary>
		/// <returns>The map seed</returns>
		int GetSeed();
		
		int Index3d(int x, int y, int h, int sizex, int sizey);
		void RegisterPopulateChunk(ModDelegates.PopulateChunk f);
		
		/// <summary>
		/// Sets the given SoundSet as default SoundSet for all blocks
		/// </summary>
		/// <param name="defaultSounds">SoundSet to use</param>
		void SetDefaultSounds(SoundSet defaultSounds);
		
		/// <summary>
		/// Gets a previously saved object from GlobalData
		/// </summary>
		/// <param name="name">The key to search for</param>
		/// <returns>The value at the given position</returns>
		byte[] GetGlobalData(string name);
		
		/// <summary>
		/// Store the given value to GlobalData. Data is persistent (will be stored in the savegame). Use carefully as big objects can cause problems
		/// </summary>
		/// <param name="name">Key value</param>
		/// <param name="value">Data to save</param>
		void SetGlobalData(string name, byte[] value);
		
		void RegisterOnLoad(ManicDigger.Action f);
		void RegisterOnSave(ManicDigger.Action f);
		void RegisterOnCommand(ModDelegates.Command f);
		
		/// <summary>
		/// Get the IP for the given player ID
		/// </summary>
		/// <param name="player"></param>
		/// <returns>IP of the given player</returns>
		string GetPlayerIp(int player);
		
		/// <summary>
		/// Get the player name for the given player ID
		/// </summary>
		/// <param name="player"></param>
		/// <returns>Name of the given player</returns>
		string GetPlayerName(int player);
		
		/// <summary>
		/// Set a special mod as requirement for the current mod. Use in PreStart() only.
		/// </summary>
		/// <param name="modname">Required mod</param>
		void RequireMod(string modname);
		
		/// <summary>
		/// Store the given value to GlobalDataNotSaved. Data is not persistent (will not be saved)
		/// </summary>
		/// <param name="name">Key value</param>
		/// <param name="value">Data to save</param>
		void SetGlobalDataNotSaved(string name, object value);
		
		/// <summary>
		/// Gets a previously saved object from GlobalDataNotSaved
		/// </summary>
		/// <param name="name">The key to search for</param>
		/// <returns>The value at the given position</returns>
		object GetGlobalDataNotSaved(string name);
		
		/// <summary>
		/// Send the given message to all players currently playing on the server. No formatting is done. Message is sent as given.
		/// </summary>
		/// <param name="message">The message to send</param>
		void SendMessageToAll(string message);
		
		/// <summary>
		/// Registers the given message to be displayed in /help
		/// </summary>
		/// <param name="command">Command for which the help string is intended</param>
		/// <param name="help">Short desciption of what the command does</param>
		void RegisterCommandHelp(string command, string help);
		
		/// <summary>
		/// Adds the given BlockType to the start inventory (blocks that each player on a survival server starts with)
		/// </summary>
		/// <param name="blocktype">Name of the blocktype</param>
		/// <param name="amount">Amount of blocks players get</param>
		void AddToStartInventory(string blocktype, int amount);
		
		long GetCurrentTick();
		
		/// <summary>
		/// Gets the number of real hours that one ingame day takes
		/// </summary>
		/// <returns>Duration of an ingame day</returns>
		double GetGameDayRealHours();
		
		/// <summary>
		/// Sets the number of real hours that one ingame day takes
		/// </summary>
		/// <param name="hours">Duration of an ingame day</param>
		void SetGameDayRealHours(double hours);

		void SetDaysPerYear(int days);
		int GetDaysPerYear();

		int GetHour();
		double GetTotalHours();
		int GetDay();
		double GetTotalDays();
		int GetYear();
		int GetSeason();

		/// <summary>
		/// Send current BlockType definitions to all players. Used on season change
		/// </summary>
		void UpdateBlockTypes();
		
		void EnableShadows(bool value);
		float GetPlayerPositionX(int player);
		float GetPlayerPositionY(int player);
		float GetPlayerPositionZ(int player);
		
		/// <summary>
		/// Sets the player's position on the server. Teleports a player to that position.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		/// <param name="z">z coordinate</param>
		void SetPlayerPosition(int player, float x, float y, float z);
		
		int GetPlayerHeading(int player);
		int GetPlayerPitch(int player);
		int GetPlayerStance(int player);
		
		/// <summary>
		/// Sets the player's orientation
		/// </summary>
		/// <param name="player"></param>
		/// <param name="heading">The body heading. Value between 0 and 256</param>
		/// <param name="pitch">Head rotation. Value between 0 and 256</param>
		/// <param name="stance">Used for animation. Represents leaning left/right</param>
		void SetPlayerOrientation(int player, int heading, int pitch, int stance);
		
		/// <summary>
		/// Gets a list of all online players
		/// </summary>
		/// <returns>Array containing the IDs of online players</returns>
		int[] AllPlayers();
		
		void SetPlayerAreaSize(int size);
		bool IsSinglePlayer();
		void AddPermissionArea(int x1, int y1, int z1, int x2, int y2, int z2, int permissionLevel);
		void RemovePermissionArea(int x1, int y1, int z1, int x2, int y2, int z2);
		int GetPlayerPermissionLevel(int player);
		void SetCreative(bool creative);
		void SetWorldSize(int x, int y, int z);
		void RegisterOnPlayerJoin(ModDelegates.PlayerJoin a);
		void RegisterOnPlayerLeave(ModDelegates.PlayerLeave a);
		void RegisterOnPlayerDisconnect(ModDelegates.PlayerDisconnect a);
		void RegisterOnPlayerChat(ModDelegates.PlayerChat a);
		void RegisterOnPlayerDeath(ModDelegates.PlayerDeath a);
		
		/// <summary>
		/// Returns the dimensions of the game window.
		/// </summary>
		/// <param name="player"></param>
		/// <returns>Array containing window size</returns>
		int[] GetScreenResolution(int player);
		
		void SendDialog(int player, string id, Dialog dialog);
		void RegisterOnDialogClick(ModDelegates.DialogClick a);
		
		/// <summary>
		/// Changes the model and/or skin of the given player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="model">Name of the model file (e.g. player.txt)</param>
		/// <param name="texture">Name of a texture file (should be present in data/public). If this is empty, default player skin will be used</param>
		void SetPlayerModel(int player, string model, string texture);
		
		void RenderHint(RenderHint hint);
		
		/// <summary>
		/// Changes freemove state of given player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="enable">Enable (true) or disable (false) freemove and noclip for given player</param>
		void EnableFreemove(int player, bool enable);
		
		int GetPlayerHealth(int player);
		int GetPlayerMaxHealth(int player);
		void SetPlayerHealth(int player, int health, int maxhealth);
		int GetPlayerOxygen(int player);
		int GetPlayerMaxOxygen(int player);
		void SetPlayerOxygen(int player, int oxygen, int maxoxygen);
		
		/// <summary>
		/// Registers the given method to be called each time a player is hit using a weapon
		/// </summary>
		/// <param name="a">Method to execute. Must have certain format: void Name(int sourcePlayer, int targetPlayer, int block, bool headshot);</param>
		void RegisterOnWeaponHit(ModDelegates.WeaponHit a);
		
		/// <summary>
		/// Registers the given method to be called every time a player presses a "SpecialKey"
		/// </summary>
		/// <param name="a">Method to execute. Must have certain format: void Name(int player, SpecialKey key);</param>
		void RegisterOnSpecialKey(ModDelegates.SpecialKey1 a);
		
		/// <summary>
		/// Returns the default spawn position of a certain player.
		/// This method will return the custom spawnpoint if one has been permanently set.
		/// If no custom spawnpoint is present this method will return the global default spawnpoint.
		/// </summary>
		/// <param name="player">Player ID</param>
		/// <returns>Spawnpoint valid for the given player</returns>
		float[] GetDefaultSpawnPosition(int player);
		
		/// <summary>
		/// Retrieves the default spawnpoint
		/// </summary>
		/// <returns>Default spawnpoint</returns>
		int[] GetDefaultSpawnPosition();
		
		/// <summary>
		/// Permanently sets the default spawnpoint
		/// </summary>
		/// <param name="x">X coordinate of new spawnpoint</param>
		/// <param name="y">Y coordinate of new spawnpoint</param>
		/// <param name="z">Z coordinate of new spawnpoint</param>
		void SetDefaultSpawnPosition(int x, int y, int z);
		
		string GetServerName();
		string GetServerMotd();
		float[] MeasureTextSize(string text, DialogFont font);
		string GetServerIp();
		string GetServerPort();
		float GetPlayerPing(int player);
		
		/// <summary>
		/// Adds a new bot player to the game.
		/// </summary>
		/// <param name="name">Name for the new player</param>
		/// <returns>The ID of the newly added bot</returns>
		int AddBot(string name);
		bool IsBot(int player);
		void SetPlayerHeight(int player, float eyeheight, float modelheight);
		
		/// <summary>
		/// Disables use of given privilege for all players
		/// </summary>
		/// <param name="privilege">Privilege to be disabled</param>
		void DisablePrivilege(string privilege); //todo privileges
		
		/// <summary>
		/// Registers the given method to be called each time the player changes their selected material
		/// </summary>
		/// <param name="a">Method to execute. Must have certain format: void Name(int player)</param>
		void RegisterChangedActiveMaterialSlot(ModDelegates.ChangedActiveMaterialSlot a);
		
		/// <summary>
		/// Get the inventory data of the player
		/// </summary>
		/// <param name="player"></param>
		/// <returns>Inventory object</returns>
		Inventory GetInventory(int player);
		int GetActiveMaterialSlot(int player);
		
		/// <summary>
		/// This method is extremely buggy when (player != target)
		/// </summary>
		/// <param name="player"></param>
		/// <param name="target">ID of target player</param>
		/// <param name="tpp">Set camera mode to Third-Person-Camera (true/false)</param>
		void FollowPlayer(int player, int target, bool tpp);
		
		/// <summary>
		/// Set spectator status of the player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="isSpectator">Player invisible to non-spectators (true) or visible for all (false)</param>
		void SetPlayerSpectator(int player, bool isSpectator);
		bool IsPlayerSpectator(int player);
		
		/// <summary>
		/// Get the BlockType object of a certain block ID. This method causes an exception when the ID is not found
		/// </summary>
		/// <param name="block">The block ID to search for</param>
		/// <returns>BlockType object</returns>
		BlockType GetBlockType(int block);
		
		/// <summary>
		/// Updates ammunition for given player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="dictionary">Dictionary containing block ids and ammunition count</param>
		void NotifyAmmo(int player, Dictionary<int, int> dictionary);
		
		/// <summary>
		/// Registers the given method to be called everytime a shot is fired from a weapon
		/// </summary>
		/// <param name="a">Method to execute. Must have certain format: void Name(int sourceplayer, int block);</param>
		void RegisterOnWeaponShot(ModDelegates.WeaponShot a);
		
		/// <summary>
		/// Writes the given string into server chat log
		/// </summary>
		/// <param name="s">log message</param>
		void LogChat(string s);
		
		/// <summary>
		/// This allows all players to use the given privilege, no matter the normal configuration
		/// </summary>
		/// <param name="privilege">The privilege to grant</param>
		/// <param name="enable">Specifies if privilege shall be granted to all (true) or default behaviour should be used (false)</param>
		void EnableExtraPrivilegeToAll(string privilege, bool enable);
		
		/// <summary>
		/// Writes the given string into server event log
		/// </summary>
		/// <param name="serverEvent">log message</param>
		void LogServerEvent(string serverEvent);
		
		void RegisterOnLoadWorld(ModDelegates.LoadWorld a);
		void SetWorldDatabaseReadOnly(bool readOnly);
		string CurrentWorld();
		void LoadWorld(string filename);
		string[] GetModPaths();
		
		/// <summary>
		/// Sends an explosion to the player. This does not inflict damage. It just pushes the player.
		/// </summary>
		/// <param name="targetplayer">ID of target player</param>
		/// <param name="dx">X coordinate of explosion source</param>
		/// <param name="dy">Y coordinate of explosion source</param>
		/// <param name="dz">Z coordinate of explosion source</param>
		/// <param name="relativeposition">Specifies if the coordinates given are relative to the player</param>
		/// <param name="range">How far from center should the effect stop</param>
		/// <param name="time">How long the effect lasts</param>
		void SendExplosion(int targetplayer, float dx, float dy, float dz, bool relativeposition, float range, float time);
		
		/// <summary>
		/// Disconnects (kicks) a player from the server
		/// </summary>
		/// <param name="player"></param>
		void DisconnectPlayer(int player);
		
		/// <summary>
		/// Disconnects (kicks) a player from the server
		/// </summary>
		/// <param name="player"></param>
		/// <param name="message">Message displayed to the player</param>
		void DisconnectPlayer(int player, string message);
		
		/// <summary>
		/// Returns the color of the player group
		/// </summary>
		/// <param name="player"></param>
		/// <returns>A color string in format: &0</returns>
		string GetGroupColor(int player);
		
		/// <summary>
		/// Returns the name of the player group
		/// </summary>
		/// <param name="player"></param>
		/// <returns>A string containing the group name</returns>
		string GetGroupName(int player);
		
		/// <summary>
		/// Registers a new HTTP handler with the integrated HTTP server
		/// </summary>
		/// <param name="name">Internal name of the module. Displayed on module overview page.</param>
		/// <param name="description">Description of the module</param>
		/// <param name="module">The actual module</param>
		void InstallHttpModule(string name, Func<string> description, FragLabs.HTTP.IHttpModule module);
		
		int GetMaxPlayers();
		ServerClient GetServerClient();
		long TotalReceivedBytes();
		long TotalSentBytes();
		
		/// <summary>
		/// Changes the color of the player name.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="color">Color code given in format: &0</param>
		void SetPlayerNameColor(int player, string color);
		
		/// <summary>
		/// Returns the restart interval of the server.
		/// </summary>
		/// <returns>Value of AutoRestartCycle</returns>
		int GetAutoRestartInterval();

		/// <summary>
		/// Returns the number of seconds the server has been running
		/// </summary>
		/// <returns>Server uptime in seconds</returns>
		int GetServerUptimeSeconds();

		/// <summary>
		/// Sends a redirection request to the specified client. The target server has to be public!
		/// </summary>
		/// <param name="player"></param>
		/// <param name="ip">The IP of the target server</param>
		/// <param name="port">The Port of the target server</param>
		void SendPlayerRedirect(int player, string ip, int port);

		/// <summary>
		/// Determines if the server process has been asked to terminate.
		/// Use this when you need to save data in a method registered using RegisterOnSave() before server quits.
		/// </summary>
		/// <returns><i>true</i> if server is about to shutdown</returns>
		bool IsShuttingDown();

		/// <summary>
		/// Registers a method to be called every time a player places a block
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z)</param>
		void RegisterCheckOnBlockBuild(ModDelegates.CheckBlockBuild f);

		/// <summary>
		/// Registers a method to be called every time a player deletes a block
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z)</param>
		void RegisterCheckOnBlockDelete(ModDelegates.CheckBlockDelete f);

		/// <summary>
		/// Registers a method to be called every time a player uses a block
		/// </summary>
		/// <param name="f">Function to register. Required parameters: (int player, int x, int y, int z)</param>
		void RegisterCheckOnBlockUse(ModDelegates.CheckBlockUse f);

		#region Deprecated functions
		[Obsolete("GetCurrentYearTotal is deprecated, please use GetYear instead.", false)]
		double GetCurrentYearTotal();
		[Obsolete("GetCurrentHourTotal is deprecated, please use GetTotalHours instead.", false)]
		double GetCurrentHourTotal();
		[Obsolete("GetGameYearRealHours is deprecated.", false)]
		double GetGameYearRealHours();
		[Obsolete("SetGameYearRealHours is deprecated.", true)]
		void SetGameYearRealHours(double hours);
		#endregion
	}

	public enum SpecialKey
	{
		Respawn,
		SetSpawn,
		TabPlayerList,
		SelectTeam,
	}
	
	public enum DeathReason
	{
		FallDamage,
		BlockDamage,
		Drowning,
		Explosion,
	}

	public class ModDelegates
	{
		public delegate void BlockBuild(int player, int x, int y, int z);
		public delegate void BlockDelete(int player, int x, int y, int z, int oldblock);
		public delegate void BlockUse(int player, int x, int y, int z);
		public delegate void BlockUseWithTool(int player, int x, int y, int z, int tool);
		public delegate void BlockUpdate(int x, int y, int z);
		public delegate void WorldGenerator(int x, int y, int z, ushort[] chunk);
		public delegate void PopulateChunk(int x, int y, int z);
		public delegate bool Command(int player, string command, string argument);
		public delegate void PlayerJoin(int player);
		public delegate void PlayerLeave(int player);
		public delegate void PlayerDisconnect(int player);
		public delegate string PlayerChat(int player, string message, bool toteam);
		public delegate void PlayerDeath(int player, DeathReason reason, int sourceID);
		public delegate void DialogClick(int player, string widgetId);
		public delegate void WeaponHit(int sourcePlayer, int targetPlayer, int block, bool headshot);
		public delegate void WeaponShot(int sourceplayer, int block);
		public delegate void SpecialKey1(int player, SpecialKey key);
		public delegate void ChangedActiveMaterialSlot(int player);
		public delegate void LoadWorld();
		public delegate void UpdateEntity(int chunkx, int chunky, int chunkz, int id);
		public delegate void UseEntity(int player, int chunkx, int chunky, int chunkz, int id);
		public delegate void HitEntity(int player, int chunkx, int chunky, int chunkz, int id);
		public delegate bool CheckBlockUse(int player, int x, int y, int z);
		public delegate bool CheckBlockBuild(int player, int x, int y, int z);
		public delegate bool CheckBlockDelete(int player, int x, int y, int z);

		public delegate void DialogClick2(DialogClickArgs args);
		public delegate void Permission(PermissionArgs args);
	}

	public class DialogClickArgs
	{
		internal int player;
		public int GetPlayer() { return player; } public void SetPlayer(int value) { player = value; }
		internal string widgetId;
		public string GetWidgetId() { return widgetId; }public void SetWidgetId(string value) { widgetId = value; }
		internal string[] textBoxValue;
		public string[] GetTextBoxValue() { return textBoxValue; }public void SetTextBoxValue(string[] value) { textBoxValue = value; }
	}

	public class PermissionArgs
	{
		internal int player;
		internal int x;
		internal int y;
		internal int z;
		public int GetPlayer() { return player; } public void SetPlayer(int value) { player = value; }
		public int GetX() { return x; } public void SetX(int value) { x = value; }
		public int GetY() { return y; } public void SetY(int value) { y = value; }
		public int GetZ() { return z; } public void SetZ(int value) { z = value; }

		internal bool allowed;
		public bool GetAllowed() { return allowed; } public void SetAllowed(bool value) { allowed = value; }
	}
	
	public enum ItemClass
	{
		Block,
		Weapon,
		MainArmor,
		Boots,
		Helmet,
		Gauntlet,
		Shield,
		Other,
	}

	[ProtoContract]
	public class Item
	{
		[ProtoMember(1, IsRequired = false)]
		public ItemClass ItemClass;
		[ProtoMember(2, IsRequired = false)]
		public string ItemId;
		[ProtoMember(3, IsRequired = false)]
		public int BlockId;
		[ProtoMember(4, IsRequired = false)]
		public int BlockCount = 1;
	}

	[ProtoContract]
	public class Inventory
	{
		[OnDeserialized()]
		void OnDeserialized()
		{
			/*
            LeftHand = new Item[10];
            if (LeftHandProto != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (LeftHandProto.ContainsKey(i))
                    {
                        LeftHand[i] = LeftHandProto[i];
                    }
                }
            }
			 */
			RightHand = new Item[10];
			if (RightHandProto != null)
			{
				for (int i = 0; i < 10; i++)
				{
					if (RightHandProto.ContainsKey(i))
					{
						RightHand[i] = RightHandProto[i];
					}
				}
			}
		}
		[OnSerializing()]
		void OnSerializing()
		{
			Dictionary<int, Item> d;// = new Dictionary<int, Item>();
			/*
            for (int i = 0; i < 10; i++)
            {
                if (LeftHand[i] != null)
                {
                    d[i] = LeftHand[i];
                }
            }
            LeftHandProto = d;
			 */
			d = new Dictionary<int, Item>();
			for (int i = 0; i < 10; i++)
			{
				if (RightHand[i] != null)
				{
					d[i] = RightHand[i];
				}
			}
			RightHandProto = d;
		}
		//dictionary because protobuf-net can't serialize array of nulls.
		//[ProtoMember(1, IsRequired = false)]
		//public Dictionary<int, Item> LeftHandProto;
		[ProtoMember(2, IsRequired = false)]
		public Dictionary<int, Item> RightHandProto;
		//public Item[] LeftHand = new Item[10];
		public Item[] RightHand = new Item[10];
		[ProtoMember(3, IsRequired = false)]
		public Item MainArmor;
		[ProtoMember(4, IsRequired = false)]
		public Item Boots;
		[ProtoMember(5, IsRequired = false)]
		public Item Helmet;
		[ProtoMember(6, IsRequired = false)]
		public Item Gauntlet;
		[ProtoMember(7, IsRequired = false)]
		public Dictionary<ProtoPoint, Item> Items = new Dictionary<ProtoPoint, Item>();
		[ProtoMember(8, IsRequired = false)]
		public Item DragDropItem;
		public void CopyFrom(Inventory inventory)
		{
			//this.LeftHand = inventory.LeftHand;
			this.RightHand = inventory.RightHand;
			this.MainArmor = inventory.MainArmor;
			this.Boots = inventory.Boots;
			this.Helmet = inventory.Helmet;
			this.Gauntlet = inventory.Gauntlet;
			this.Items = inventory.Items;
			this.DragDropItem = inventory.DragDropItem;
		}
		public static Inventory Create()
		{
			Inventory i = new Inventory();
			//i.LeftHand = new Item[10];
			i.RightHand = new Item[10];
			return i;
		}
	}

	[ProtoContract]
	public class ProtoPoint
	{
		[ProtoMember(1, IsRequired = false)]
		public int X;
		[ProtoMember(2, IsRequired = false)]
		public int Y;
		public ProtoPoint()
		{
		}
		public ProtoPoint(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
		public override bool Equals(object obj)
		{
			ProtoPoint obj2 = obj as ProtoPoint;
			if (obj2 != null)
			{
				return this.X == obj2.X
					&& this.Y == obj2.Y;
			}
			return base.Equals(obj);
		}
		public override int GetHashCode()
		{
			return X ^ Y;
		}
	}

	public enum RenderHint
	{
		Fast,
		Nice,
	}

	[ProtoContract]
	public class Dialog
	{
		[ProtoMember(1,IsRequired=false)]
		public Widget[] Widgets;
		[ProtoMember(2, IsRequired = false)]
		public int Width;
		[ProtoMember(3, IsRequired = false)]
		public int Height;
		[ProtoMember(4, IsRequired = false)]
		public bool IsModal;
	}

	[ProtoContract]
	public class DialogFont
	{
		public DialogFont()
		{
		}
		public DialogFont(string FamilyName, float Size, DialogFontStyle FontStyle)
		{
			this.FamilyName = FamilyName;
			this.Size = Size;
			this.FontStyle = FontStyle;
		}
		[ProtoMember(1, IsRequired = false)]
		public string FamilyName = "Verdana";
		[ProtoMember(2, IsRequired = false)]
		public float Size = 11f;
		[ProtoMember(3, IsRequired = false)]
		public DialogFontStyle FontStyle;
	}
	[Flags]
	public enum DialogFontStyle
	{
		Regular = 0,
		Bold = 1,
		Italic = 2,
		Underline = 4,
		Strikeout = 8,
	}

	[ProtoContract]
	public class Widget
	{
		[ProtoMember(1, IsRequired = false)]
		public string Id;
		[ProtoMember(2, IsRequired = false)]
		public bool Click;
		[ProtoMember(3, IsRequired = false)]
		public int X;
		[ProtoMember(4, IsRequired = false)]
		public int Y;
		[ProtoMember(5, IsRequired = false)]
		public int Width;
		[ProtoMember(6, IsRequired = false)]
		public int Height;
		[ProtoMember(7, IsRequired = false)]
		public string Text;
		[ProtoMember(8, IsRequired = false)]
		public char ClickKey;
		[ProtoMember(9, IsRequired = false)]
		public string Image;
		[ProtoMember(10, IsRequired = false)]
		public int Color = -1; //white
		[ProtoMember(11, IsRequired = false)]
		public DialogFont Font;
		[ProtoMember(12, IsRequired = false)]
		public WidgetType Type;
		public const string SolidImage = "Solid";
		public static Widget MakeSolid(float x, float y, float width, float height, int color)
		{
			Widget w = new Widget();
			w.Type = WidgetType.Image;
			w.Image = SolidImage;
			w.X = (int)x;
			w.Y = (int)y;
			w.Width = (int)width;
			w.Height = (int)height;
			w.Color = color;
			return w;
		}

		public static Widget MakeText(string text, DialogFont Font, float x, float y, int textColor)
		{
			Widget w = new Widget();
			w.Type = WidgetType.Text;
			w.Text = text;
			w.X = (int)x;
			w.Y = (int)y;
			w.Font = Font;
			w.Color = textColor;
			return w;
		}

		public static Widget MakeTextBox(string text, DialogFont Font, float x, float y, float width, float height, int textColor)
		{
			Widget w = new Widget();
			w.Type = WidgetType.TextBox;
			w.Text = text;
			w.X = (int)x;
			w.Y = (int)y;
			w.Width = (int)width;
			w.Height = (int)height;
			w.Font = Font;
			w.Color = textColor;
			return w;
		}
	}

	public enum WidgetType
	{
		Image,
		Text,
		TextBox,
	}

	public class ModInfo
	{
		public string[] RequiredMods;
	}

	public interface IMod
	{
		/// <summary>
		/// Called once before the Mod is loaded. Use this to declare dependencies to other Mods.
		/// </summary>
		/// <param name="m">ModManager object</param>
		void PreStart(ModManager m);
		/// <summary>
		/// Called once when the Mod is started. Use this if you need to initialize fields, etc...
		/// </summary>
		/// <param name="m">ModManager object</param>
		void Start(ModManager m);
	}

	public enum DrawType
	{
		Empty,
		Solid,
		Transparent,
		Fluid,
		Torch,
		Plant,
		OpenDoorLeft,
		OpenDoorRight,
		ClosedDoor,
		Ladder,
		Fence,
		HalfHeight,
		Flat,
		Cactus,
	}

	public enum WalkableType
	{
		Empty,
		Fluid,
		Solid,
	}

	[ProtoContract]
	public class SoundSet
	{
		[ProtoMember(1)]
		public string[] Walk = new string[0];
		[ProtoMember(2)]
		public string[] Break = new string[0];
		[ProtoMember(3)]
		public string[] Build = new string[0];
		[ProtoMember(4)]
		public string[] Clone = new string[0];
		[ProtoMember(5)]
		public string[] Shoot = new string[0];
		[ProtoMember(6)]
		public string[] ShootEnd = new string[0];
		[ProtoMember(7)]
		public string[] Reload = new string[0];
	}

	[ProtoContract]
	public class BlockType
	{
		public BlockType() {}
		[ProtoMember(1)]
		public string TextureIdTop = "Unknown";
		[ProtoMember(2)]
		public string TextureIdBottom = "Unknown";
		[ProtoMember(3)]
		public string TextureIdFront = "Unknown";
		[ProtoMember(4)]
		public string TextureIdBack = "Unknown";
		[ProtoMember(5)]
		public string TextureIdLeft = "Unknown";
		[ProtoMember(6)]
		public string TextureIdRight = "Unknown";
		[ProtoMember(7)]
		public string TextureIdForInventory = "Unknown";
		[ProtoMember(8)]
		public DrawType DrawType;
		[ProtoMember(9)]
		public WalkableType WalkableType;
		[ProtoMember(10)]
		public int Rail;
		[ProtoMember(11)]
		public float WalkSpeed = 1;
		[ProtoMember(12)]
		public bool IsSlipperyWalk;
		[ProtoMember(13)]
		public SoundSet Sounds;
		[ProtoMember(14)]
		public int LightRadius;
		[ProtoMember(15)]
		public int StartInventoryAmount;
		[ProtoMember(16)]
		public int Strength;
		[ProtoMember(17)]
		public string Name;
		[ProtoMember(18)]
		public bool IsBuildable;
		[ProtoMember(19)]
		public bool IsUsable;
		[ProtoMember(20)]
		public bool IsTool;
		[ProtoMember(21)]
		public string handimage;
		[ProtoMember(22)]
		public bool IsPistol;
		[ProtoMember(23)]
		public int AimRadius;
		[ProtoMember(24)]
		public float Recoil;
		[ProtoMember(25)]
		public float Delay;
		[ProtoMember(26)]
		public float BulletsPerShot;
		[ProtoMember(27)]
		public float WalkSpeedWhenUsed = 1;
		[ProtoMember(28)]
		public bool IronSightsEnabled;
		[ProtoMember(29)]
		public float IronSightsMoveSpeed = 1;
		[ProtoMember(30)]
		public string IronSightsImage;
		[ProtoMember(31)]
		public float IronSightsAimRadius;
		[ProtoMember(32)]
		public float IronSightsFov;
		[ProtoMember(33)]
		public int AmmoMagazine;
		[ProtoMember(34)]
		public int AmmoTotal;
		[ProtoMember(35)]
		public float ReloadDelay;
		[ProtoMember(36)]
		public float ExplosionRange;
		[ProtoMember(37)]
		public float ExplosionTime;
		[ProtoMember(38)]
		public float ProjectileSpeed; // 0 is infinite
		[ProtoMember(39)]
		public bool ProjectileBounce;
		[ProtoMember(40)]
		public float DamageBody;
		[ProtoMember(41)]
		public float DamageHead;
		[ProtoMember(42)]
		public PistolType PistolType;
		[ProtoMember(43)]
		public int DamageToPlayer = 0;
		[ProtoMember(44)]
		public int WhenPlayerPlacesGetsConvertedTo;
		[ProtoMember(45)]
		public float PickDistanceWhenUsed;
		
		public string AllTextures
		{
			set
			{
				TextureIdTop = value;
				TextureIdBottom = value;
				TextureIdFront = value;
				TextureIdBack = value;
				TextureIdLeft = value;
				TextureIdRight = value;
				TextureIdForInventory = value;
			}
		}
		
		public string SideTextures
		{
			set
			{
				TextureIdFront = value;
				TextureIdBack = value;
				TextureIdLeft = value;
				TextureIdRight = value;
			}
		}
		
		public string TopBottomTextures
		{
			set
			{
				TextureIdTop = value;
				TextureIdBottom = value;
			}
		}

		public bool IsFluid()
		{
			return DrawType == DrawType.Fluid;
		}

		public bool IsEmptyForPhysics()
		{
			return (DrawType == DrawType.Ladder)
				|| (WalkableType != WalkableType.Solid && WalkableType != WalkableType.Fluid);
		}
	}

	public enum PistolType
	{
		Normal,
		Grenade,
	}

	public delegate void Action();
	public delegate void Action<T1, T2>(T1 t1, T2 t2);
	public delegate void Action<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
	public delegate void Action<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
	public delegate void Action<T1, T2, T3, T4, T5>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
	public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
	public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
	public delegate void Action<T>(T obj);

	public delegate TResult Func<TResult>();
	public delegate TResult Func<T1, T2, TResult>(T1 t1, T2 t2);
	public delegate TResult Func<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3);
	public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 t1, T2 t2, T3 t3, T4 t4);
	public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
}
