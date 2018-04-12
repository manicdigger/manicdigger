public class LanguageCi : Language
{
	public LanguageCi()
	{
		stringsMax = 1024 * 32;
		stringsCount = 0;
		strings = new TranslatedString[stringsMax];
		loadedLanguagesCount = 0;
		loadedLanguagesMax = 64;
		loadedLanguages = new string[loadedLanguagesMax];
	}

	internal GamePlatform platform;
	internal string[] loadedLanguages;
	internal int loadedLanguagesMax;
	internal int loadedLanguagesCount;

	public override void LoadTranslations()
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
				if (platform.StringStartsWithIgnoreCase(lineList[j], "#"))
				{
					// skip lines starting with '#' as comments
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

	public override void Add(string language, string id, string translated)
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

	public override void Override(string language, string id, string translated)
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

	public override string Get(string id)
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

	public override string GetUsedLanguage()
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

	public override void NextLanguage()
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

	bool IsNewLanguage(string language)
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

	public override TranslatedString[] AllStrings()
	{
		return strings;
	}
}
