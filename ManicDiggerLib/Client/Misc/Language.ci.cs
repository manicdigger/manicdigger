public class Language
{
    public Language()
    {
        stringsMax = 1024 * 32;
        stringsCount = 0;
        strings = new TranslatedString[stringsMax];
        loadedLanguagesCount = 0;
        loadedLanguagesMax = 64;
        loadedLanguages = new string[loadedLanguagesMax];
    }

    internal GamePlatform platform;
    internal string OverrideLanguage;
    internal string[] loadedLanguages;
    internal int loadedLanguagesMax;
    internal int loadedLanguagesCount;

    public string CannotWriteChatLog() { return Get("CannotWriteChatLog"); }
    public string ChunkUpdates() { return Get("ChunkUpdates"); }
    public string Connecting() { return Get("Connecting"); }
    public string ConnectingProgressKilobytes() { return Get("ConnectingProgressKilobytes"); }
    public string ConnectingProgressPercent() { return Get("ConnectingProgressPercent"); }
    public string DefaultKeys() { return Get("DefaultKeys"); }
    public string Exit() { return Get("Exit"); }
    public string FogDistance() { return Get("FogDistance"); }
    public string FontOption() { return Get("FontOption"); }
    public string FrameRateLagSimulation() { return Get("FrameRateLagSimulation"); }
    public string FrameRateUnlimited() { return Get("FrameRateUnlimited"); }
    public string FrameRateVsync() { return Get("FrameRateVsync"); }
    public string FreemoveNotAllowed() { return Get("FreemoveNotAllowed"); }
    public string GameName() { return Get("GameName"); }
    public string Graphics() { return Get("Graphics"); }
    public string InvalidVersionConnectAnyway() { return Get("InvalidVersionConnectAnyway"); }
    public string KeyBlockInfo() { return Get("KeyBlockInfo"); }
    public string KeyChange() { return Get("KeyChange"); }
    public string KeyChat() { return Get("KeyChat"); }
    public string KeyCraft() { return Get("KeyCraft"); }
    public string KeyFreeMove() { return Get("KeyFreeMove"); }
    public string KeyFullscreen() { return Get("KeyFullscreen"); }
    public string KeyJump() { return Get("KeyJump"); }
    public string KeyMoveBack() { return Get("KeyMoveBack"); }
    public string KeyMoveFoward() { return Get("KeyMoveFoward"); }
    public string KeyMoveLeft() { return Get("KeyMoveLeft"); }
    public string KeyMoveRight() { return Get("KeyMoveRight"); }
    public string KeyMoveSpeed() { return Get("KeyMoveSpeed"); }
    public string KeyPlayersList() { return Get("KeyPlayersList"); }
    public string KeyReloadWeapon() { return Get("KeyReloadWeapon"); }
    public string KeyRespawn() { return Get("KeyRespawn"); }
    public string KeyReverseMinecart() { return Get("KeyReverseMinecart"); }
    public string Keys() { return Get("Keys"); }
    public string KeyScreenshot() { return Get("KeyScreenshot"); }
    public string KeySetSpawnPosition() { return Get("KeySetSpawnPosition"); }
    public string KeyShowMaterialSelector() { return Get("KeyShowMaterialSelector"); }
    public string KeyTeamChat() { return Get("KeyTeamChat"); }
    public string KeyTextEditor() { return Get("KeyTextEditor"); }
    public string KeyThirdPersonCamera() { return Get("KeyThirdPersonCamera"); }
    public string KeyToggleFogDistance() { return Get("KeyToggleFogDistance"); }
    public string KeyUse() { return Get("KeyUse"); }
    public string MoveFree() { return Get("MoveFree"); }
    public string MoveFreeNoclip() { return Get("MoveFreeNoclip"); }
    public string MoveNormal() { return Get("MoveNormal"); }
    public string MoveSpeed() { return Get("MoveSpeed"); }
    public string NoMaterialsForCrafting() { return Get("NoMaterialsForCrafting"); }
    public string Off() { return Get("Off"); }
    public string On() { return Get("On"); }
    public string Options() { return Get("Options"); }
    public string Other() { return Get("Other"); }
    public string PressToUse() { return Get("PressToUse"); }
    public string Respawn() { return Get("Respawn"); }
    public string ReturnToGame() { return Get("ReturnToGame"); }
    public string ReturnToMainMenu() { return Get("ReturnToMainMenu"); }
    public string ReturnToOptionsMenu() { return Get("ReturnToOptionsMenu"); }
    public string ShadowsOption() { return Get("ShadowsOption"); }
    public string SoundOption() { return Get("SoundOption"); }
    public string AutoJumpOption() { return Get("AutoJumpOption"); }
    public string ClientLanguageOption() { return Get("ClientLanguageOption"); }
    public string SpawnPositionSet() { return Get("SpawnPositionSet"); }
    public string SpawnPositionSetTo() { return Get("SpawnPositionSetTo"); }
    public string Triangles() { return Get("Triangles"); }
    public string UseServerTexturesOption() { return Get("UseServerTexturesOption"); }
    public string ViewDistanceOption() { return Get("ViewDistanceOption"); }
    public string OptionSmoothShadows() { return Get("OptionSmoothShadows"); }
    public string OptionFramerate() { return Get("OptionFramerate"); }
    public string OptionResolution() { return Get("OptionResolution"); }
    public string OptionFullscreen() { return Get("OptionFullscreen"); }
    
    public string ServerCannotWriteLog() { return Get("Server_CannotWriteLogFile"); }
    public string ServerLoadingSavegame() { return Get("Server_LoadingSavegame"); }
    public string ServerCreatingSavegame() { return Get("Server_CreatingSavegame"); }
    public string ServerLoadedSavegame() { return Get("Server_LoadedSavegame"); }
    public string ServerConfigNotFound() { return Get("Server_ConfigNotFound"); }
    public string ServerConfigCorruptBackup() { return Get("Server_ConfigCorruptBackup"); }
    public string ServerConfigCorruptNoBackup() { return Get("Server_ConfigCorruptNoBackup"); }
    public string ServerConfigLoaded() { return Get("Server_ConfigLoaded"); }
    public string ServerClientConfigNotFound() { return Get("Server_ClientConfigNotFound"); }
    public string ServerClientConfigGuestGroupNotFound() { return Get("Server_ClientConfigGuestGroupNotFound"); }
    public string ServerClientConfigRegisteredGroupNotFound() { return Get("Server_ClientConfigRegisteredGroupNotFound"); }
    public string ServerClientConfigLoaded() { return Get("Server_ClientConfigLoaded"); }
    public string ServerInvalidSpawnCoordinates() { return Get("Server_InvalidSpawnCoordinates"); }
    public string ServerProgressDownloadingData() { return Get("Server_ProgressDownloadingData"); }
    public string ServerProgressDownloadingMap() { return Get("Server_ProgressDownloadingMap"); }
    public string ServerProgressGenerating() { return Get("Server_ProgressGenerating"); }
    public string ServerNoChatPrivilege() { return Get("Server_NoChatPrivilege"); }
    public string ServerFillAreaInvalid() { return Get("Server_FillAreaInvalid"); }
    public string ServerFillAreaTooLarge() { return Get("Server_FillAreaTooLarge"); }
    public string ServerNoSpectatorBuild() { return Get("Server_NoSpectatorBuild"); }
    public string ServerNoBuildPrivilege() { return Get("Server_NoBuildPrivilege"); }
    public string ServerNoBuildPermissionHere() { return Get("Server_NoBuildPermissionHere"); }
    public string ServerNoSpectatorUse() { return Get("Server_NoSpectatorUse"); }
    public string ServerNoUsePrivilege() { return Get("Server_NoUsePrivilege"); }
    public string ServerPlayerJoin() { return Get("Server_PlayerJoin"); }
    public string ServerPlayerDisconnect() { return Get("Server_PlayerDisconnect"); }
    public string ServerUsernameBanned() { return Get("Server_UsernameBanned"); }
    public string ServerNoGuests() { return Get("Server_NoGuests"); }
    public string ServerUsernameInvalid() { return Get("Server_UsernameInvalid"); }
    public string ServerPasswordInvalid() { return Get("Server_PasswordInvalid"); }
    public string ServerClientException() { return Get("Server_ClientException"); }
    public string ServerIPBanned() { return Get("Server_IPBanned"); }
    public string ServerTooManyPlayers() { return Get("Server_TooManyPlayers"); }
    public string ServerHTTPServerError() { return Get("Server_HTTPServerError"); }
    public string ServerHTTPServerStarted() { return Get("Server_HTTPServerStarted"); }
    public string ServerHeartbeatSent() { return Get("Server_HeartbeatSent"); }
    public string ServerHeartbeatError() { return Get("Server_HeartbeatError"); }
    public string ServerBanlistLoaded() { return Get("Server_BanlistLoaded"); }
    public string ServerBanlistCorruptNoBackup() { return Get("Server_BanlistCorruptNoBackup"); }
    public string ServerBanlistCorrupt() { return Get("Server_BanlistCorrupt"); }
    public string ServerBanlistNotFound() { return Get("Server_BanlistNotFound"); }
    public string ServerSetupAccept() { return Get("Server_SetupAccept"); }
    public string ServerSetupEnableHTTP() { return Get("Server_SetupEnableHTTP"); }
    public string ServerSetupMaxClients() { return Get("Server_SetupMaxClients"); }
    public string ServerSetupMaxClientsInvalidValue() { return Get("Server_SetupMaxClientsInvalidValue"); }
    public string ServerSetupMaxClientsInvalidInput() { return Get("Server_SetupMaxClientsInvalidInput"); }
    public string ServerSetupPort() { return Get("Server_SetupPort"); }
    public string ServerSetupPortInvalidValue() { return Get("Server_SetupPortInvalidValue"); }
    public string ServerSetupPortInvalidInput() { return Get("Server_SetupPortInvalidInput"); }
    public string ServerSetupWelcomeMessage() { return Get("Server_SetupWelcomeMessage"); }
    public string ServerSetupMOTD() { return Get("Server_SetupMOTD"); }
    public string ServerSetupName() { return Get("Server_SetupName"); }
    public string ServerSetupPublic() { return Get("Server_SetupPublic"); }
    public string ServerSetupQuestion() { return Get("Server_SetupQuestion"); }
    public string ServerSetupFirstStart() { return Get("Server_SetupFirstStart"); }
    public string ServerGameSaved() { return Get("Server_GameSaved"); }
    public string ServerInvalidBackupName() { return Get("Server_InvalidBackupName"); }
    public string ServerMonitorConfigLoaded() { return Get("Server_MonitorConfigLoaded"); }
    public string ServerMonitorConfigNotFound() { return Get("Server_MonitorConfigNotFound"); }
    public string ServerMonitorChatMuted() { return Get("Server_MonitorChatMuted"); }
    public string ServerMonitorChatNotSent() { return Get("Server_MonitorChatNotSent"); }
    public string ServerMonitorBuildingDisabled() { return Get("Server_MonitorBuildingDisabled"); }

    public void LoadTranslations()
    {
        IntRef fileCount = IntRef.Create(0);
        string[] fileList = platform.DirectoryGetFiles(platform.PathCombine("data", "localization"), fileCount);
        //Iterate over all files in the directory
        for (int i = 0; i < fileCount.value; i++)
        {
            IntRef lineCount = IntRef.Create(0);
            string[] lineList = platform.FileReadAllLines(fileList[i], lineCount);
            //Iterate over each line in these files
            for (int j = 1; j < lineCount.value; j++)
            {
                if (platform.StringEmpty(lineList[j]))
                {
                    //Skip line if empty
                    continue;
                }
                IntRef splitCount = IntRef.Create(0);
                string[] splitList = platform.StringSplit(lineList[j], "=", splitCount);
                if (splitCount.value >= 2)
                {
                    Add(lineList[0], splitList[0], splitList[1]);
                }
            }
        }
        //Add english default strings if not defined.
        AddEnglish();
    }

    void AddEnglish()
    {
        Add("en", "MainMenu_AssetsLoadProgress", "Loading... {0}%");
        Add("en", "MainMenu_Singleplayer", "Singleplayer");
        Add("en", "MainMenu_Multiplayer", "Multiplayer");
        Add("en", "MainMenu_Quit", "&cQuit");
        Add("en", "MainMenu_ButtonBack", "Back");
        Add("en", "MainMenu_SingleplayerButtonCreate", "Create or open...");
        Add("en", "MainMenu_Login", "Login");
        Add("en", "MainMenu_LoginUsername", "Username");
        Add("en", "MainMenu_LoginPassword", "Password");
        Add("en", "MainMenu_LoginRemember", "Remember me");
        Add("en", "MainMenu_ChoiceYes", "Yes");
        Add("en", "MainMenu_ChoiceNo", "No");
        Add("en", "MainMenu_LoginInvalid", "&4Invalid username or password");
        Add("en", "MainMenu_LoginConnecting", "Connecting...");
        Add("en", "MainMenu_MultiplayerConnect", "Connect");
        Add("en", "MainMenu_MultiplayerConnectIP", "Connect to IP");
        Add("en", "MainMenu_MultiplayerRefresh", "Refresh");
        Add("en", "MainMenu_MultiplayerLoading", "Loading...");
        Add("en", "MainMenu_ConnectToIpConnect", "Connect");
        Add("en", "MainMenu_ConnectToIpIp", "IP");
        Add("en", "MainMenu_ConnectToIpPort", "Port");

        Add("en", "CannotWriteChatLog", "Cannot write to chat log file {0}.");
        Add("en", "ChunkUpdates", "Chunk updates: {0}");
        Add("en", "Connecting", "Connecting...");
        Add("en", "ConnectingProgressKilobytes", "{0} KB");
        Add("en", "ConnectingProgressPercent", "{0}%");
        Add("en", "DefaultKeys", "Default keys");
        Add("en", "Exit", "Return to main menu");
        Add("en", "FogDistance", "Fog distance: {0}");
        Add("en", "FontOption", "Font: {0}");
        Add("en", "FrameRateLagSimulation", "Frame rate: lag simulation.");
        Add("en", "FrameRateUnlimited", "Frame rate: unlimited.");
        Add("en", "FrameRateVsync", "Frame rate: vsync.");
        Add("en", "FreemoveNotAllowed", "Freemove is not allowed on this server.");
        Add("en", "GameName", "Manic Digger");
        Add("en", "Graphics", "Graphics");
        Add("en", "InvalidVersionConnectAnyway", "Invalid game version. Local: {0}, Server: {1}. Do you want to connect anyway?");
        Add("en", "KeyBlockInfo", "Block information");
        Add("en", "KeyChange", "{0}: {1}");
        Add("en", "KeyChat", "Chat");
        Add("en", "KeyCraft", "Craft");
        Add("en", "KeyFreeMove", "Free move");
        Add("en", "KeyFullscreen", "Fullscreen");
        Add("en", "KeyJump", "Jump");
        Add("en", "KeyMoveBack", "Move back");
        Add("en", "KeyMoveFoward", "Move foward");
        Add("en", "KeyMoveLeft", "Move left");
        Add("en", "KeyMoveRight", "Move right");
        Add("en", "KeyMoveSpeed", "{0}x move speed");
        Add("en", "KeyPlayersList", "Players list");
        Add("en", "KeyReloadWeapon", "Reload weapon");
        Add("en", "KeyRespawn", "Respawn");
        Add("en", "KeyReverseMinecart", "Reverse minecart");
        Add("en", "Keys", "Keys");
        Add("en", "KeyScreenshot", "Screenshot");
        Add("en", "KeySetSpawnPosition", "Set spawn position");
        Add("en", "KeyShowMaterialSelector", "Open inventory");
        Add("en", "KeyTeamChat", "Team Chat");
        Add("en", "KeyTextEditor", "Texteditor");
        Add("en", "KeyThirdPersonCamera", "Third-person camera");
        Add("en", "KeyToggleFogDistance", "Toggle fog distance");
        Add("en", "KeyUse", "Use");
        Add("en", "MoveFree", "Move: Free.");
        Add("en", "MoveFreeNoclip", "Move: Free, Noclip.");
        Add("en", "MoveNormal", "Move: Normal.");
        Add("en", "MoveSpeed", "Move Speed: {0}.");
        Add("en", "NoMaterialsForCrafting", "No materials for crafting.");
        Add("en", "Off", "OFF");
        Add("en", "On", "ON");
        Add("en", "Options", "Options");
        Add("en", "Other", "Other");
        Add("en", "PressToUse", "(press {0} to use)");
        Add("en", "Respawn", "Respawn");
        Add("en", "ReturnToGame", "Return to game");
        Add("en", "ReturnToMainMenu", "Back");
        Add("en", "ReturnToOptionsMenu", "Return to options menu");
        Add("en", "ShadowsOption", "Shadows: {0}");
        Add("en", "SoundOption", "Sound: {0}");
        Add("en", "AutoJumpOption", "Auto Jump: {0}");
        Add("en", "ClientLanguageOption", "Language: {0}");
        Add("en", "SpawnPositionSet", "Spawn position set.");
        Add("en", "SpawnPositionSetTo", "Spawn position set to: {0}");
        Add("en", "Triangles", "Triangles: {0}");
        Add("en", "UseServerTexturesOption", "Use server textures (restart): {0}");
        Add("en", "ViewDistanceOption", "View distance: {0}");
        Add("en", "OptionSmoothShadows", "Smooth shadows: {0}");
        Add("en", "OptionFramerate", "Framerate: {0}");
        Add("en", "OptionResolution", "Resolution: {0}");
        Add("en", "OptionFullscreen", "Fullscreen: {0}");
        Add("en", "OptionDarkenSides", "Darken block sides: {0}");

        Add("en", "Server_CannotWriteLogFile", "Cannot write to server log file {0}.");
        Add("en", "Server_LoadingSavegame", "Loading savegame...");
        Add("en", "Server_CreatingSavegame", "Creating new savegame file.");
        Add("en", "Server_LoadedSavegame", "Savegame loaded: ");
        Add("en", "Server_ConfigNotFound", "Server configuration file not found, creating new.");
        Add("en", "Server_ConfigCorruptBackup", "ServerConfig corrupt! Created new. Backup saved as ServerConfig.txt.old");
        Add("en", "Server_ConfigCorruptNoBackup", "ServerConfig corrupt! Created new. COULD NOT BACKUP OLD!");
        Add("en", "Server_ConfigLoaded", "Server configuration loaded.");
        Add("en", "Server_ClientConfigNotFound", "Server client configuration file not found, creating new.");
        Add("en", "Server_ClientConfigGuestGroupNotFound", "Default guest group not found!");
        Add("en", "Server_ClientConfigRegisteredGroupNotFound", "Default registered group not found!");
        Add("en", "Server_ClientConfigLoaded", "Server client configuration loaded.");
        Add("en", "Server_InvalidSpawnCoordinates", "Invalid default spawn coordinates!");
        Add("en", "Server_ProgressDownloadingData", "Downloading data...");
        Add("en", "Server_ProgressGenerating", "Generating world...");
        Add("en", "Server_ProgressDownloadingMap", "Downloading map...");
        Add("en", "Server_NoChatPrivilege", "{0}Insufficient privileges to chat.");
        Add("en", "Server_FillAreaInvalid", "Fillarea is invalid or contains blocks in an area you are not allowed to build in.");
        Add("en", "Server_FillAreaTooLarge", "Fill area is too large.");
        Add("en", "Server_NoSpectatorBuild", "Spectators are not allowed to build.");
        Add("en", "Server_NoBuildPrivilege", "Insufficient privileges to build.");
        Add("en", "Server_NoBuildPermissionHere", "You need permission to build in this section of the world.");
        Add("en", "Server_NoSpectatorUse", "Spectators are not allowed to use blocks.");
        Add("en", "Server_NoUsePrivilege", "Insufficient privileges to use blocks.");
        Add("en", "Server_PlayerJoin", "Player {0} joins.");
        Add("en", "Server_PlayerDisconnect", "Player {0} disconnected.");
        Add("en", "Server_UsernameBanned", "Your username has been banned from this server.{0}");
        Add("en", "Server_NoGuests", "Guests are not allowed on this server. Login or register an account.");
        Add("en", "Server_UsernameInvalid", "Invalid username (allowed characters: a-z,A-Z,0-9,-,_; max. length: 16).");
        Add("en", "Server_PasswordInvalid", "Invalid server password.");
        Add("en", "Server_ClientException", "Your client threw an exception at server.");
        Add("en", "Server_IPBanned", "Your IP has been banned from this server.{0}");
        Add("en", "Server_TooManyPlayers", "Too many players! Try to connect later.");
        Add("en", "Server_HTTPServerError", "Cannot start HTTP server on TCP port {0}.");
        Add("en", "Server_HTTPServerStarted", "HTTP server listening on TCP port {0}.");
        Add("en", "Server_HeartbeatSent", "Heartbeat sent.");
        Add("en", "Server_HeartbeatError", "Unable to send heartbeat.");
        Add("en", "Server_BanlistLoaded", "Server banlist loaded.");
        Add("en", "Server_BanlistCorruptNoBackup", "Banlist corrupt! Created new. COULD NOT BACKUP OLD!");
        Add("en", "Server_BanlistCorrupt", "Banlist corrupt! Created new. Backup saved as ServerBanlist.txt.old");
        Add("en", "Server_BanlistNotFound", "Server banlist not found, creating new.");
        Add("en", "Server_SetupAccept", "y");
        Add("en", "Server_SetupEnableHTTP", "Dou you want to enable the builtin HTTP server? (Y/N)");
        Add("en", "Server_SetupMaxClients", "Enter the maximum number of clients (Default: 16)");
        Add("en", "Server_SetupMaxClientsInvalidValue", "Number may not be negative. Using default (16)");
        Add("en", "Server_SetupMaxClientsInvalidInput", "Invalid input. Using default (16)");
        Add("en", "Server_SetupPort", "Enter the port the server shall run on (Default: 25565)");
        Add("en", "Server_SetupPortInvalidValue", "Out of port range. Using default (25565)");
        Add("en", "Server_SetupPortInvalidInput", "Invalid input. Using default (25565)");
        Add("en", "Server_SetupWelcomeMessage", "Enter the welcome message (displayed when joining your server)");
        Add("en", "Server_SetupMOTD", "Enter the MOTD (displayed on server list)");
        Add("en", "Server_SetupName", "Please enter the server's name");
        Add("en", "Server_SetupPublic", "Do you want the server to be public (visible on the server list)? (Y/N)");
        Add("en", "Server_SetupQuestion", "Would you like to set up some basic parameters? (Y/N)");
        Add("en", "Server_SetupFirstStart", "It seems this is the first time you started this server.");
        Add("en", "Server_GameSaved", "Game saved. ({0} seconds)");
        Add("en", "Server_InvalidBackupName", "Invalid backup filename: ");
        Add("en", "Server_MonitorConfigLoaded", "Server monitor configuration loaded.");
        Add("en", "Server_MonitorConfigNotFound", "Server monitor configuration file not found, creating new.");
        Add("en", "Server_MonitorChatMuted", "Spam protection: {0} has been muted for {1} seconds.");
        Add("en", "Server_MonitorChatNotSent", "Spam protection: Your message has not been sent.");
        Add("en", "Server_MonitorBuildingDisabled", "{0} exceeds set block limit.");
        Add("en", "Server_CommandInvalidArgs", "Invalid arguments. Type /help to see command's usage.");
        Add("en", "Server_CommandInvalidSpawnPosition", "Invalid spawn position.");
        Add("en", "Server_CommandNonexistantPlayer", "{0}Player {1} does not exist.");
        Add("en", "Server_CommandInvalidPosition", "Invalid position.");
        Add("en", "Server_CommandInsufficientPrivileges", "{0}Insufficient privileges to access this command.");
        Add("en", "Server_CommandBackupFailed", "{0}Backup could not be created. Check filename.");
        Add("en", "Server_CommandBackupCreated", "{0}Backup created.");
        Add("en", "Server_CommandException", "Command exception.");
        Add("en", "Server_CommandUnknown", "Unknown command /");
        Add("en", "Server_CommandPlayerNotFound", "{0}Player {1} not found.");
        Add("en", "Server_CommandPMNoAnswer", "{0}No PM to answer.");
        Add("en", "Server_CommandGroupNotFound", "{0}Group {1} not found.");
        Add("en", "Server_CommandTargetGroupSuperior", "{0}The target group is superior your group.");
        Add("en", "Server_CommandTargetUserSuperior", "{0}Target user is superior or equal.");
        Add("en", "Server_CommandSetGroupTo", "{0}{1} set group of {2} to {3}.");
        Add("en", "Server_CommandOpTargetOffline", "{0}Player {1} is offline. Use /chgrp_offline command.");
        Add("en", "Server_CommandOpTargetOnline", "{0}Player {1} is online. Use /chgrp command.");
        Add("en", "Server_CommandInvalidGroup", "{0}Invalid group.");
        Add("en", "Server_CommandSetOfflineGroupTo", "{0}{1} set group of {2} to {3} (offline).");
        Add("en", "Server_CommandRemoveSuccess", "{0}Client {1} removed from config.");
        Add("en", "Server_CommandRemoveNotFound", "{0}No entry of client {1} found.");
        Add("en", "Server_CommandLoginNoPW", "{0}Group {1} doesn't allow password access.");
        Add("en", "Server_CommandLoginSuccess", "{0}{1} logs in group {2}.");
        Add("en", "Server_CommandLoginInfo", "Type /help see your available privileges.");
        Add("en", "Server_CommandLoginInvalidPassword", "{0}Invalid password.");
        Add("en", "Server_CommandWelcomeChanged", "{0}{1} set new welcome message: {2}");
        Add("en", "Server_CommandKickBanReason", " Reason: ");
        Add("en", "Server_CommandKickMessage", "{0}{1} was kicked by {2}.{3}");
        Add("en", "Server_CommandKickNotification", "You were kicked by an administrator.{0}");
        Add("en", "Server_CommandNonexistantID", "{0}Player ID {1} does not exist.");
        Add("en", "Server_CommandBanMessage", "{0}{1} was permanently banned by {2}.{3}");
        Add("en", "Server_CommandBanNotification", "You were permanently banned by an administrator.{0}");
        Add("en", "Server_CommandIPBanMessage", "{0}{1} was permanently IP banned by {2}.{3}");
        Add("en", "Server_CommandIPBanNotification", "You were permanently IP banned by an administrator.{0}");
        Add("en", "Server_CommandTimeBanMessage", "{0}{1} was banned by {2} for {3} minutes.{4}");
        Add("en", "Server_CommandTimeBanNotification", "You were banned by an administrator for {0} minutes.{1}");
        Add("en", "Server_CommandTimeIPBanMessage", "{0}{1} was IP banned by {2} for {3} minutes.{4}");
        Add("en", "Server_CommandTimeIPBanNotification", "You were IP banned by an administrator for {0} minutes.{1}");
        Add("en", "Server_CommandTimeBanInvalidValue", "Duration must be greater than 0!");
        Add("en", "Server_CommandBanOfflineTargetOnline", "{0}Player {1} is online. Use /ban command.");
        Add("en", "Server_CommandBanOfflineMessage", "{0}{1} (offline) was banned by {2}.{3}");
        Add("en", "Server_CommandUnbanSuccess", "{0}Player {1} unbanned.");
        Add("en", "Server_CommandUnbanIPNotFound", "{0}IP {1} not found.");
        Add("en", "Server_CommandUnbanIPSuccess", "{0}IP {1} unbanned.");
        Add("en", "Server_CommandGiveAll", "{0}Given all blocks to {1}");
        Add("en", "Server_CommandGiveSuccess", "{0}Given {1} {2} to {3}.");
        Add("en", "Server_CommandResetInventorySuccess", "{0}{1}reset inventory of {2}.");
        Add("en", "Server_CommandResetInventoryOfflineSuccess", "{0}{1}reset inventory of {2} (offline).");
        Add("en", "Server_CommandMonstersToggle", "{0} turned monsters {1}.");
        Add("en", "Server_CommandAreaAddIdInUse", "{0}Area ID already in use.");
        Add("en", "Server_CommandAreaAddSuccess", "{0}New area added: {1}");
        Add("en", "Server_CommandAreaDeleteNonexistant", "{0}Area does not exist.");
        Add("en", "Server_CommandAreaDeleteSuccess", "{0}Area deleted.");
        Add("en", "Server_CommandAnnouncementMessage", "{0}Announcement: {1}");
        Add("en", "Server_CommandSetSpawnInvalidCoordinates", "{0}Invalid spawn coordinates.");
        Add("en", "Server_CommandSetSpawnDefaultSuccess", "{0}Default spawn position set to {1},{2},{3}.");
        Add("en", "Server_CommandSetSpawnGroupSuccess", "{0}Spawn position of group {1} set to {2},{3},{4}.");
        Add("en", "Server_CommandSetSpawnPlayerSuccess", "{0}Spawn position of player {1} set to {2},{3},{4}.");
        Add("en", "Server_CommandPrivilegeAddHasAlready", "{0}Player {1} already has privilege {2}.");
        Add("en", "Server_CommandPrivilegeAddSuccess", "{0}New privilege for {1}: {2}");
        Add("en", "Server_CommandPrivilegeRemoveNoPriv", "{0}Player {1} doesn't have privilege {2}.");
        Add("en", "Server_CommandPrivilegeRemoveSuccess", "{0} {1} lost privilege: {2}");
        Add("en", "Server_CommandRestartSuccess", "{0}{1} restarted server.");
        Add("en", "Server_CommandShutdownSuccess", "{0}{1} shut down the server.");
        Add("en", "Server_CommandRestartModsSuccess", "{0}{1} restarted mods.");
        Add("en", "Server_CommandTeleportInvalidCoordinates", "{0}Invalid coordinates.");
        Add("en", "Server_CommandTeleportSuccess", "{0}New Position ({1},{2},{3}).");
        Add("en", "Server_CommandTeleportTargetMessage", "{0}You have been teleported to ({1},{2},{3}) by {4}.");
        Add("en", "Server_CommandTeleportSourceMessage", "{0}You teleported {1} to ({2},{3},{4}).");
        Add("en", "Server_CommandFillLimitDefaultSuccess", "{0}Default fill area limit set to {1}.");
        Add("en", "Server_CommandFillLimitGroupSuccess", "{0}Fill area limit of group {1} set to {2}.");
        Add("en", "Server_CommandFillLimitPlayerSuccess", "{0}Fill area limit of player {1} set to {2}.");
        Add("en", "Server_CommandInvalidType", "Invalid type.");
    }

    void Add(string language, string id, string translated)
    {
        if (IsNewLanguage(language))
        {
            if (loadedLanguagesCount < loadedLanguagesMax)
            {
                loadedLanguages[loadedLanguagesCount] = language;
                loadedLanguagesCount++;
            }
        }
        if (stringsCount > stringsMax)
        {
            return;
        }
        if (ContainsTranslation(language, id))
        {
        	return;
        }
        TranslatedString s = new TranslatedString();
        s.language = language;
        s.id = id;
        s.translated = translated;
        strings[stringsCount++] = s;
    }
    
    public void Override(string language, string id, string translated)
    {
        if (IsNewLanguage(language))
        {
            if (loadedLanguagesCount < loadedLanguagesMax)
            {
                loadedLanguages[loadedLanguagesCount] = language;
                loadedLanguagesCount++;
            }
        }
        //Just add the new string if it doesn't exist
        if (!ContainsTranslation(language, id))
        {
            Add(language, id, translated);
        }
        //Otherwise overwrite the existing string
        else
        {
            int replaceIndex = -1;
            for (int i = 0; i < stringsCount; i++)
            {
                if (strings[i] == null)
                {
                    continue;
                }
                if (strings[i].language == language)
                {
                    if (strings[i].id == id)
                    {
                        replaceIndex = i;
                        break;
                    }
                }
            }
            if (replaceIndex != -1)
            {
                TranslatedString s = new TranslatedString();
                s.language = language;
                s.id = id;
                s.translated = translated;
                strings[replaceIndex] = s;
            }
        }
    }

    TranslatedString[] strings;
    int stringsMax;
    int stringsCount;

    bool ContainsTranslation(string language, string id)
    {
    	for (int i = 0; i < stringsCount; i++)
    	{
    		if (strings[i] == null)
    		{
    			continue;
    		}
    		if (strings[i].language == language)
    		{
    			if (strings[i].id == id)
    			{
    				return true;
    			}
    		}
    	}
    	return false;
    }

    public string Get(string id)
    {
        string currentLanguage = "en";
        if (OverrideLanguage != null)
        {
            currentLanguage = OverrideLanguage;  //Use specific language if defined
        }
        else if (platform != null)
        {
            currentLanguage = platform.GetLanguageIso6391();  //Else use system language if defined
        }
        for (int i = 0; i < stringsCount; i++)
        {
            if (strings[i] == null)
            {
                continue;
            }
            if (strings[i].id == id && strings[i].language == currentLanguage)
            {
                return strings[i].translated;
            }
        }
        // fallback to english
        for (int i = 0; i < stringsCount; i++)
        {
            if (strings[i] == null)
            {
                continue;
            }
            if (strings[i].id == id && strings[i].language == "en")
            {
                return strings[i].translated;
            }
        }
        // not found
        return id;
    }
    
    public string GetUsedLanguage()
    {
        string currentLanguage = "en";
        if (OverrideLanguage != null)
        {
            currentLanguage = OverrideLanguage;  //Use specific language if defined
        }
        else if (platform != null)
        {
            currentLanguage = platform.GetLanguageIso6391();  //Else use system language if defined
        }
        return currentLanguage;
    }
    
    public void NextLanguage()
    {
        if (OverrideLanguage == null)
        {
            OverrideLanguage = "en";
        }
        //Get index of currently selected language
        int languageIndex = -1;
        for (int i = 0; i < loadedLanguagesMax; i++)
        {
            //Skip empty elements
            if (loadedLanguages[i] == null)
            {
                continue;
            }
            if (loadedLanguages[i] == OverrideLanguage)
            {
                languageIndex = i;
            }
        }
        if (languageIndex < 0)
        {
            languageIndex = 0;
        }
        languageIndex++;
        if (languageIndex >= loadedLanguagesMax || languageIndex >= loadedLanguagesCount)
        {
            languageIndex = 0;
        }
        OverrideLanguage = loadedLanguages[languageIndex];
    }
    
    public bool IsNewLanguage(string language)
    {
        //Scan whole array of loaded languages if given already exists
        for (int i = 0; i < loadedLanguagesMax; i++)
        {
            //Skip empty elements
            if (loadedLanguages[i] == null)
            {
                continue;
            }
            if (loadedLanguages[i] == language)
            {
                return false;
            }
        }
        return true;
    }
    
    public TranslatedString[] AllStrings()
    {
        return strings;
    }
}

public class TranslatedString
{
    internal string id;
    internal string language;
    internal string translated;
}
