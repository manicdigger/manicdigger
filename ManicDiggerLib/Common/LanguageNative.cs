using System;
using System.IO;

namespace ManicDigger.Common
{
	/// <summary>
	/// Description of LanguageNative.
	/// </summary>
	public class LanguageNative : Language
	{
		public LanguageNative()
		{
			stringsMax = 1024 * 32;
			stringsCount = 0;
			strings = new TranslatedString[stringsMax];
			loadedLanguagesCount = 0;
			loadedLanguagesMax = 64;
			loadedLanguages = new string[loadedLanguagesMax];
		}

		internal string[] loadedLanguages;
		internal int loadedLanguagesMax;
		internal int loadedLanguagesCount;

		public override void LoadTranslations()
		{
			string translationPath = Path.Combine("data", "localization");
			if (Directory.Exists(translationPath))
			{
				string[] fileList = Directory.GetFiles(translationPath);
				//Iterate over all files in the directory
				for (int i = 0; i < fileList.Length; i++)
				{
					string[] lineList = File.ReadAllLines(fileList[i]);
					//Iterate over each line in these files
					for (int j = 1; j < lineList.Length; j++)
					{
						if (string.IsNullOrEmpty(lineList[j]))
						{
							//Skip line if empty
							continue;
						}
						if (lineList[j].StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
						{
							// skip lines starting with '#' as comments
							continue;
						}
						string[] splitList = lineList[j].Split('=');
						if (splitList.Length >= 2)
						{
							Add(lineList[0], splitList[0], splitList[1]);
						}
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
			// English as default language
			string currentLanguage = "en";
			if (OverrideLanguage != null)
			{
				currentLanguage = OverrideLanguage;  //Use specific language if defined
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
			// English as default language
			string currentLanguage = "en";
			if (OverrideLanguage != null)
			{
				currentLanguage = OverrideLanguage;  //Use specific language if defined
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

}
