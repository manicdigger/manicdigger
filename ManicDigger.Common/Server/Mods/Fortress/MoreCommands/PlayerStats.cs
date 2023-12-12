/*
* Stats Mod - Version 1.1
* last change: 2023-12-12
* Author: croxxx
* 
* This allows privileged users to display the following:
* -first seen date
* -last seen date
* -time played on server
* -number of block changes
* -number of chat messages sent
* -Last known IP adress
* 
* The command is /pstat [playername]
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ManicDigger.Mods
{
    public class PlayerStats : IMod
    {
        public void PreStart(ModManager m) { }

        public void Start(ModManager m)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    System.Console.WriteLine("[PlayerStats] Directory " + path + " does not exist - trying to create it...");
                    Directory.CreateDirectory(path);
                    System.Console.WriteLine("[PlayerStats] Success!");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("[PlayerStats] ERROR: " + ex.Message);
                }
            }
            else
                System.Console.WriteLine("[PlayerStats] Found " + path + ". Using it.");

            this.m = m;

            m.RegisterOnCommand(DisplayUserData);
            m.RegisterCommandHelp("pstat", "Displays further information about the specified user");
            m.RegisterPrivilege("pstat");

            m.RegisterTimer(PlayTime_Tick, (double)1);
            m.RegisterOnPlayerJoin(PlayerJoin);
            m.RegisterOnPlayerLeave(PlayerLeave);
            m.RegisterOnPlayerChat(PlayerChat);
            m.RegisterOnBlockBuild(PlayerBuild);
            m.RegisterOnBlockDelete(PlayerDelete);

            m.RegisterOnSave(SavePlayerStats);

            System.Console.WriteLine("[PlayerStats] Loaded Mod Version 1.1");
        }

        //Internal variables.
        //DO NOT CHANGE! TODO change directory to save Data folder
        ModManager m;
        List<Player> onlinePlayers = new List<Player>();
        string path = "UserData" + Path.DirectorySeparatorChar + "UserStats";
        string chatPrefix = "&8[&6PlayerStats&8] ";

        struct Player
        {
            //public Player(){}
            public string name;
            public string firstSeen;
            public string lastSeen;
            public string lastIP;
            public int playTime;
            public int blocksPlaced;
            public int blocksDeleted;
            public int messagesSent;
        }

        void PlayerJoin(int player)
        {
            bool playerExists = false;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles("*.txt");
            foreach (FileInfo fi in files)
            {
                char[] trenner = new char[1];
                trenner[0] = '.';
                string fname = fi.Name.Split(trenner, 2)[0];
                if (m.GetPlayerName(player).Equals(fname, StringComparison.InvariantCultureIgnoreCase))
                {
                    playerExists = true;
                    try
                    {
                        using (TextReader tr = new StreamReader(fi.FullName))
                        {
                            Player p = new Player();
                            p.name = tr.ReadLine();
                            p.firstSeen = tr.ReadLine();
                            p.lastSeen = tr.ReadLine();
                            p.lastIP = m.GetPlayerIp(player); tr.ReadLine();
                            p.playTime = int.Parse(tr.ReadLine());
                            p.blocksPlaced = int.Parse(tr.ReadLine());
                            p.blocksDeleted = int.Parse(tr.ReadLine());
                            p.messagesSent = int.Parse(tr.ReadLine());
                            onlinePlayers.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("[PlayerStats] ERROR:  " + ex.Message);
                    }
                }
            }
            if (!playerExists)
            {
                Player p = new Player();
                p.name = m.GetPlayerName(player);
                p.firstSeen = DateTime.Now.ToString("dd.MM.yyyy H:mm:ss zzz");
                p.lastSeen = DateTime.Now.ToString("dd.MM.yyyy H:mm:ss zzz");
                p.lastIP = m.GetPlayerIp(player);
                p.playTime = 0;
                p.blocksPlaced = 0;
                p.blocksDeleted = 0;
                p.messagesSent = 0;
                onlinePlayers.Add(p);
                SavePlayer(p);
            }
        }

        void PlayerLeave(int player)
        {
            for (int i = 0; i < onlinePlayers.Count; i++)
            {
                if (onlinePlayers[i].name.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
                {
                    Player p = onlinePlayers[i];
                    p.lastSeen = DateTime.Now.ToString("dd.MM.yyyy H:mm:ss zzz");
                    SavePlayer(p);
                    onlinePlayers.RemoveAt(i);
                }
            }
        }

        string PlayerChat(int player, string message, bool toTeam)
        {
            for (int i = 0; i < onlinePlayers.Count; i++)
            {
                if (onlinePlayers[i].name.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
                {
                    Player p = onlinePlayers[i];
                    p.messagesSent++;
                    onlinePlayers[i] = p;
                }
            }
            return message;
        }

        void PlayerBuild(int player, int x, int y, int z)
        {
            for (int i = 0; i < onlinePlayers.Count; i++)
            {
                if (onlinePlayers[i].name.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
                {
                    Player p = onlinePlayers[i];
                    p.blocksPlaced++;
                    onlinePlayers[i] = p;
                }
            }
        }

        void PlayerDelete(int player, int x, int y, int z, int blockID)
        {
            for (int i = 0; i < onlinePlayers.Count; i++)
            {
                if (onlinePlayers[i].name.Equals(m.GetPlayerName(player), StringComparison.InvariantCultureIgnoreCase))
                {
                    Player p = onlinePlayers[i];
                    p.blocksDeleted++;
                    onlinePlayers[i] = p;
                }
            }
        }

        void SavePlayerStats()
        {
            foreach (Player p in onlinePlayers)
            {
                SavePlayer(p);
            }
        }

        void SavePlayer(Player p)
        {
            //m.SendMessageToAll(path + Path.DirectorySeparatorChar + p.name.ToLower() + ".txt");
            try
            {
                using (StreamWriter sw = new StreamWriter(path + Path.DirectorySeparatorChar + p.name.ToLower() + ".txt"))
                {
                    sw.WriteLine(p.name);
                    sw.WriteLine(p.firstSeen);
                    sw.WriteLine(p.lastSeen);
                    sw.WriteLine(p.lastIP);
                    sw.WriteLine(p.playTime);
                    sw.WriteLine(p.blocksPlaced);
                    sw.WriteLine(p.blocksDeleted);
                    sw.WriteLine(p.messagesSent);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("[PlayerStats] ERROR:  " + ex.Message);
            }
        }

        bool DisplayUserData(int player, string command, string argument)
        {
            if (command.Equals("pstat", StringComparison.InvariantCultureIgnoreCase))
            {
                if (!m.PlayerHasPrivilege(player, "pstat"))
                {
                    m.SendMessage(player, chatPrefix + m.colorError() + "You are not allowed to view player statistics.");
                    System.Console.WriteLine(string.Format("[PlayerStats] {0} tried to view player statistics (no permission)", m.GetPlayerName(player)));
                    return true;
                }

                Player p = new Player();
                bool playerFound = false;

                DirectoryInfo di = new DirectoryInfo(path);
                FileInfo[] files = di.GetFiles("*.txt");
                if (m.IsSinglePlayer() && argument == "")
                    argument = "local";

                foreach (FileInfo fi in files)
                {
                    char[] trenner = new char[1];
                    trenner[0] = '.';
                    string fname = fi.Name.Split(trenner, 2)[0];
                    if (!argument.Equals(fname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    else
                    {
                        playerFound = true;
                    }
                    try
                    {
                        using (TextReader tr = new StreamReader(fi.FullName))
                        {
                            p.name = tr.ReadLine();
                            p.firstSeen = tr.ReadLine();
                            p.lastSeen = tr.ReadLine();
                            p.lastIP = tr.ReadLine();
                            p.playTime = int.Parse(tr.ReadLine());
                            p.blocksPlaced = int.Parse(tr.ReadLine());
                            p.blocksDeleted = int.Parse(tr.ReadLine());
                            p.messagesSent = int.Parse(tr.ReadLine());
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("[PlayerStats] ERROR:  " + ex.Message);
                    }
                }

                if (playerFound)
                {
                    m.SendMessage(player, chatPrefix + string.Format("&7Player statistics for &f{0}:", p.name));
                    m.SendMessage(player, "&7--------------------------------------------------");
                    m.SendMessage(player, string.Format("&7Last known IP: &f{0}", p.lastIP));
                    m.SendMessage(player, string.Format("&7First seen: &f{0}", p.firstSeen));
                    m.SendMessage(player, string.Format("&7Last seen: &f{0}", p.lastSeen));
                    m.SendMessage(player, string.Format("&7Total ingame time: &f{0}", timeToString(p.playTime)));
                    m.SendMessage(player, string.Format("&7Blocks placed: &f{0}", p.blocksPlaced.ToString()));
                    m.SendMessage(player, string.Format("&7Blocks destroyed: &f{0}", p.blocksDeleted.ToString()));
                    m.SendMessage(player, string.Format("&7Chat messages sent: &f{0}", p.messagesSent.ToString()));
                    m.SendMessage(player, "&7--------------------------------------------------");
                    System.Console.WriteLine(string.Format("[PlayerStats] {0} displayed player statistics of {1}.", m.GetPlayerName(player), argument));
                    playerFound = false;
                }
                else
                {
                    m.SendMessage(player, chatPrefix + m.colorError() + string.Format("No record found for player {0}.", argument));
                }
                return true;
            }
            return false;
        }

        void PlayTime_Tick()
        {
            for (int i = 0; i < onlinePlayers.Count; i++)
            {
                Player p = onlinePlayers[i];
                p.playTime++;
                p.lastSeen = DateTime.Now.ToString("dd.MM.yyyy H:mm:ss zzz");
                onlinePlayers[i] = p;
            }
        }

        string timeToString(int seconds)
        {
            int days = 0, hours = 0, minutes = 0;
            if (seconds >= 86400)
            {
                days = seconds / 86400;
                seconds -= (days * 86400);
            }
            if (seconds >= 3600)
            {
                hours = seconds / 3600;
                seconds -= (hours * 3600);
            }
            if (seconds >= 60)
            {
                minutes = seconds / 60;
                seconds -= (minutes * 60);
            }
            return string.Format("{0} days {1} hours {2} minutes {3} seconds", days, hours, minutes, seconds);
        }
    }
}