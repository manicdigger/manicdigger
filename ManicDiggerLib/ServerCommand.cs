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
                case "re":
                    if (!string.IsNullOrEmpty(argument))
                    {
                        this.AnswerMessage(sourceClientId, argument);
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "You can't send an empty message.");
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
                case "op_offline":
                case "chgrp_offline":
                case "cg_offline":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 2)
                    {
                        this.ChangeGroupOffline(sourceClientId, ss[0], ss[1]);
                        return;
                    }
                    SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                    return;
                case "remove_client":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length == 1)
                    {
                        this.RemoveClientFromConfig(sourceClientId, ss[0]);
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
                case "ban_offline":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length >= 2)
                    {
                        this.BanOffline(sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                        return;
                    }
                    this.BanOffline(sourceClientId, argument);
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
                case "set_home":
                    // When no coordinates are given, set spawn to players current position.
                    if (string.IsNullOrEmpty(argument))
                    {
                        this.SetSpawnPosition(sourceClientId,
                                          (int) GetClient(sourceClientId).PositionMul32GlX / 32,
                                          (int) GetClient(sourceClientId).PositionMul32GlZ / 32,
                                          (int) GetClient(sourceClientId).PositionMul32GlY / 32);
                        return;
                    }
                    //            0 1 2
                    // agrument:  x y z
                    ss = argument.Split(new[] { ' ' });

                    if (ss.Length < 2 || ss.Length > 3)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    try
                    {
                        x = Convert.ToInt32(ss[0]);
                        y = Convert.ToInt32(ss[1]);
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
                        z = Convert.ToInt32(ss[2]);
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
                    this.SetSpawnPosition(sourceClientId, x, y, z);
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
                case "mods":
                    this.RestartMods(sourceClientId);
                    break;
                //case "crashserver": for (; ; ) ;
                case "stats":
                    double seconds = (DateTime.UtcNow - statsupdate).TotalSeconds;
                    SendMessage(sourceClientId, "Packets/s:" + decimal.Round((decimal)(StatTotalPackets / seconds), 2, MidpointRounding.AwayFromZero));
                    SendMessage(sourceClientId, "Total KBytes/s:" + decimal.Round((decimal)(StatTotalPacketsLength / seconds / 1024), 2, MidpointRounding.AwayFromZero));
                    break;
                case "tp":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length != 1)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    foreach (var k in clients)
                    {
                        if (k.Value.playername.Equals(ss[0], StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.TeleportToPlayer(sourceClientId, k.Key);
                            return;
                        }
                    }
                    foreach (var k in clients)
                    {
                        if (k.Value.playername.StartsWith(ss[0], StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.TeleportToPlayer(sourceClientId, k.Key);
                            return;
                        }
                    }
                    SendMessage(sourceClientId, string.Format("{0}Player {1} does not exist.", colorError, ss[0]));
                    break;
                case "tp_pos":
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length < 2 || ss.Length > 3)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }

                    try
                    {
                        x = Convert.ToInt32(ss[0]);
                        y = Convert.ToInt32(ss[1]);
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
                        z = Convert.ToInt32(ss[2]);
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
                    this.TeleportToPosition(sourceClientId, x, y, z);
                    break;
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
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.backup_database))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        break;
                    }
                    if (!BackupDatabase(argument))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Backup could not be created. Check filename.", colorError));
                    }
                    else
                    {
                        SendMessage(sourceClientId, string.Format("{0}Backup created.", colorSuccess));
                        ServerEventLog(String.Format("{0} backups database: {1}.", GetClient(sourceClientId).playername, argument));
                    }
                    break;
                    /*
                case "load":
                    if (!GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.load))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        break;
                    }
                    if (!GameStorePath.IsValidName(argument))
                    {
                        SendMessage(sourceClientId, string.Format("Invalid load filename: {0}", argument));
                        break;
                    }
                    if (!LoadDatabase(argument))
                    {
                        SendMessage(sourceClientId, string.Format("{0}World could not be loaded. Check filename.", colorError));
                    }
                    else
                    {
                        SendMessage(sourceClientId, string.Format("{0}World loaded.", colorSuccess));
                        ServerEventLog(String.Format("{0} loads world: {1}.", GetClient(sourceClientId).playername, argument));
                    }
                    break;
                    */
                case "reset_inventory":
                    this.ResetInventory(sourceClientId, argument);
                    return;
                case "fill_limit":
                    //           0    1      2
                    // agrument: type target maxFill
                    ss = argument.Split(new[] { ' ' });
                    if (ss.Length < 2 || ss.Length > 3)
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    // Add an empty target argument, when user sets default max-fill.
                    if (ss[0].Equals("-d") || ss[0].Equals("-default"))
                    {
                        string[] ssTemp = new string[ss.Length + 1];
                        ssTemp[0] = ss[0];
                        ssTemp[1] = "";
                        Array.Copy(ss, 1, ssTemp, 2, ss.Length - 1);
                        ss = ssTemp;
                    }
                    int maxFill;
                    if (!Int32.TryParse(ss[2], out maxFill))
                    {
                        SendMessage(sourceClientId, colorError + "Invalid arguments. Type /help to see command's usage.");
                        return;
                    }
                    else
                    {
                        this.SetFillAreaLimit(sourceClientId, ss[0], ss[1], maxFill);
                    }
                    return;
                default:
                    for (int i = 0; i < modEventHandlers.oncommand.Count; i++)
                    {
                        try
                        {
                            if (modEventHandlers.oncommand[i](sourceClientId, command, argument))
                            {
                                return;
                            }
                        }
                        catch
                        {
                            SendMessage(sourceClientId, "Command exception.");
                        }
                    }
                    SendMessage(sourceClientId, colorError + "Unknown command /" + command);
                    return;
            }
        }

        public void Help(int sourceClientId)
        {
            SendMessage(sourceClientId, colorHelp + "Available privileges:");
            foreach (string privilege in GetClient(sourceClientId).privileges)
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
                case "ban_offline":
                    return "/ban_offline [username] {reason}";
                case "unban":
                    return "/unban [-p playername | -ip ipaddress]";
                case "run":
                    return "/run [JavaScript (max. length 4096 char.)]";
                case "op":
                    return "/op [username] [group]";
                case "chgrp":
                    return "/chgrp [username] [group]";
                case "op_offline":
                    return "/op_offline [username] [group]";
                case "chgrp_offline":
                    return "/chgrp_offline [username] [group]";
                case "remove_client":
                    return "/remove_client [username]";
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
                case "set_home":
                    return "/set_home {[x] [y] {z}}";
                case "privilege_add":
                    return "/privilege_add [username] [privilege]";
                case "privilege_remove":
                    return "/privilege_remove [username] [privilege]";
                case "restart":
                    return "/restart";
                case "teleport_player":
                    return "/teleport_player [target] [x] [y] {z}";
                case "tp":
                    return "/tp [username]";
                case "tp_pos":
                    return "/tp_pos [x] [y] {z}";
                case "backup_database":
                    return "/backup_database [filename]";
                case "reset_inventory":
                    return "/reset_inventory [target]";
                case "fill_limit":
                    return "/fill_limit [-default|-group|-player] [limit]";
                default:
                    if (commandhelps.ContainsKey(command))
                    {
                        return commandhelps[command];
                    }
                    return "No description available.";
            }
        }
        public Dictionary<string, string> commandhelps = new Dictionary<string, string>();
        public Dictionary<string, string> lastSender = new Dictionary<string, string>();

        public bool PrivateMessage(int sourceClientId, string recipient, string message)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.pm))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            Client targetClient = GetClient(recipient);
            Client sourceClient = GetClient(sourceClientId);
            if (targetClient != null)
            {
                SendMessage(targetClient.Id, string.Format("PM {0}: {1}", sourceClient.ColoredPlayername(colorNormal), message));
                SendMessage(sourceClientId, string.Format("PM -> {0}: {1}", targetClient.ColoredPlayername(colorNormal), message));
                lastSender[targetClient.playername] = sourceClient.playername;
                // TODO: move message sound to client
                //SendSound(k.Key, "message.wav");
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, recipient));
            return false;
        }

        public bool AnswerMessage(int sourceClientId, string message)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.pm))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            
            Client sourceClient = GetClient(sourceClientId);
            if (!lastSender.ContainsKey(sourceClient.playername))
            {
                SendMessage(sourceClientId, string.Format("{0}Nobody sent you a PM yet.", colorError));
                return false;
            }

            Client targetClient = GetClient(lastSender[sourceClient.playername]);
            if (targetClient != null)
            {
                SendMessage(targetClient.Id, string.Format("PM {0}: {1}", sourceClient.ColoredPlayername(colorNormal), message));
                SendMessage(sourceClientId, string.Format("PM -> {0}: {1}", targetClient.ColoredPlayername(colorNormal), message));
                lastSender[targetClient.playername] = sourceClient.playername;
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, lastSender[sourceClient.playername]));
            return false;
        }

        public bool ChangeGroup(int sourceClientId, string target, string newGroupName)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.chgrp))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            // Get related group from config file.
            ManicDigger.Group newGroup = serverClient.Groups.Find(
                delegate(ManicDigger.Group grp)
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
            ManicDigger.Client clientConfig = serverClient.Clients.Find(
                delegate(ManicDigger.Client client)
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
                    clientConfig = new ManicDigger.Client();
                    clientConfig.Name = targetClient.playername;
                    clientConfig.Group = newGroup.Name;
                    serverClient.Clients.Add(clientConfig);
                }
                else
                {
                    clientConfig.Group = newGroup.Name;
                }
                SaveServerClient();
                SendMessageToAll(string.Format("{0}{1} set group of {2} to {3}.", colorSuccess, GetClient(sourceClientId).ColoredPlayername(colorSuccess), targetClient.ColoredPlayername(colorSuccess), newGroup.GroupColorString() + newGroupName));
                ServerEventLog(String.Format("{0} sets group of {1} to {2}.", GetClient(sourceClientId).playername, targetClient.playername, newGroupName));
                targetClient.AssignGroup(newGroup);
                SendFreemoveState(targetClient.Id, targetClient.privileges.Contains(ServerClientMisc.Privilege.freemove));
                SetFillAreaLimit(targetClient.Id);
                return true;
            }

            // Target is at the moment not online.
            SendMessage(sourceClientId, string.Format("{0}Player {1} is offline. Use /chgrp_offline command.", colorError, target));
            return false;
        }

        public bool ChangeGroupOffline(int sourceClientId, string target, string newGroupName)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.chgrp_offline))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            // Get related group from config file.
            ManicDigger.Group newGroup = serverClient.Groups.Find(
                delegate(ManicDigger.Group grp)
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

            // Get related client from config file.
            ManicDigger.Client clientConfig = serverClient.Clients.Find(
                delegate(ManicDigger.Client client)
                {
                    return client.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase);
                }
            );

            // Get related client.
            Client targetClient = GetClient(target);

            if (targetClient != null)
            {
                SendMessage(sourceClientId, string.Format("{0}Player {1} is online. Use /chgrp command.", colorError, target));
                return false;
            }

            // Target is at the moment not online. Create or change a entry in ServerClient.
            if (clientConfig == null)
            {
                clientConfig = new ManicDigger.Client();
                clientConfig.Name = target;
                clientConfig.Group = newGroup.Name;
                serverClient.Clients.Add(clientConfig);
            }
            else
            {
                // Check if target's current group is superior.
                ManicDigger.Group oldGroup = serverClient.Groups.Find(
                    delegate(ManicDigger.Group grp)
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
            SendMessageToAll(string.Format("{0}{1} set group of {2} to {3} (offline).", colorSuccess, GetClient(sourceClientId).ColoredPlayername(colorSuccess), target, newGroup.GroupColorString() + newGroupName));
            ServerEventLog(String.Format("{0} sets group of {1} to {2} (offline).", GetClient(sourceClientId).playername, target, newGroupName));
            return true;
        }

        public bool RemoveClientFromConfig(int sourceClientId, string target)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.remove_client))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            // Get related client from config file
            ManicDigger.Client targetClient = serverClient.Clients.Find(
                delegate(ManicDigger.Client client)
                {
                    return client.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase);
                }
            );
            // Entry exists.
            if (targetClient != null)
            {
                // Get target's group.
                ManicDigger.Group targetGroup = serverClient.Groups.Find(
                    delegate(ManicDigger.Group grp)
                {
                    return grp.Name.Equals(targetClient.Group);
                }
                );
                if (targetGroup == null)
                {
                    SendMessage(sourceClientId, string.Format("{0}Invalid group.", colorError));
                    return false;
                }
                // Check if target's group is superior.
                if (targetGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || targetGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target user is superior or equal.", colorError));
                    return false;
                }
                // Remove target's entry.
                serverClient.Clients.Remove(targetClient);
                this.SaveServerClient();
                // If client is online, change his group
                if(GetClient(target) != null)
                {
                    GetClient(target).AssignGroup(this.defaultGroupGuest);
                    SendMessageToAll(string.Format("{0}{1} set group of {2} to {3}.", colorSuccess, GetClient(sourceClientId).ColoredPlayername(colorSuccess), GetClient(target).ColoredPlayername(colorSuccess), this.defaultGroupGuest.GroupColorString() + defaultGroupGuest.Name));
                }
                SendMessage(sourceClientId, string.Format("{0}Client {1} removed from config.", colorSuccess, target));
                ServerEventLog(string.Format("{0} removes client {1} from config.", GetClient(sourceClientId).playername, target));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}No entry of client {1} found.", colorError, target));
            return false;
        }

        public bool Login (int sourceClientId, string targetGroupString, string password)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.login))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            ManicDigger.Group targetGroup = serverClient.Groups.Find(
                delegate(ManicDigger.Group grp)
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
                SendFreemoveState(sourceClientId, GetClient(sourceClientId).privileges.Contains(ServerClientMisc.Privilege.freemove));
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.welcome))
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.logging))
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.kick))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason + ".";
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
                SendMessageToAll(string.Format("{0}{1} was kicked by {2}.{3}", colorImportant, targetNameColored, sourceNameColored, reason));
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.ban))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason + ".";
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
                banlist.BannedUsers.Add(targetName);
                SaveConfig();
                SendMessageToAll(string.Format("{0}{1} was banned by {2}.{3}", colorImportant, targetNameColored, sourceNameColored, reason));
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.banip))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason + ".";
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
                banlist.BannedIPs.Add(((IPEndPoint)targetClient.socket.RemoteEndPoint).Address.ToString());
                SaveConfig();
                SendMessageToAll(string.Format("{0}{1} was IP banned by {2}.{3}", colorImportant, targetNameColored, sourceNameColored, reason));
                ServerEventLog(string.Format("{0} IP bans {1}.{2}", sourceName, targetName, reason));
                SendDisconnectPlayer(targetClientId, string.Format("You were IP banned by an administrator.{0}", reason));
                KillPlayer(targetClientId);
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player ID {1} does not exist.", colorError, targetClientId));
            return false;
        }

        public bool BanOffline(int sourceClientId, string target)
        {
            return this.BanOffline(sourceClientId, target, "");
        }
        public bool BanOffline(int sourceClientId, string target, string reason)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.ban_offline))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            if (!reason.Equals(""))
            {
                reason = " Reason: " + reason;
            }

            if( GetClient(target) != null)
            {
                SendMessage(sourceClientId, string.Format("{0}Player {1} is online. Use /ban command.", colorError, target));
                return false;
            }

            // Target is at the moment not online. Check if there is an entry in ServerClient

            // Get related client from config file
            ManicDigger.Client targetClient = serverClient.Clients.Find(
                delegate(ManicDigger.Client client)
                {
                    return client.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase);
                }
            );

            // Entry exists.
            if (targetClient != null)
            {
                // Get target's group.
                ManicDigger.Group targetGroup = serverClient.Groups.Find(
                    delegate(ManicDigger.Group grp)
                {
                    return grp.Name.Equals(targetClient.Group);
                }
                );
                if (targetGroup == null)
                {
                    SendMessage(sourceClientId, string.Format("{0}Invalid group.", colorError));
                    return false;
                }

                // Check if target's group is superior.
                if (targetGroup.IsSuperior(GetClient(sourceClientId).clientGroup) || targetGroup.EqualLevel(GetClient(sourceClientId).clientGroup))
                {
                    SendMessage(sourceClientId, string.Format("{0}Target user is superior or equal.", colorError));
                    return false;
                }

                // Remove target's entry.
                serverClient.Clients.Remove(targetClient);
                this.SaveServerClient();
            }

            // Finally ban user.
            banlist.BannedUsers.Add(target);
            SaveConfig();
            SendMessageToAll(string.Format("{0}{1} (offline) was banned by {2}.{3}", colorImportant, target, GetClient(sourceClientId).ColoredPlayername(colorImportant), reason));
            ServerEventLog(string.Format("{0} bans {1}.{2}", GetClient(sourceClientId).playername, target, reason));
            return true;
        }

        public bool Unban(int sourceClientId, string type, string target)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.unban))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            // unban a playername
            if (type.Equals("-p"))
            {
                // case insensitive
                bool exists = banlist.UnbanPlayer(target);
                SaveBanlist();
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
                bool exists = banlist.UnbanIP(target);
                SaveBanlist();
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
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_clients))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Players:");
                    foreach (var k in clients)
                    {
                        // Format: Key Playername IP
                        SendMessage(sourceClientId, string.Format("[{0}] {1} {2}", k.Key, k.Value.ColoredPlayername(colorNormal), ((IPEndPoint)k.Value.socket.RemoteEndPoint).Address.ToString()));
                    }
                    return true;
                case "-clients2":
                case "-c2":
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_clients))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Players:");
                    foreach (var k in clients)
                    {
                        // Format: Key Playername:Group:Privileges IP
                        SendMessage(sourceClientId, string.Format("[{0}] {1}", k.Key, k.Value.ToString()));
                    }
                    return true;
                case "-areas":
                case "-a":
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_areas))
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
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_banned_users))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Banned Users:");
                    foreach (string currentUser in banlist.BannedUsers)
                    {
                        SendMessage(sourceClientId, currentUser);
                    }
                    return true;
                case "-bannedips":
                case "-bip":
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_banned_users))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of Banned IPs:");
                    foreach (string currentIP in banlist.BannedIPs)
                    {
                        SendMessage(sourceClientId, currentIP);
                    }
                    return true;
                case "-groups":
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_groups))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of groups:");
                    foreach (ManicDigger.Group currenGroup in serverClient.Groups)
                    {
                        SendMessage(sourceClientId, currenGroup.ToString());
                    }
                    return true;
                case "-saved_clients":
                    if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.list_saved_clients))
                    {
                        SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                        return false;
                    }
                    SendMessage(sourceClientId, colorImportant + "List of saved clients:");
                    foreach (ManicDigger.Client currenClient in serverClient.Clients)
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.giveall))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                string targetName = targetClient.playername;
                string sourcename = GetClient(sourceClientId).playername;
                for (int i = 0; i < BlockTypes.Length; i++)
                {
                    if (!BlockTypes[i].IsBuildable)
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.give))
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
                for (int i = 0; i < BlockTypes.Length; i++)
                {
                    if (!BlockTypes[i].IsBuildable)
                    {
                        continue;
                    }
                    if (!BlockTypes[i].Name.Equals(blockname, StringComparison.InvariantCultureIgnoreCase))
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

        public bool ResetInventory(int sourceClientId, string target)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.reset_inventory))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            Client targetClient = GetClient(target);
            if(targetClient != null)
            {
                ResetPlayerInventory(targetClient.playername);
                SendMessageToAll(string.Format("{0}{1}reset inventory of {2}.", colorImportant, GetClient(sourceClientId).ColoredPlayername(colorImportant), targetClient.ColoredPlayername(colorImportant)));
                ServerEventLog(string.Format("{0} resets inventory of {1}.", GetClient(sourceClientId).playername, targetClient.playername));
                return true;
            }
            // Player is not online.
            if (Inventory != null && Inventory.ContainsKey(target))
            {
                Inventory.Remove(target);
                SendMessageToAll(string.Format("{0}{1}reset inventory of {2} (offline).", colorImportant, GetClient(sourceClientId).ColoredPlayername(colorImportant)));
                ServerEventLog(string.Format("{0} resets inventory of {1} (offline).", GetClient(sourceClientId).playername, target));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} not found.", colorError, target));
            return false;
        }

        public bool Monsters(int sourceClientId, string option)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.monsters))
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
                    SendPacket(k.Key, Serialize(new Packet_Server()
                        {
                            Id = Packet_ServerIdEnum.RemoveMonsters
                        }));
                }
            }
            SendMessageToAll(string.Format("{0} turned monsters {1}.", GetClient(sourceClientId).ColoredPlayername(colorSuccess), option));
            ServerEventLog(string.Format("{0} turns monsters {1}.", GetClient(sourceClientId).playername, option));
            return true;
        }

        public bool AreaAdd(int sourceClientId, int id, string coords, string[] permittedGroups, string[] permittedUsers, int? level)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.area_add))
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.area_delete))
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.announcement))
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.run))
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.set_spawn))
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
                    serverClient.DefaultSpawn = new ManicDigger.Spawn() {x = x, y = y, z = z};
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
                            foreach (ManicDigger.Client client in serverClient.Clients)
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
                    ManicDigger.Group targetGroup = serverClient.Groups.Find(
                        delegate(ManicDigger.Group grp)
                    {
                        return grp.Name.Equals(target,StringComparison.InvariantCultureIgnoreCase);
                    }
                    );
                    if (targetGroup == null)
                    {
                        SendMessage(sourceClientId, string.Format("{0}Group {1} not found.", colorError, target));
                        return false;
                    }
                    targetGroup.Spawn = new ManicDigger.Spawn()
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
                            foreach (ManicDigger.Client client in serverClient.Clients)
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

                    ManicDigger.Client clientEntry = serverClient.Clients.Find(
                        delegate(ManicDigger.Client client)
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
                    clientEntry.Spawn = new ManicDigger.Spawn()
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

        public bool SetSpawnPosition(int sourceClientId, int x, int y, int? z)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.set_home))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            Console.WriteLine(x + " " + y + " " + z);

            // Validate spawn position.
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

            // Get related client entry.
            ManicDigger.Client clientEntry = serverClient.Clients.Find(
                delegate(ManicDigger.Client client)
                {
                    return client.Name.Equals(GetClient(sourceClientId).playername, StringComparison.InvariantCultureIgnoreCase);
                }
            );
            // TODO: When guests have "set_home" privilege, count of client entries can quickly grow.
            if (clientEntry == null)
            {
                clientEntry = new ManicDigger.Client();
                clientEntry.Name = GetClient(sourceClientId).playername;
                clientEntry.Group = GetClient(sourceClientId).clientGroup.Name;
                serverClient.Clients.Add(clientEntry);
            }
            // Change or add spawn entry of client.
            clientEntry.Spawn = new ManicDigger.Spawn()
            {
                x = x,
                y = y,
                z = z,
            };
            SaveServerClient();
            // Send player new spawn position.
            this.SendPlayerSpawnPosition(sourceClientId, x, y, rZ);
            return true;
        }

        public bool PrivilegeAdd(int sourceClientId, string target, string privilege)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.privilege_add))
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
                if (privilege.Equals(ServerClientMisc.Privilege.freemove))
                {
                    SendFreemoveState(targetClient.Id, targetClient.privileges.Contains(ServerClientMisc.Privilege.freemove));
                }
                SendMessageToAll(string.Format("{0}New privilege for {1}: {2}", colorSuccess, targetClient.ColoredPlayername(colorSuccess), privilege.ToString()));
                ServerEventLog(string.Format("{0} gives {1} privilege {2}.", GetClient(sourceClientId).playername, targetClient.playername, privilege.ToString()));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} does not exist.", colorError, target));
            return false;
        }

        public bool PrivilegeRemove(int sourceClientId, string target, string privilege)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.privilege_remove))
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
                if (privilege.Equals(ServerClientMisc.Privilege.freemove))
                {
                    SendFreemoveState(targetClient.Id, targetClient.privileges.Contains(ServerClientMisc.Privilege.freemove));
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
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.restart))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            SendMessageToAll(string.Format("{0}{1} restarted server.", colorImportant, GetClient(sourceClientId).ColoredPlayername(colorImportant)));
            ServerEventLog(string.Format("{0} restarts server.", GetClient(sourceClientId).playername));
            Exit();
            return true;
        }

        public bool RestartMods(int sourceClientId)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.restart))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            SendMessageToAll(string.Format("{0}{1} restarted mods.", colorImportant, GetClient(sourceClientId).ColoredPlayername(colorImportant)));
            ServerEventLog(string.Format("{0} restarts mods.", GetClient(sourceClientId).playername));
            
            modEventHandlers = new ModEventHandlers();
            foreach (var m in httpModules)
            {
                httpServer.Uninstall(m.module);
            }
            httpModules.Clear();

            LoadMods(true);
            return true;
        }

        public bool TeleportToPlayer(int sourceClientId, int clientTo)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.tp))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }
            Client t = clients[clientTo];
            SendPlayerTeleport(sourceClientId, sourceClientId, t.PositionMul32GlX,
                t.PositionMul32GlY, t.PositionMul32GlZ, (byte)t.positionheading, (byte)t.positionpitch, t.stance);
            return true;
        }

        public bool TeleportToPosition(int sourceClientId, int x, int y, int? z)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.tp_pos))
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

            Client client = GetClient(sourceClientId);
            SendPlayerTeleport(client.Id, client.Id, x * chunksize, rZ * chunksize, y * chunksize , (byte)client.positionheading, (byte)client.positionpitch, client.stance);
            SendMessage(client.Id, string.Format("{0}New Position ({1},{2},{3}).", colorSuccess, x, y, rZ));
            return true;
        }

        public bool TeleportPlayer(int sourceClientId, string target, int x, int y, int? z)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.teleport_player))
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
                SendPlayerTeleport(targetClient.Id, targetClient.Id, x * chunksize, rZ * chunksize, y * chunksize , (byte)targetClient.positionheading, (byte)targetClient.positionpitch, targetClient.stance);
                SendMessage(targetClient.Id, string.Format("{0}You have been teleported to ({1},{2},{3}) by {4}.", colorImportant, x, y, rZ, GetClient(sourceClientId).ColoredPlayername(colorImportant)));
                SendMessage(sourceClientId, string.Format("{0}You teleported {1} to ({2},{3},{4}).", colorSuccess, targetClient.ColoredPlayername(colorSuccess), x, y, rZ));
                ServerEventLog(string.Format("{0} teleports {1} to {2} {3} {4}.", GetClient(sourceClientId).playername, targetClient.playername, x, y, rZ));
                return true;
            }
            SendMessage(sourceClientId, string.Format("{0}Player {1} does not exist.", colorError, target));
            return false;
        }

        public bool SetFillAreaLimit(int sourceClientId, string targetType, string target, int maxFill)
        {
            if (!PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.fill_limit))
            {
                SendMessage(sourceClientId, string.Format("{0}Insufficient privileges to access this command.", colorError));
                return false;
            }

            switch (targetType)
            {
                case "-default":
                case "-d":
                    serverClient.DefaultFillLimit = maxFill;
                    SaveServerClient();
                    // Inform related players.
                    bool hasEntry = false;
                    foreach (var k in clients)
                    {
                        hasEntry = false;
                        if (k.Value.clientGroup.FillLimit != null)
                        {
                            hasEntry = true;
                        }
                        else
                        {
                            foreach (ManicDigger.Client client in serverClient.Clients)
                            {
                                if (client.Name.Equals(k.Value.playername, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (client.FillLimit != null)
                                    {
                                        hasEntry = true;
                                    }
                                    break;
                                }
                            }
                        }
                        if (!hasEntry)
                        {
                            this.SetFillAreaLimit(k.Key);
                        }
                    }
                    SendMessage(sourceClientId, string.Format("{0}Default fill area limit set to {1}.", colorSuccess, maxFill));
                    ServerEventLog(String.Format("{0} sets default fill area limit to {1}.", GetClient(sourceClientId).playername, maxFill));
                    return true;
                case "-group":
                case "-g":
                    // Check if group even exists.
                    ManicDigger.Group targetGroup = serverClient.Groups.Find(
                        delegate(ManicDigger.Group grp)
                    {
                        return grp.Name.Equals(target,StringComparison.InvariantCultureIgnoreCase);
                    }
                    );
                    if (targetGroup == null)
                    {
                        SendMessage(sourceClientId, string.Format("{0}Group {1} not found.", colorError, target));
                        return false;
                    }
                    targetGroup.FillLimit = maxFill;
                    SaveServerClient();
                    // Inform related players.
                    hasEntry = false;
                    foreach (var k in clients)
                    {
                        if (k.Value.clientGroup.Name.Equals(targetGroup.Name))
                        {
                            // Inform only if there is no spawn set under clients.
                            foreach (ManicDigger.Client client in serverClient.Clients)
                            {
                                if (client.Name.Equals(k.Value.playername, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (client.FillLimit != null)
                                    {
                                        hasEntry = true;
                                    }
                                    break;
                                }
                            }
                            if (!hasEntry)
                            {
                                this.SetFillAreaLimit(k.Key);
                            }
                        }
                    }
                    SendMessage(sourceClientId, string.Format("{0}Fill area limit of group {1} set to {2}.", colorSuccess, targetGroup.Name, maxFill));
                    ServerEventLog(String.Format("{0} sets spawn of group {1} to {2}.", GetClient(sourceClientId).playername, targetGroup.Name, maxFill));
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

                    ManicDigger.Client clientEntry = serverClient.Clients.Find(
                        delegate(ManicDigger.Client client)
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
                    clientEntry.FillLimit = maxFill;
                    SaveServerClient();
                    // Inform player if he's online.
                    if (targetClientId != null)
                    {
                        this.SetFillAreaLimit(targetClientId.Value);
                    }
                    SendMessage(sourceClientId, string.Format("{0}Fill area limit of player {1} set to {2}.", colorSuccess, targetClientPlayername, maxFill));
                    ServerEventLog(String.Format("{0} sets fill area limit of player {1} to {2}.", GetClient(sourceClientId).playername, targetClientPlayername, maxFill));
                    return true;
                default:
                    SendMessage(sourceClientId, "Invalid type.");
                    return false;
            }
        }
    }
}
