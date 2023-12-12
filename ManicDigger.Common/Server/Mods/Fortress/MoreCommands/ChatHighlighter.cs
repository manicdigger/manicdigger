/*
 * ChatHighlighter Mod - Version 1.1
 * last change: 2015-02-10
 * Author: croxxx
 * 
 * This mod allows automatic coloring of predefined keywords in chat.
 * 
 * The commands are:
 * /highlight_add [word] [highlight color]
 * /highlight_remove [word]
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
	public class ChatHighlighter : IMod
	{
		//Enter the desired language code here. Currently supported are EN and DE.
		string languageCode = "EN";

		public void PreStart(ModManager m) { }

		public void Start(ModManager m)
		{
			this.m = m;

			m.RegisterPrivilege("highlight");
			m.RegisterCommandHelp("highlight", "Use /highlight_add or /highlight_remove");
			m.RegisterOnCommand(OnCommand);
			m.RegisterOnPlayerChat(OnChat);
			m.RegisterOnLoad(OnLoad);

			Console.WriteLine("[ChatHighlighter] Loaded Mod Version 1.1");
		}

		//Internal variables.
		//DO NOT CHANGE!
		ModManager m;
		string filePath = "UserData" + Path.DirectorySeparatorChar + "ChatHighlights.txt";
		string chatPrefix = "&8[&eChatHighlighter&8] ";
		List<Highlight> highlightedWords = new List<Highlight>();

		struct Highlight
		{
			public Highlight(string newWord, string newColor)
			{
				word = newWord;
				color = newColor;
			}
			public string word;
			public string color;
		}

		string GetLocalizedString(string value)
		{
			switch (languageCode)
			{
				#region German translation
				case "DE":
					switch (value)
					{
						case "error_no_permission":
							return "Du darfst die Hervorhebung von Wörtern nicht verwalten.";

						case "error_contains_semicolon":
							return "Das angegebene Wort darf kein Semikolon (;) enthalten!";

						case "error_non_hexadecimal":
							return "Die Farbe muss als Hexadezimaler Wert angegeben werden (0-9, a-f)";

						case "error_invalid_args":
							return "Ungültige Argumente. Schau mal in /help";

						case "info_success_add":
							return "&2{0} erfolgreich hinzugefügt. Hervorhebung: {1}";

						case "info_success_remove":
							return "&2{0} erfolgreich aus der Liste entfernt.";

						case "info_failed_notfound":
							return "Das Wort {0} wurde in der Liste nicht gefunden.";

						default:
							return string.Format("&4FEHLER: &fString '{0}' existiert nicht.", value);
					}
				#endregion

				#region English translation
				case "EN":
					switch (value)
					{
						case "error_no_permission":
							return "You are not allowed to manage highlighting of words.";

						case "error_contains_semicolon":
							return "The specified word may not contain a semicolon (;)";

						case "error_non_hexadecimal":
							return "The color code must be given as a hexadecimal value (0-9, a-f)";

						case "error_invalid_args":
							return "Invalid arguments. Type /help to see command's usage.";

						case "info_success_add":
							return "&2Successfully added {0}. Replacement: {1}";

						case "info_success_remove":
							return "&2Successfully removed {0} from the list.";

						case "info_failed_notfound":
							return "The word {0} was not found in the list.";

						default:
							return string.Format("&4ERROR: &fString '{0}' does not exist.", value);
					}
				#endregion

				default:
					return string.Format("&4ERROR: &fThe language code {0} is not in the list.", languageCode);
			}
		}

		string OnChat(int player, string message, bool toTeam)
		{
			string newMessage = "";
			string[] args;
			try
			{
				args = message.Split(' ');
			}
			catch
			{
				return message;
			}

			for (int i = 0; i < args.Length; i++)
			{
				foreach (Highlight highlight in highlightedWords)
				{
					if (highlight.word.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
					{
						args[i] = highlight.color + args[i] + "&f";
					}
				}
				newMessage += (args[i] + " ");
			}
			return newMessage;
		}

		void OnSave()
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(filePath))
				{
					foreach (Highlight h in highlightedWords)
					{
						sw.WriteLine(h.word + ";" + h.color);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[ChatHighlighter] ERROR:  " + ex.Message);
			}
		}

		void OnLoad()
		{
			highlightedWords.Clear();
			if (!File.Exists(filePath))
			{
				Console.WriteLine("[ChatHighlighter] " + filePath + " not found. Creating new.");
				OnSave();
			}
			DirectoryInfo di = new DirectoryInfo(filePath);
			try
			{
				using (TextReader tr = new StreamReader(di.FullName, Encoding.UTF8))
				{
					string line = tr.ReadLine();
					char[] separator = new char[1];
					separator[0] = ';';
					while (!string.IsNullOrEmpty(line))
					{
						string[] highlight = line.Split(separator);
						if (!string.IsNullOrEmpty(highlight[0]) && !string.IsNullOrEmpty(highlight[1]))
						{
							Highlight h = new Highlight(highlight[0], highlight[1]);
							highlightedWords.Add(h);
						}
						else
						{
							Console.WriteLine("[ChatHighlighter] ERROR:  Invalid entry in ChatHighlights.txt detected.");
						}
						line = tr.ReadLine();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[ChatHighlighter] ERROR:  " + ex.Message);
			}
		}

		bool OnCommand(int player, string command, string argument)
		{
			if (command.Equals("highlight_add", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "highlight"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_no_permission"));
					Console.WriteLine(string.Format("[ChatHighlighter] {0} tried to add a highlighted word (no permission)", m.GetPlayerName(player)));
					return true;
				}
				string[] args;
				try
				{
					args = argument.Split(' ');
					if (args[0].Contains(";"))
					{
						m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_contains_semicolon"));
						return true;
					}
					string color = args[1].ToLower();
					switch (color)
					{
						//Check for hexadecimal values.
						case "0":
						case "1":
						case "2":
						case "3":
						case "4":
						case "5":
						case "6":
						case "7":
						case "8":
						case "9":
						case "a":
						case "b":
						case "c":
						case "d":
						case "e":
						case "f":
							Highlight h = new Highlight(args[0], "&" + color);
							highlightedWords.Add(h);
							OnSave();
							m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_success_add"), h.word, h.color + h.word));
							break;

						//Non hexadecimal value. Error message and abort
						default:
							m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_non_hexadecimal"));
							break;
					}
				}
				catch
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_invalid_args"));
					return true;
				}
				return true;
			}
			if (command.Equals("highlight_remove", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "highlight"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_no_permission"));
					Console.WriteLine(string.Format("[ChatHighlighter] {0} tried to remove a highlighted word (no permission)", m.GetPlayerName(player)));
					return true;
				}
				bool isFound = false;
				for (int i = 0; i < highlightedWords.Count; i++)
				{
					if (highlightedWords[i].word.Equals(argument, StringComparison.InvariantCultureIgnoreCase))
					{
						highlightedWords.RemoveAt(i);
						OnSave();
						m.SendMessage(player, chatPrefix + string.Format(GetLocalizedString("info_success_remove"), argument));
						Console.WriteLine(string.Format("[ChatHighlighter] {0} removed highlighted word {1}", m.GetPlayerName(player), argument));
						isFound = true;
						break;
					}
				}
				if (!isFound)
				{
					m.SendMessage(player, chatPrefix + m.colorError() + string.Format(GetLocalizedString("info_failed_notfound"), argument));
				}
				return true;
			}
			if (command.Equals("highlight_reload", StringComparison.InvariantCultureIgnoreCase))
			{
				if (!m.PlayerHasPrivilege(player, "highlight"))
				{
					m.SendMessage(player, chatPrefix + m.colorError() + GetLocalizedString("error_no_permission"));
					Console.WriteLine(string.Format("[ChatHighlighter] {0} tried to reload highlights (no permission)", m.GetPlayerName(player)));
					return true;
				}
				OnLoad();
				return true;
			}
			return false;
		}
	}
}
