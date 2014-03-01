public class Language
{
    public Language()
    {
        stringsMax = 1024;
        stringsCount = 0;
        strings = new TranslatedString[stringsMax];
    }

    internal GamePlatform platform;

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
    public string SpawnPositionSet() { return Get("SpawnPositionSet"); }
    public string SpawnPositionSetTo() { return Get("SpawnPositionSetTo"); }
    public string Triangles() { return Get("Triangles"); }
    public string UseServerTexturesOption() { return Get("UseServerTexturesOption"); }
    public string ViewDistanceOption() { return Get("ViewDistanceOption"); }

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
        Add("en", "CannotWriteChatLog", "Cannot write to chat log file {0}.");
        Add("en", "ChunkUpdates", "Chunk updates: {0}");
        Add("en", "Connecting", "Connecting...");
        Add("en", "ConnectingProgressKilobytes", "{0} KB");
        Add("en", "ConnectingProgressPercent", "{0}%");
        Add("en", "DefaultKeys", "Default keys");
        Add("en", "Exit", "Exit");
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
        Add("en", "ReturnToMainMenu", "Return to main menu");
        Add("en", "ReturnToOptionsMenu", "Return to options menu");
        Add("en", "ShadowsOption", "Shadows: {0}");
        Add("en", "SoundOption", "Sound: {0}");
        Add("en", "SpawnPositionSet", "Spawn position set.");
        Add("en", "SpawnPositionSetTo", "Spawn position set to: {0}");
        Add("en", "Triangles", "Triangles: {0}");
        Add("en", "UseServerTexturesOption", "Use server textures (restart): {0}");
        Add("en", "ViewDistanceOption", "View distance: {0}");
    }

    void Add(string language, string id, string translated)
    {
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
        if (platform != null)
        {
            currentLanguage = platform.GetLanguageIso6391();
        }
        for (int i = 0; i < stringsMax; i++)
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
        for (int i = 0; i < stringsMax; i++)
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
}

public class TranslatedString
{
    internal string id;
    internal string language;
    internal string translated;
}
