using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using GameModeFortress;
using ManicDigger;
using System.Drawing;

namespace ManicDiggerServer
{
    public partial class Server
    {
        public void CommandInterpreter(int sourceClientId, string command, string argument)
        {
            string[] ss;
            int id;

            switch (command)
            {
                case "msg":
                case "pm":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length >= 2)
                    {
                        this.PrivateMessage(sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "op":
                case "chgrp":
                case "cg":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 2)
                    {
                        this.ChangeGroup(sourceClientId, ss[0], ss[1]);
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "login":
                     // enables to change temporary group with a group's password (only if group allows it)
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 2)
                    {
                        this.Login(sourceClientId, ss[0], ss[1]);
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "welcome":
                    this.WelcomeMessage(sourceClientId, argument);
                    return;
                case "announcement":
                    this.Announcement(sourceClientId, argument);
                    return;
                case "logging":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 1)
                    {
                        this.SetLogging(sourceClientId, ss[0], "");
                        return;
                    }
                    if (ss.Length == 2)
                    {
                        this.SetLogging(sourceClientId, ss[0], ss[1]);
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "kick_id":
                    ss = argument.Split(new[] { ' ' });
                    if (!Int32.TryParse(ss[0], out id))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    if (ss.Length >= 2)
                    {
                        this.Kick(sourceClientId, id, string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.Kick(sourceClientId, id);
                    return;
                case "kick":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length >= 2)
                    {
                        this.Kick(sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.Kick(sourceClientId, argument);
                    return;
                case "banip_id":
                    ss = argument.Split(new[] { ' ' });
                    if (!Int32.TryParse(ss[0], out id))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    if (ss.Length >= 2)
                    {
                        this.BanIP(sourceClientId, id, string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.BanIP(sourceClientId, id);
                    return;
                case "banip":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length >= 2)
                    {
                        this.BanIP(sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.BanIP(sourceClientId, argument);
                    return;
                case "ban_id":
                    ss = argument.Split(new[] { ' ' });
                    if (!Int32.TryParse(ss[0], out id))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    if (ss.Length >= 2)
                    {
                        this.Ban(sourceClientId, id, string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.Ban(sourceClientId, id);
                    return;
                case "ban":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length >= 2)
                    {
                        this.Ban(sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.Ban(sourceClientId, argument);
                    return;
                case "unban":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 2)
                    {
                        this.Unban(sourceClientId, ss[0], ss[1]);
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "list":
                    this.List(sourceClientId, argument);
                    return;
                case "giveall":
                    this.GiveAll(sourceClientId, argument);
                    return;
                case "give":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 3)
                    {
                        int amount;
                        if (!Int32.TryParse(ss[2], out amount))
                        {
                            SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                            return;
                        }
                        else
                        {
                            this.Give(sourceClientId, ss[0], ss[1], amount);
                        }
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "monsters":
                    if (!argument.Equals("off") && !argument.Equals("on"))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    this.Monsters(sourceClientId, argument);
                    return;
                case "area_add":
                    int areaId;
                    ss = argument.Split(new [] { ' ' });

                    if (ss.Length < 4 || ss.Length > 5)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }

                    if (!Int32.TryParse(ss[0], out areaId))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid argument. Type /help to see command's usage.");
                        return;
                    }
                    string coords = ss[1];
                    string[] permittedGroups = ss[2].ToString().Split(new [] { ',' });
                    string[] permittedUsers = ss[3].ToString().Split(new [] { ',' });

                    int? areaLevel;
                    try
                    {
                        areaLevel = Convert.ToInt32(ss[4]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        areaLevel = null;
                    }
                    catch (FormatException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    catch (OverflowException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }

                    this.AreaAdd(sourceClientId, areaId, coords, permittedGroups, permittedUsers, areaLevel);
                    return;
                case "area_delete":
                    if (!Int32.TryParse(argument, out areaId))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid argument. Type /help to see command's usage.");
                        return;
                    }
                    this.AreaDelete(sourceClientId, areaId);
                    return;
                case "help":
                    this.Help(sourceClientId);
                    return;
                case "run":
                case "":
                    // JavaScript
                    // assume script expression or command coming
                    var script = argument;
                    RunInClientSandbox(script, sourceClientId);
                    return;
                case "crash":
                    KillPlayer(sourceClientId);
                    return;
                case "set_spawn":
                    //           0    1      2 3 4
                    // agrument: type target x y z
                    ss = argument.Split(new[] { ' ' });

                    if (ss.Length < 3 || ss.Length > 5)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }

                    // Add an empty target argument, when user sets default spawn.
                    if (ss[0].Equals("-d") || ss[0].Equals("-default"))
                    {
                        string[] ssTemp = new string[ss.Length + 1];
                        ssTemp[0] = ss[0];
                        ssTemp[1] = "";
                        Array.Copy(ss, 1, ssTemp, 2, ss.Length - 1);
                        ss = ssTemp;
                    }

                    int x;
                    int y;
                    int? z;
                    try
                    {
                        x = Convert.ToInt32(ss[2]);
                        y = Convert.ToInt32(ss[3]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid spawn position.");
                        return;
                    }
                    catch (FormatException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid spawn position.");
                        return;
                    }
                    catch (OverflowException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid spawn position.");
                        return;
                    }

                    try
                    {
                        z = Convert.ToInt32(ss[4]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        z = null;
                    }
                    catch (FormatException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid spawn position.");
                        return;
                    }
                    catch (OverflowException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid spawn position.");
                        return;
                    }
                    this.SetSpawnPosition(sourceClientId, ss[0], ss[1], x, y, z);
                    return;
                case "privilege_add":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length != 2)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    this.PrivilegeAdd(sourceClientId, ss[0], ss[1]);
                    return;
                case "privilege_remove":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length != 2)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    this.PrivilegeRemove(sourceClientId, ss[0], ss[1]);
                    return;
                case "restart":
                    this.RestartServer(sourceClientId);
                    break;
                //case "crashserver": for (; ; ) ;
                case "teleport_player":
                    ss = argument.Split(new[] { ' ' });

                    if (ss.Length < 3 || ss.Length > 4)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }

                    try
                    {
                        x = Convert.ToInt32(ss[1]);
                        y = Convert.ToInt32(ss[2]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid position.");
                        return;
                    }
                    catch (FormatException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid position.");
                        return;
                    }
                    catch (OverflowException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid position.");
                        return;
                    }

                    try
                    {
                        z = Convert.ToInt32(ss[3]);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        z = null;
                    }
                    catch (FormatException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid position.");
                        return;
                    }
                    catch (OverflowException)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid position.");
                        return;
                    }
                    this.TeleportPlayer(sourceClientId, ss[0], x, y, z);
                    break;
                case "backup_database":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.backup))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        break;
                    }
                    this.BackupDatabase(argument);
                    SendMessage(sourceClientId, string.Format("{0}Backup created.", colorSuccess));
                    break;
                default:
                    SendMessage(sourceClientId, colorError + "Unknown command /" + command);
                    return;
            }
        }

        public void Help(int sourceClientId)
        {
            SendMessage(sourceClientId, colorHelp + "Available privileges:");
            foreach (ServerClientMisc.Privilege privilege in GetClient(sourceClientId).privileges)
            {
                SendMessage(sourceClientId,string.Format("{0}{1}: {2}",colorHelp, privilege.ToString(), this.CommandHelp(privilege.ToString())));
            }
        }

        private string CommandHelp(string command)
        {
            switch (command)
            {
                case "msg":
                case "pm":
                    return "/msg [username] [message]";
                case "kick":
                    return "/kick [username] {reason}";
                case "kick_id":
                    return "kick_id [player id] {reason}";
                case "ban":
                    return "/ban [username] {reason}";
                case "ban_id":
                    return "/ban_id [player id] {reason}";
                case "banip":
                    return "/banip [username] {reason}";
                case "banip_id":
                    return "/banip_id [player id] {reason}";
                case "unban":
                    return "/unban [-p playername | -ip ipaddress]";
                case "run":
                    return "/run [JavaScript (max. length 4096 char.)]";
                case "op":
                    return "/op [username] [group]";
                case "chgrp":
                    return "/chgrp [username] [group]";
                case "login":
                    return "/login [group] [password]";
                case "welcome":
                    return "/welcome [login motd message]";
                case "logging":
                    return "/logging [-s | -b | -se | -c] {on | off}";
                case "list_clients":
                    return "/list [-clients]";
                case "list_saved_clients":
                    return "/list [-saved_clients]";
                case "list_groups":
                    return "/list [-groups]";
                case "list_banned_users":
                    return "/list [-bannedusers | -bannedips]";
                case "list_areas":
                    return "/list [-areas]";
                case "give":
                    return "/give [username] blockname amount";
                case "giveall":
                    return "/giveall [username]";
                case "monsters":
                    return "/monsters [on|off]";
                case "area_add":
                    return "/area_add [ID] [x1,x2,y1,y2,z1,z2] [group1,group2,..] [user1,user2,..] {level}";
                case "area_delete":
                    return "/area_delete [ID]";
                case "announcement":
                    return "/announcement [message]";
                case "set_spawn":
                    return "/set_spawn [-default|-group|-player] [target] [x] [y] {z}";
                case "privilege_add":
                    return "/privilege_add [username] [privilege]";
                case "privilege_remove":
                    return "/privilege_remove [username] [privilege]";
                case "restart":
                    return "/restart";
                case "teleport_player":
                    return "/teleport_player [target] [x] [y] {z}";
                case "backup":
                    return "/backup [filename]";
                default:
                    return "No description available.";
            }
        }

        public bool PrivateMessage(int sourceClientId, string recipient, string message)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.pm))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            Client targetClient = GetClient(recipient);
            if (targetClient != null)
            {
                SendMessage(targetClient.Id, string.Format("PM {0}: {1}", targetClient.ColoredPlayername(colorNormal), message));
                SendMessage(sourceClientId, string.Format("PM -> {0}: {1}", targetClient.ColoredPlayername(colorNormal), message));
                // TODO: move message sound to client
                //SendSound(k.Key, "message.wav");
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, recipient));
            return false;
        }

        public bool ChangeGroup(int sourceClientId, string target, string newGroupName)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.chgrp))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            // Get related group from config file.
            GameModeFortress.Group newGroup = serverClient.Groups.Find(
                delegate(GameModeFortress.Group grp)
                {
                    return grp.Name.Equals(newGroupName, StringComparison.InvariantCultureIgnoreCase);
                }
            );
            if (newGroup == null)
            {
                SendMessage(sourceClientId, string.Format("{0}Group {1} not found.", colorError, newGroupName));
                return false;
            }

            // Forbid to assign groups with levels higher then the source's client group level.
            if (newGroup.IsSuperior(GetClient(sourceClientId).clientGroup))
            {
                SendMessage(sourceClientId, string.Format("{0}The target group is superior your group.", colorError));
                return false;
            }

            // Get related client from config file
            GameModeFortress.Client clientConfig = serverClient.Clients.Find(
                delegate(GameModeFortress.Client client)
                {
                    return client.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase);
                }
            );

            // Get related client.
            Client targetClient = GetClient(target);

            if (targetClient != null)
            {
                if (targetClient.clientGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target user is superior or equal.", colorError));
                    return false;
                }
                // Add or change group membership in config file.

                // Client is not yet in config file. Create a new entry.
                if (clientConfig == null)
                {
                    clientConfig = new GameModeFortress.Client();
                    clientConfig.Name = targetClient.playername;
                    clientConfig.Group = newGroup.Name;
                    serverClient.Clients.Add(clientConfig);
                }
                else
                {
                    clientConfig.Group = newGroup.Name;
                }
                SaveServerClient();
                SendMessageToAll(string.Format("{0}New group for {1}: {2}", colorSuccess, targetClient.ColoredPlayername(colorSuccess), newGroup.GroupColorString() + newGroupName));
                ServerEventLog(String.Format("{0} sets group of {1} to {2}.", GetClient(sourceClientId).playername, targetClient.playername, newGroupName));
                targetClient.AssignGroup(newGroup);
                return true;
            }

            // Target is at the moment not online. Create or change anyway a entry in ServerClient
            if (clientConfig == null)
            {
                clientConfig = new GameModeFortress.Client();
                clientConfig.Name = target;
                clientConfig.Group = newGroup.Name;
                serverClient.Clients.Add(clientConfig);
            }
            else
            {
                // Check if target's current group is superior.
                GameModeFortress.Group oldGroup = serverClient.Groups.Find(
                    delegate(GameModeFortress.Group grp)
                {
                    return grp.Name.Equals(clientConfig.Group);
                }
                );
                if (oldGroup == null)
                {
                    SendMessage(sourceClientId, string.Format("{0}Invalid group.", colorError));
                    return false;
                }
                if (oldGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || oldGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target user is superior or equal.", colorError));
                    return false;
                }
                clientConfig.Group = newGroup.Name;
            }

            SaveServerClient();
            SendMessageToAll(string.Format("{0}New group for {1}: {2} (offline)", colorSuccess, target, newGroup.GroupColorString() + newGroupName));
            ServerEventLog(String.Format("{0} sets group of {1} to {2} (offline).", GetClient(sourceClientId), target, newGroupName));
            return true;
        }

        public bool Login (int sourceClientId, string targetGroupString, string password)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.login))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            GameModeFortress.Group targetGroup = serverClient.Groups.Find(
                delegate(GameModeFortress.Group grp)
                {
                    return grp.Name.Equals(targetGroupString, StringComparison.InvariantCultureIgnoreCase);
                }
            );
            if (targetGroup == null)
            {
                SendMessage(sourceClientId, string.Format("{0}Group {1} not found.", colorError, targetGroupString));
                return false;
            }
            if (string.IsNullOrEmpty(targetGroup.Password))
            {
                SendMessage(sourceClientId, string.Format("{0}Group {1} doesn't allow password access.", colorError, targetGroupString));
                return false;
            }
            if (targetGroup.Password.Equals(password))
            {
                GetClient(sourceClientId).AssignGroup(targetGroup);
                SendMessageToAll(string.Format("{0}{1} logs in group {2}.", colorSuccess, GetClient(sourceClientId).ColoredPlayername(colorSuccess), targetGroupString));
                SendMessage(sourceClientId, "Type /help see your available privileges.");
                ServerEventLog(string.Format("{0} logs in group {1}.", GetClient(sourceClientId).playername, targetGroupString));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Invalid password.", colorError));
            ServerEventLog(string.Format("{0} fails to log in (invalid password: {1}).", GetClient(sourceClientId).playername, password));
            return false;
        }

        public bool WelcomeMessage(int sourceClientId, string welcomeMessage)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.welcome))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            config.WelcomeMessage = welcomeMessage;
            SendMessageToAll(string.Format("{0}{1} set new welcome message: {2}", colorSuccess, GetClient(sourceClientId).ColoredPlayername(colorSuccess), welcomeMessage));
            ServerEventLog(string.Format("{0} changes welcome message to {1}.", GetClient(sourceClientId).playername, welcomeMessage));
            SaveConfig();
            return true;
        }

        public bool SetLogging(int sourceClientId, string type, string option)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.logging))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            switch (type)
            {
            // all logging state
                case "-s":
                    SendMessage(sourceClientId, "Build: " + config.BuildLogging);
                    SendMessage(sourceClientId, "Server events: " + config.ServerEventLogging);
                    SendMessage(sourceClientId, "Chat: " + config.ChatLogging);
                    return true;
                case "-b":
                    if (option.Equals("on"))
                    {
                        config.BuildLogging = true;
                        SaveConfig();
                        SendMessage(sourceClientId, string.Format("{0}Build logging enabled.", colorSuccess));
                        ServerEventLog(string.Format("{0} enables build logging.", GetClient(sourceClientId).playername));
                        return true;
                    }
                    if (option.Equals("off"))
                    {
                        config.BuildLogging = false;
                        SaveConfig();
                        SendMessage(sourceClientId, string.Format("{0}Build logging disabled.", colorSuccess));
                        ServerEventLog(string.Format("{0} disables build logging.", GetClient(sourceClientId).playername));
                        return true;
                    }
                    SendMessage(sourceClientId, string.Format("{0}Build logging: {1}", colorNormal, config.BuildLogging));
                    return true;
                case "-se":
                    if (option.Equals("on"))
                    {
                        config.ServerEventLogging = true;
                        SaveConfig();
                        SendMessage(sourceClientId, string.Format("{0}Server event logging enabled.", colorSuccess));
                        ServerEventLog(string.Format("{0} enables server event logging.", GetClient(sourceClientId).playername));
                        return true;
                    }
                    if (option.Equals("off"))
                    {
                        ServerEventLog(string.Format("{0} disables server event logging.", GetClient(sourceClientId).playername));
                        config.ServerEventLogging = false;
                        SaveConfig();
                        SendMessage(sourceClientId, string.Format("{0}Server event logging disabled.", colorSuccess));
                        return true;
                    }
                    SendMessage(sourceClientId, string.Format("{0}Server event logging: {1}", colorNormal, config.ServerEventLogging));
                    return true;
                case "-c":
                    if (option.Equals("on"))
                    {
                        config.ChatLogging = true;
                        SaveConfig();
                        SendMessage(sourceClientId, string.Format("{0}Chat logging enabled.", colorSuccess));
                        ServerEventLog(string.Format("{0} enables chat logging.", GetClient(sourceClientId).playername));
                        return true;
                    }
                    if (option.Equals("off"))
                    {
                        config.ChatLogging = false;
                        SaveConfig();
                        SendMessage(sourceClientId, string.Format("{0}Chat logging disabled.", colorSuccess));
                        ServerEventLog(string.Format("{0} disables chat logging.", GetClient(sourceClientId).playername));
                        return true;
                    }
                    SendMessage(sourceClientId, string.Format("{0}Chat logging: {1}", colorNormal, config.ChatLogging));
                    return true;
                default:
                    SendMessage(sourceClientId, string.Format("{0}Invalid type: {1}", colorError, type));
                    return false;
            }
        }

        public bool Kick(int sourceClientId, string target)
        {
            return Kick(sourceClientId, target, "");
        }

        public bool Kick(int sourceClientId, string target, string reason)
        {
            Client targetClient = GetClient(target);
            if (targetClient != null)
            {
                return this.Kick(sourceClientId, targetClient.Id, reason);
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
            return false;
        }

        public bool Kick(int sourceClientId, int targetClientId)
        {
            return this.Kick(sourceClientId, targetClientId, "");
        }

        public bool Kick(int sourceClientId, int targetClientId, string reason)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.kick))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason;
            }
            Client targetClient = GetClient(targetClientId);
            if (targetClient != null)
            {
                if (targetClient.clientGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target is superior or equal.", colorError));
                    return false;
                }
                string targetName = targetClient.playername;
                string sourceName = GetClient(sourceClientId).playername;
                string targetNameColored = targetClient.ColoredPlayername(colorImportant);
                string sourceNameColored = GetClient(sourceClientId).ColoredPlayername(colorImportant);
                SendMessageToAll(string.Format("{0}{1} was kicked by {2}", colorImportant, targetNameColored, sourceNameColored));
                ServerEventLog(string.Format("{0} kicks {1}.{2}", sourceName, targetName, reason));
                SendDisconnectPlayer(targetClientId, string.Format("You were kicked by an administrator.{0}", reason));
                KillPlayer(targetClientId);
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player ID {1} does not exist.", colorError, targetClientId));
            return false;
        }

        public bool Ban(int sourceClientId, string target)
        {
            return Ban(sourceClientId, target, "");
        }

        public bool Ban(int sourceClientId, string target, string reason)
        {
            Client targetClient = GetClient(target);
            if (targetClient != null)
            {
                return this.Ban(sourceClientId, targetClient.Id, reason);
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
            return false;
        }

        public bool Ban(int sourceClientId, int targetClientId)
        {
            return this.Ban(sourceClientId, targetClientId, "");
        }

        public bool Ban(int sourceClientId, int targetClientId, string reason)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.ban))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason;
            }
            Client targetClient = GetClient(targetClientId);
            if (targetClient != null)
            {
                if (targetClient.clientGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target is superior or equal.", colorError));
                    return false;
                }
                string targetName = targetClient.playername;
                string sourceName = GetClient(sourceClientId).playername;
                string targetNameColored = targetClient.ColoredPlayername(colorImportant);
                string sourceNameColored = GetClient(sourceClientId).ColoredPlayername(colorImportant);
                config.BannedUsers.Add(targetName);
                SaveConfig();
                SendMessageToAll(string.Format("{0}{1} was banned by {2}", colorImportant, targetNameColored, sourceNameColored));
                ServerEventLog(string.Format("{0} bans {1}.{2}", sourceName, targetName, reason));
                SendDisconnectPlayer(targetClientId, string.Format("You were banned by an administrator.{0}", reason));
                KillPlayer(targetClientId);
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player ID {1} does not exist.", colorError, targetClientId));
            return false;
        }

        public bool BanIP(int sourceClientId, string target)
        {
            return BanIP(sourceClientId, target, "");
        }

        public bool BanIP(int sourceClientId, string target, string reason)
        {
            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                return this.BanIP(sourceClientId, targetClient.Id, reason);
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
            return false;
        }

        public bool BanIP(int sourceClientId, int targetClientId)
        {
            return this.BanIP(sourceClientId, targetClientId, "");
        }

        public bool BanIP(int sourceClientId, int targetClientId, string reason)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.banip))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason;
            }
            Client targetClient = GetClient(targetClientId);
            if(targetClient != null)
            {
                if (targetClient.clientGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target is superior or equal.", colorError));
                    return false;
                }
                string targetName = targetClient.playername;
                string sourceName = GetClient(sourceClientId).playername;
                string targetNameColored = targetClient.ColoredPlayername(colorImportant);
                string sourceNameColored = GetClient(sourceClientId).ColoredPlayername(colorImportant);
                config.BannedIPs.Add(((IPEndPoint)targetClient.socket.RemoteEndPoint).Address.ToString());
                SaveConfig();
                SendMessageToAll(string.Format("{0}{1} was IP banned by {2}", colorImportant, targetNameColored, sourceNameColored));
                ServerEventLog(string.Format("{0} IP bans {1}.{2}", sourceName, targetName, reason));
                SendDisconnectPlayer(targetClientId, string.Format("You were IP banned by an administrator.{0}", reason));
                KillPlayer(targetClientId);
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player ID {1} does not exist.", colorError, targetClientId));
            return false;
        }

        public bool Unban(int sourceClientId, string type, string target)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.unban))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            // unban a playername
            if (type.Equals("-p"))
            {
                // case insensitive
                bool exists = config.UnbanPlayer(target);
                SaveConfig();
                if (!exists)
                {
                    SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
                }
                else
                {
                    SendMessage(sourceClientId, string.Format("{0}Player {1} unbanned.", colorSuccess, target));
                    ServerEventLog(string.Format("{0} unbans player {1}.", GetClient(sourceClientId).playername, target));
                }
                return true;
            }
            // unban an IP
            else if (type.Equals("-ip"))
            {
                bool exists = config.BannedIPs.Remove(target);
                SaveConfig();
                if (!exists)
                {
                    SendMessage(sourceClientId, string.Format("{0}IP {1} not found.", colorError, target));
                }
                else
                {
                    SendMessage(sourceClientId, string.Format("{0}IP {1} unbanned.", colorSuccess, target));
                    ServerEventLog(string.Format("{0} unbans IP {1}.", GetClient(sourceClientId).playername, target));
                }
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Invalid type: {1}", colorError, type));
            return false;
        }

        public bool List(int sourceClientId, string type)
        {
            switch (type)
            {
                case "-clients":
                case "-c":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.list_clients))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Players:");
                    foreach (var k in clients)
                    {
                        // Format: Key Playername:Group:Privileges IP
                        SendMessage(sourceClientId, string.Format("{0} {1}", k.Key, k.Value.ToString()));
                    }
                    return true;
                case "-areas":
                case "-a":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.list_areas))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Areas:");
                    foreach (AreaConfig area in config.Areas)
                    {
                        SendMessage(sourceClientId, area.ToString());
                    }
                    return true;
                case "-bannedusers":
                case "-bu":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.list_banned_users))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Banned Users:");
                    foreach (string currentUser in config.BannedUsers)
                    {
                        SendMessage(sourceClientId, currentUser);
                    }
                    return true;
                case "-bannedips":
                case "-bip":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.list_banned_users))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Banned IPs:");
                    foreach (string currentIP in config.BannedIPs)
                    {
                        SendMessage(sourceClientId, currentIP);
                    }
                    return true;
                case "-groups":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.list_groups))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of groups:");
                    foreach (GameModeFortress.Group currenGroup in serverClient.Groups)
                    {
                        SendMessage(sourceClientId, currenGroup.ToString());
                    }
                    return true;
                case "-saved_clients":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.list_saved_clients))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of saved clients:");
                    foreach (GameModeFortress.Client currenClient in serverClient.Clients)
                    {

                        SendMessage(sourceClientId, currenClient.ToString());
                    }
                    return true;
                default:
                    SendMessage(sourceClientId, "Invalid parameter.");
                    return false;
            }
        }

        public bool GiveAll(int sourceClientId, string target)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.giveall))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                string targetName = targetClient.playername;
                string sourcename = GetClient(sourceClientId).playername;
                for (int i = 0; i < d_Data.IsBuildable.Length; i++)
                {
                    if (!d_Data.IsBuildable[i])
                    {
                        continue;
                    }
                    Inventory inventory = GetPlayerInventory(targetName).Inventory;
                    InventoryUtil util = GetInventoryUtil(inventory);

                    for (int xx = 0; xx < util.CellCount.X; xx++)
                    {
                        for (int yy = 0; yy < util.CellCount.Y; yy++)
                        {
                            if (!inventory.Items.ContainsKey(new ProtoPoint(xx, yy)))
                            {
                                continue;
                            }
                            Item currentItem = inventory.Items[new ProtoPoint(xx, yy)];
                            if (currentItem != null
                                && currentItem.ItemClass == ItemClass.Block
                                && currentItem.BlockId == i)
                            {
                                currentItem.BlockCount = 999;
                                goto nextblock;
                            }
                        }
                    }
                    for (int xx = 0; xx < util.CellCount.X; xx++)
                    {
                        for (int yy = 0; yy < util.CellCount.Y; yy++)
                        {
                            Item newItem = new Item();
                            newItem.ItemClass = ItemClass.Block;
                            newItem.BlockId = i;
                            newItem.BlockCount = 999;

                            if (util.ItemAtCell(new Point(xx, yy)) == null)
                            {
                                inventory.Items[new ProtoPoint(xx, yy)] = newItem;
                                goto nextblock;
                            }
                        }
                    }
                nextblock:
                    targetClient.IsInventoryDirty = true;
                }
                ServerEventLog(string.Format("{0} gives all to {1}.", sourcename, targetName));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
            return false;
        }

        public bool Give(int sourceClientId, string target, string blockname, int amount)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.give))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                string targetName = targetClient.playername;
                string sourcename = GetClient(sourceClientId).playername;
                //int amount;
                if (amount < 0)
                {
                    return false;
                }
                if (amount > 9999)
                {
                    amount = 9999;
                }
                for (int i = 0; i < d_Data.IsBuildable.Length; i++)
                {
                    if (!d_Data.IsBuildable[i])
                    {
                        continue;
                    }
                    if (!d_Data.Name[i].Equals(blockname, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    Inventory inventory = GetPlayerInventory(targetName).Inventory;
                    InventoryUtil util = GetInventoryUtil(inventory);

                    for (int xx = 0; xx < util.CellCount.X; xx++)
                    {
                        for (int yy = 0; yy < util.CellCount.Y; yy++)
                        {
                            if (!inventory.Items.ContainsKey(new ProtoPoint(xx, yy)))
                            {
                                continue;
                            }
                            Item currentItem = inventory.Items[new ProtoPoint(xx, yy)];
                            if (currentItem != null
                                 && currentItem.ItemClass == ItemClass.Block
                                 && currentItem.BlockId == i)
                            {
                                if (amount == 0)
                                {
                                    inventory.Items[new ProtoPoint(xx, yy)] = null;
                                }
                                else
                                {
                                    currentItem.BlockCount = amount;
                                }
                                goto nextblock;
                            }
                        }
                    }
                    for (int xx = 0; xx < util.CellCount.X; xx++)
                    {
                        for (int yy = 0; yy < util.CellCount.Y; yy++)
                        {
                            Item newItem = new Item();
                            newItem.ItemClass = ItemClass.Block;
                            newItem.BlockId = i;
                            newItem.BlockCount = amount;

                            if (util.ItemAtCell(new Point(xx, yy)) == null)
                            {
                                inventory.Items[new ProtoPoint(xx, yy)] = newItem;
                                goto nextblock;
                            }
                        }
                    }
                 nextblock:
                    targetClient.IsInventoryDirty = true;
                }
                ServerEventLog(string.Format("{0} gives {1} {2} to {3}.", sourcename, amount, blockname, targetName));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
            return false;
        }

        public bool Monsters(int sourceClientId, string option)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.monsters))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            config.Monsters = option.Equals("off") ? false : true;
            SaveConfig();
            if (!config.Monsters)
            {
                foreach (var k in clients)
                {
                    SendPacket(k.Key, Serialize(new PacketServer()
                        {
                         PacketId = ServerPacketId.RemoveMonsters
                        }));
                }
            }
            SendMessage(sourceClientId, colorSuccess + "Monsters turned " + option);
            ServerEventLog(string.Format("{0} turns monsters {1}.", GetClient(sourceClientId).playername, option));
            return true;
        }

        public bool AreaAdd(int sourceClientId, int id, string coords, string[] permittedGroups, string[] permittedUsers, int? level)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.area_add))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            if (config.Areas.Find(v => v.Id == id) != null)
            {
                SendMessage(sourceClientId, string.Format("{0}Area ID already in use.", colorError));
                return false;
            }

            AreaConfig newArea = new AreaConfig(){Id = id, Coords = coords};
            if (permittedGroups != null)
            {
                for (int i = 0; i < permittedGroups.Length; i++)
                {
                    newArea.PermittedGroups.Add(permittedGroups[i]);
                }
            }
            if (permittedUsers != null)
            {
                for (int i = 0; i < permittedUsers.Length; i++)
                {
                    newArea.PermittedUsers.Add(permittedUsers[i]);
                }
            }
            if (level != null)
            {
                newArea.Level = level;
            }

            config.Areas.Add(newArea);
            SaveConfig();
            SendMessage(sourceClientId, string.Format("{0}New area added: {1}", colorSuccess, newArea.ToString()));
            ServerEventLog(string.Format("{0} adds area: {1}.", GetClient(sourceClientId), newArea.ToString()));
            return true;
        }

        public bool AreaDelete(int sourceClientId, int id)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.area_delete))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            AreaConfig targetArea = config.Areas.Find(v => v.Id == id);
            if(targetArea == null)
            {
                SendMessage(sourceClientId, string.Format("{0}Area does not exist.", colorError));
                return false;
            }
            config.Areas.Remove(targetArea);
            SaveConfig();
            SendMessage(sourceClientId, string.Format("{0}Area deleted.", colorSuccess));
            ServerEventLog(string.Format("{0} deletes area: {1}.", GetClient(sourceClientId).playername, id));
            return true;
        }

        public bool Announcement(int sourceClientId, string message)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.announcement))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            ServerEventLog(String.Format("{0} announced: {1}.", GetClient(sourceClientId).playername, message));
            SendMessageToAll(string.Format("{0}Announcement: {1}", colorError, message));
            return true;
        }

        public bool ClearInterpreter(int sourceClientId)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.run))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            GetClient(sourceClientId).Interpreter = null;
            SendMessage(sourceClientId, "Interpreter cleared.");
            return true;
        }

        public bool SetSpawnPosition(int sourceClientId, string targetType, string target, int x, int y, int? z)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.set_spawn))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            // validate spawn coordinates
            int rZ = 0;
            if (z == null)
            {
                if (!MapUtil.IsValidPos(d_Map, x, y))
                {
                    SendMessage(sourceClientId, string.Format("{0}Invalid spawn coordinates.", colorError));
                    return false;
                }
                rZ = MapUtil.blockheight(d_Map, 0, x, y);
            }
            else
            {
                rZ = z.Value;
            }
            if (!MapUtil.IsValidPos(d_Map, x, y, rZ))
            {
                SendMessage(sourceClientId, string.Format("{0}Invalid spawn coordinates.", colorError));
                return false;
            }

            switch (targetType)
            {
                case "-default":
                case "-d":
                    serverClient.DefaultSpawn = new GameModeFortress.Spawn() {x = x, y = y, z = z};
                    SaveServerClient();
                    // Inform related players.
                    bool hasEntry = false;
                    foreach (var k in clients)
                    {
                        hasEntry = false;
                        if (k.Value.clientGroup.Spawn != null)
                        {
                            hasEntry = true;
                        }
                        else
                        {
                            foreach (GameModeFortress.Client client in serverClient.Clients)
                            {
                                if (client.Name.Equals(k.Value.playername, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (client.Spawn != null)
                                    {
                                        hasEntry = true;
                                    }
                                    break;
                                }
                            }
                        }
                        if (!hasEntry)
                        {
                            this.SendPlayerSpawnPosition(k.Key, x, y, rZ);
                        }
                    }
                    SendMessage(sourceClientId, string.Format("{0}Default spawn position set to {1},{2},{3}.", colorSuccess, x, y, rZ));
                    ServerEventLog(String.Format("{0} sets default spawn to {1},{2}{3}.", GetClient(sourceClientId).playername, x, y, z == null ? "" : "," + z.Value));
                    return true;
                case "-group":
                case "-g":
                    // Check if group even exists.
                    GameModeFortress.Group targetGroup = serverClient.Groups.Find(
                        delegate(GameModeFortress.Group grp)
                    {
                        return grp.Name.Equals(target,StringComparison.InvariantCultureIgnoreCase);
                    }
                    );
                    if (targetGroup == null)
                    {
                        SendMessage(sourceClientId, string.Format("{0}Group {1} not found.", colorError, target));
                        return false;
                    }
                    targetGroup.Spawn = new GameModeFortress.Spawn()
                    {
                        x = x,
                        y = y,
                        z = z,
                    };
                    SaveServerClient();
                    // Inform related players.
                    hasEntry = false;
                    foreach (var k in clients)
                    {
                        if (k.Value.clientGroup.Name.Equals(targetGroup.Name))
                        {
                            // Inform only if there is no spawn set under clients.
                            foreach (GameModeFortress.Client client in serverClient.Clients)
                            {
                                if (client.Name.Equals(k.Value.playername, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (client.Spawn != null)
                                    {
                                        hasEntry = true;
                                    }
                                    break;
                                }
                            }
                            if (!hasEntry)
                            {
                                this.SendPlayerSpawnPosition(k.Key, x, y, rZ);
                            }
                        }
                    }
                    SendMessage(sourceClientId, string.Format("{0}Spawn position of group {1} set to {2},{3},{4}.", colorSuccess, targetGroup.Name, x, y, rZ));
                    ServerEventLog(String.Format("{0} sets spawn of group {1} to {2},{3}{4}.", GetClient(sourceClientId).playername, targetGroup.Name, x, y, z == null ? "" : "," + z.Value));
                    return true;
                case "-player":
                case "-p":
                    // Get related client.
                    Client targetClient = this.GetClient(target);
                    int? targetClientId = null;
                    if(targetClient != null)
                    {
                        targetClientId = targetClient.Id;
                    }
                    string targetClientPlayername = targetClient == null ? target : targetClient.playername;

                    GameModeFortress.Client clientEntry = serverClient.Clients.Find(
                        delegate(GameModeFortress.Client client)
                        {
                            return client.Name.Equals(targetClientPlayername, StringComparison.InvariantCultureIgnoreCase);
                        }
                    );
                    if (clientEntry == null)
                    {
                        SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
                        return false;
                    }
                    // Change or add spawn entry of client.
                    clientEntry.Spawn = new GameModeFortress.Spawn()
                    {
                        x = x,
                        y = y,
                        z = z,
                    };
                    SaveServerClient();
                    // Inform player if he's online.
                    if (targetClientId != null)
                    {
                        this.SendPlayerSpawnPosition(targetClientId.Value, x, y, rZ);
                    }
                    SendMessage(sourceClientId, string.Format("{0}Spawn position of player {1} set to {2},{3},{4}.", colorSuccess, targetClientPlayername, x, y, rZ));
                    ServerEventLog(String.Format("{0} sets spawn of player {1} to {2},{3}{4}.", GetClient(sourceClientId).playername, targetClientPlayername, x, y, z == null ? "" : "," + z.Value));
                    return true;
                default:
                    SendMessage(sourceClientId, "Invalid type.");
                    return false;
            }
        }


        private bool PrivilegeAdd(int sourceClientId, string target, string privilege)
        {
            try
            {
                ServerClientMisc.Privilege newPrivilege = (ServerClientMisc.Privilege) Enum.Parse(typeof(ServerClientMisc.Privilege), privilege);

                if (Enum.IsDefined(typeof(ServerClientMisc.Privilege), newPrivilege) | newPrivilege.ToString().Contains(","))
                {
                    return this.PrivilegeAdd(sourceClientId, target, newPrivilege);
                }
                SendMessage(sourceClientId, string.Format("{0}Privilege {1} does not exist.", colorError, privilege));
                return false;
            }
            catch (ArgumentException)
            {
                SendMessage(sourceClientId, string.Format("{0}Privilege {1} does not exist.", colorError, privilege));
                return false;
            }
        }
        public bool PrivilegeAdd(int sourceClientId, string target, ServerClientMisc.Privilege privilege)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.privilege_add))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                if(targetClient.privileges.Contains(privilege))
                {
                    SendMessage(sourceClientId, string.Format("{0}Player {1} already has privilege {2}.", colorError, target, privilege.ToString()));
                    return false;
                }
                targetClient.privileges.Add(privilege);
                SendMessageToAll(string.Format("{0}New privilege for {1}: {2}", colorSuccess, targetClient.ColoredPlayername(colorSuccess), privilege.ToString()));
                ServerEventLog(string.Format("{0} gives {1} privilege {2}.", GetClient(sourceClientId).playername, targetClient.playername, privilege.ToString()));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} does not exist.", colorError, target));
            return false;
        }

        private bool PrivilegeRemove(int sourceClientId, string target, string privilege)
        {
            try
            {
                ServerClientMisc.Privilege newPrivilege = (ServerClientMisc.Privilege) Enum.Parse(typeof(ServerClientMisc.Privilege), privilege);

                if (Enum.IsDefined(typeof(ServerClientMisc.Privilege), newPrivilege) | newPrivilege.ToString().Contains(","))
                {
                    return this.PrivilegeRemove(sourceClientId, target, newPrivilege);
                }
                SendMessage(sourceClientId, string.Format("{0}Privilege {1} does not exist.", colorError, privilege));
                return false;
            }
            catch (ArgumentException)
            {
                SendMessage(sourceClientId, string.Format("{0}Privilege {1} does not exist.", colorError, privilege));
                return false;
            }
        }
        public bool PrivilegeRemove(int sourceClientId, string target, ServerClientMisc.Privilege privilege)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.privilege_remove))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                if(!targetClient.privileges.Remove(privilege))
                {
                    SendMessage(sourceClientId, string.Format("{0}Player {1} don't has privilege {2}.", colorError, target, privilege.ToString()));
                    return false;
                }

                SendMessageToAll(string.Format("{0} {1} lost privilege: {2}", colorImportant, targetClient.ColoredPlayername(colorImportant), privilege.ToString()));
                ServerEventLog(string.Format("{0} removes {1} privilege {2}.", GetClient(sourceClientId).playername, targetClient.playername, privilege.ToString()));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} does not exist.", colorError, target));
            return false;
        }

        public bool RestartServer(int sourceClientId)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.restart))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            SendMessageToAll(string.Format("{0}{1} restarted server.", colorImportant, GetClient(sourceClientId).ColoredPlayername(colorImportant)));
            ServerEventLog(string.Format("{0} restarts server.", GetClient(sourceClientId).playername));
            Exit();
            return true;
        }

        public bool TeleportPlayer(int sourceClientId, string target, int x, int y, int? z)
        {
            if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.teleport_player))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            // validate target position
            int rZ = 0;
            if (z == null)
            {
                if (!MapUtil.IsValidPos(d_Map, x, y))
                {
                    SendMessage(sourceClientId, string.Format("{0}Invalid coordinates.", colorError));
                    return false;
                }
                rZ = MapUtil.blockheight(d_Map, 0, x, y);
            }
            else
            {
                rZ = z.Value;
            }
            if (!MapUtil.IsValidPos(d_Map, x, y, rZ))
            {
                SendMessage(sourceClientId, string.Format("{0}Invalid coordinates.", colorError));
                return false;
            }
            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                SendPlayerTeleport(targetClient.Id, targetClient.Id, x * chunksize, rZ * chunksize, y * chunksize , (byte)targetClient.positionheading, (byte)targetClient.positionpitch);
                SendMessage(targetClient.Id, string.Format("{0}You have been teleported to ({1},{2},{3}) by {4}.", colorImportant, x, y, rZ, GetClient(sourceClientId).ColoredPlayername(colorImportant)));
                SendMessage(sourceClientId, string.Format("{0}You teleported {1} to ({2},{3},{4}).", colorSuccess, targetClient.ColoredPlayername(colorSuccess), x, y, rZ));
                ServerEventLog(string.Format("{0} teleports {1} to {2} {3} {4}.", GetClient(sourceClientId).playername, targetClient.playername, x, y, rZ));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} does not exist.", colorError, target));
            return false;
        }
    }
}