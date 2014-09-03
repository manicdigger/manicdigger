using System;
using ManicDigger;
using System.Collections.Generic;
using System.IO;
using ManicDigger.ClientNative;
using System.Xml.Serialization;

public class ServerSystemBanList : ServerSystem
{
    bool loaded;
    public override void Update(Server server, float dt)
    {
        if (!loaded)
        {
            loaded = true;
            LoadBanlist(server);
        }

        if (server.banlist.ClearTimeBans() > 0)
        {
            SaveBanlist(server);
        }

        foreach (KeyValuePair<int, ClientOnServer> k in server.clients)
        {
            int clientId = k.Key;
            ClientOnServer c = k.Value;
            IPEndPointCi iep1 = c.socket.RemoteEndPoint();

            if (server.banlist.IsIPBanned(iep1.AddressToString()))
            {
                IPEntry entry = server.banlist.GetIPEntry(iep1.AddressToString());
                string reason = entry.Reason;
                if (string.IsNullOrEmpty(reason))
                    reason = "";
                server.SendPacket(clientId, ServerPackets.DisconnectPlayer(string.Format(server.language.ServerIPBanned(), reason)));
                Console.WriteLine(string.Format("Banned IP {0} tries to connect.", iep1.AddressToString()));
                server.ServerEventLog(string.Format("Banned IP {0} tries to connect.", iep1.AddressToString()));
                server.KillPlayer(clientId);
                continue;
            }

            string username = c.playername;
            if (server.banlist.IsUserBanned(username))
            {
                UserEntry entry = server.banlist.GetUserEntry(username);
                string reason = entry.Reason;
                if (string.IsNullOrEmpty(reason))
                    reason = "";
                server.SendPacket(clientId, ServerPackets.DisconnectPlayer(string.Format(server.language.ServerUsernameBanned(), reason)));
                Console.WriteLine(string.Format("{0} fails to join (banned username: {1}).", (c.socket.RemoteEndPoint()).AddressToString(), username));
                server.ServerEventLog(string.Format("{0} fails to join (banned username: {1}).", (c.socket.RemoteEndPoint()).AddressToString(), username));
                server.KillPlayer(clientId);
                continue;
            }
        }
    }

    public override bool OnCommand(Server server, int sourceClientId, string command, string argument)
    {
        string[] ss;
        int id;
        int duration;

        Language language = server.language;
        string colorError = server.colorError;

        switch (command)
        {
            case "banip_id":
                ss = argument.Split(new[] { ' ' });
                if (!Int32.TryParse(ss[0], out id))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (ss.Length >= 2)
                {
                    BanIP(server, sourceClientId, id, string.Join(" ", ss, 1, ss.Length - 1));
                    return true;
                }
                BanIP(server, sourceClientId, id);
                return true;
            case "banip":
                ss = argument.Split(new[] { ' ' });
                if (ss.Length >= 2)
                {
                    BanIP(server, sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                    return true;
                }
                BanIP(server, sourceClientId, argument);
                return true;
            case "ban_id":
                ss = argument.Split(new[] { ' ' });
                if (!Int32.TryParse(ss[0], out id))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (ss.Length >= 2)
                {
                    Ban(server, sourceClientId, id, string.Join(" ", ss, 1, ss.Length - 1));
                    return true;
                }
                Ban(server, sourceClientId, id);
                return true;
            case "ban":
                ss = argument.Split(new[] { ' ' });
                if (ss.Length >= 2)
                {
                    Ban(server, sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                    return true;
                }
                Ban(server, sourceClientId, argument);
                return true;
            case "timebanip_id":  //Format: /timebanip_id <player_id> <duration> [reason]
                ss = argument.Split(new[] { ' ' });
                if (ss.Length < 2)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (!Int32.TryParse(ss[0], out id))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (!Int32.TryParse(ss[1], out duration))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (duration <= 0)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandTimeBanInvalidValue"));
                    return true;
                }
                if (ss.Length >= 3)
                {
                    this.TimeBanIP(server, sourceClientId, id, string.Join(" ", ss, 2, ss.Length - 2), duration);
                    return true;
                }
                this.TimeBanIP(server, sourceClientId, id, duration);
                return true;
            case "timebanip":  //Format: /timebanip <playername> <duration> [reason]
                ss = argument.Split(new[] { ' ' });
                if (ss.Length < 2)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (!Int32.TryParse(ss[1], out duration))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (duration <= 0)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandTimeBanInvalidValue"));
                    return true;
                }
                if (ss.Length >= 3)
                {
                    this.TimeBanIP(server, sourceClientId, ss[0], string.Join(" ", ss, 2, ss.Length - 2), duration);
                    return true;
                }
                this.TimeBanIP(server, sourceClientId, ss[0], duration);
                return true;
            case "timeban_id":  //Format: /timeban_id <player_id> <duration> [reason]
                ss = argument.Split(new[] { ' ' });
                if (ss.Length < 2)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (!Int32.TryParse(ss[0], out id))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (!Int32.TryParse(ss[1], out duration))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (duration <= 0)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandTimeBanInvalidValue"));
                    return true;
                }
                if (ss.Length >= 3)
                {
                    this.TimeBan(server, sourceClientId, id, string.Join(" ", ss, 2, ss.Length - 2), duration);
                    return true;
                }
                this.TimeBan(server, sourceClientId, id, duration);
                return true;
            case "timeban":  //Format: /timeban <playername> <duration> [reason]
                ss = argument.Split(new[] { ' ' });
                if (ss.Length < 2)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (!Int32.TryParse(ss[1], out duration))
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                    return true;
                }
                if (duration <= 0)
                {
                    server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandTimeBanInvalidValue"));
                    return true;
                }
                if (ss.Length >= 3)
                {
                    this.TimeBan(server, sourceClientId, ss[0], string.Join(" ", ss, 2, ss.Length - 2), duration);
                    return true;
                }
                this.TimeBan(server, sourceClientId, ss[0], duration);
                return true;
            case "ban_offline":
                ss = argument.Split(new[] { ' ' });
                if (ss.Length >= 2)
                {
                    this.BanOffline(server, sourceClientId, ss[0], string.Join(" ", ss, 1, ss.Length - 1));
                    return true;
                }
                this.BanOffline(server, sourceClientId, argument);
                return true;
            case "unban":
                ss = argument.Split(new[] { ' ' });
                if (ss.Length == 2)
                {
                    this.Unban(server, sourceClientId, ss[0], ss[1]);
                    return true;
                }
                server.SendMessage(sourceClientId, colorError + language.Get("Server_CommandInvalidArgs"));
                return true;
            default:
                return false;
        }
    }

    public bool Ban(Server server, int sourceClientId, string target)
    {
        return Ban(server, sourceClientId, target, "");
    }

    public bool Ban(Server server, int sourceClientId, string target, string reason)
    {
        ClientOnServer targetClient = server.GetClient(target);
        if (targetClient != null)
        {
            return this.Ban(server, sourceClientId, targetClient.Id, reason);
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandPlayerNotFound"), server.colorError, target));
        return false;
    }

    public bool Ban(Server server, int sourceClientId, int targetClientId)
    {
        return this.Ban(server, sourceClientId, targetClientId, "");
    }

    public bool Ban(Server server, int sourceClientId, int targetClientId, string reason)
    {
        if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.ban))
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
            return false;
        }
        if (!reason.Equals(""))
        {
            reason = server.language.Get("Server_CommandKickBanReason") + reason + ".";
        }
        ClientOnServer targetClient = server.GetClient(targetClientId);
        if (targetClient != null)
        {
            if (targetClient.clientGroup.IsSuperior(server.GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(server.GetClient(sourceClientId).clientGroup))
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandTargetUserSuperior"), server.colorError));
                return false;
            }
            string targetName = targetClient.playername;
            string sourceName = server.GetClient(sourceClientId).playername;
            string targetNameColored = targetClient.ColoredPlayername(server.colorImportant);
            string sourceNameColored = server.GetClient(sourceClientId).ColoredPlayername(server.colorImportant);
            server.banlist.BanPlayer(targetName, sourceName, reason);
            SaveBanlist(server);
            server.SendMessageToAll(string.Format(server.language.Get("Server_CommandBanMessage"), server.colorImportant, targetNameColored, sourceNameColored, reason));
            server.ServerEventLog(string.Format("{0} bans {1}.{2}", sourceName, targetName, reason));
            server.SendPacket(targetClientId, ServerPackets.DisconnectPlayer(string.Format(server.language.Get("Server_CommandBanNotification"), reason)));
            server.KillPlayer(targetClientId);
            return true;
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandNonexistantID"), server.colorError, targetClientId));
        return false;
    }

    public bool BanIP(Server server, int sourceClientId, string target)
    {
        return BanIP(server, sourceClientId, target, "");
    }

    public bool BanIP(Server server, int sourceClientId, string target, string reason)
    {
        ClientOnServer targetClient = server.GetClient(target);
        if (targetClient != null)
        {
            return this.BanIP(server, sourceClientId, targetClient.Id, reason);
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandPlayerNotFound"), server.colorError, target));
        return false;
    }

    public bool BanIP(Server server, int sourceClientId, int targetClientId)
    {
        return this.BanIP(server, sourceClientId, targetClientId, "");
    }

    public bool BanIP(Server server, int sourceClientId, int targetClientId, string reason)
    {
        if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.banip))
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
            return false;
        }
        if (!reason.Equals(""))
        {
            reason = server.language.Get("Server_CommandKickBanReason") + reason + ".";
        }
        ClientOnServer targetClient = server.GetClient(targetClientId);
        if (targetClient != null)
        {
            if (targetClient.clientGroup.IsSuperior(server.GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(server.GetClient(sourceClientId).clientGroup))
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandTargetUserSuperior"), server.colorError));
                return false;
            }
            string targetName = targetClient.playername;
            string sourceName = server.GetClient(sourceClientId).playername;
            string targetNameColored = targetClient.ColoredPlayername(server.colorImportant);
            string sourceNameColored = server.GetClient(sourceClientId).ColoredPlayername(server.colorImportant);
            server.banlist.BanIP((targetClient.socket.RemoteEndPoint()).AddressToString(), sourceName, reason);
            SaveBanlist(server);
            server.SendMessageToAll(string.Format(server.language.Get("Server_CommandIPBanMessage"), server.colorImportant, targetNameColored, sourceNameColored, reason));
            server.ServerEventLog(string.Format("{0} IP bans {1}.{2}", sourceName, targetName, reason));
            server.SendPacket(targetClientId, ServerPackets.DisconnectPlayer(string.Format(server.language.Get("Server_CommandIPBanNotification"), reason)));
            server.KillPlayer(targetClientId);
            return true;
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandNonexistantID"), server.colorError, targetClientId));
        return false;
    }

    public bool TimeBan(Server server, int sourceClientId, string target, int duration)
    {
        return TimeBan(server, sourceClientId, target, "", duration);
    }

    public bool TimeBan(Server server, int sourceClientId, string target, string reason, int duration)
    {
        ClientOnServer targetClient = server.GetClient(target);
        if (targetClient != null)
        {
            return this.TimeBan(server, sourceClientId, targetClient.Id, reason, duration);
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandPlayerNotFound"), server.colorError, target));
        return false;
    }

    public bool TimeBan(Server server, int sourceClientId, int targetClientId, int duration)
    {
        return this.TimeBan(server, sourceClientId, targetClientId, "", duration);
    }

    public bool TimeBan(Server server, int sourceClientId, int targetClientId, string reason, int duration)
    {
        if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.ban))
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
            return false;
        }
        if (!reason.Equals(""))
        {
            reason = server.language.Get("Server_CommandKickBanReason") + reason + ".";
        }
        ClientOnServer targetClient = server.GetClient(targetClientId);
        if (targetClient != null)
        {
            if (targetClient.clientGroup.IsSuperior(server.GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(server.GetClient(sourceClientId).clientGroup))
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandTargetUserSuperior"), server.colorError));
                return false;
            }
            string targetName = targetClient.playername;
            string sourceName = server.GetClient(sourceClientId).playername;
            string targetNameColored = targetClient.ColoredPlayername(server.colorImportant);
            string sourceNameColored = server.GetClient(sourceClientId).ColoredPlayername(server.colorImportant);
            server.banlist.TimeBanPlayer(targetName, sourceName, reason, duration);
            SaveBanlist(server);
            server.SendMessageToAll(string.Format(server.language.Get("Server_CommandTimeBanMessage"), server.colorImportant, targetNameColored, sourceNameColored, duration, reason));
            server.ServerEventLog(string.Format("{0} bans {1} for {2} minutes.{3}", sourceName, targetName, duration, reason));
            server.SendPacket(targetClientId, ServerPackets.DisconnectPlayer(string.Format(server.language.Get("Server_CommandTimeBanNotification"), duration, reason)));
            server.KillPlayer(targetClientId);
            return true;
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandNonexistantID"), server.colorError, targetClientId));
        return false;
    }

    public bool TimeBanIP(Server server, int sourceClientId, string target, int duration)
    {
        return TimeBanIP(server, sourceClientId, target, "", duration);
    }

    public bool TimeBanIP(Server server, int sourceClientId, string target, string reason, int duration)
    {
        ClientOnServer targetClient = server.GetClient(target);
        if (targetClient != null)
        {
            return this.TimeBanIP(server, sourceClientId, targetClient.Id, reason, duration);
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandPlayerNotFound"), server.colorError, target));
        return false;
    }

    public bool TimeBanIP(Server server, int sourceClientId, int targetClientId, int duration)
    {
        return TimeBanIP(server, sourceClientId, targetClientId, "", duration);
    }

    public bool TimeBanIP(Server server, int sourceClientId, int targetClientId, string reason, int duration)
    {
        if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.banip))
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
            return false;
        }
        if (!reason.Equals(""))
        {
            reason = server.language.Get("Server_CommandKickBanReason") + reason + ".";
        }
        ClientOnServer targetClient = server.GetClient(targetClientId);
        if (targetClient != null)
        {
            if (targetClient.clientGroup.IsSuperior(server.GetClient(sourceClientId).clientGroup) || targetClient.clientGroup.EqualLevel(server.GetClient(sourceClientId).clientGroup))
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandTargetUserSuperior"), server.colorError));
                return false;
            }
            string targetName = targetClient.playername;
            string sourceName =server. GetClient(sourceClientId).playername;
            string targetNameColored = targetClient.ColoredPlayername(server.colorImportant);
            string sourceNameColored = server.GetClient(sourceClientId).ColoredPlayername(server.colorImportant);
            server.banlist.TimeBanIP((targetClient.socket.RemoteEndPoint()).AddressToString(), sourceName, reason, duration);
            SaveBanlist(server);
            server.SendMessageToAll(string.Format(server.language.Get("Server_CommandTimeIPBanMessage"),server. colorImportant, targetNameColored, sourceNameColored, duration, reason));
            server.ServerEventLog(string.Format("{0} IP bans {1} for {2} minutes.{3}", sourceName, targetName, duration, reason));
            server.SendPacket(targetClientId, ServerPackets.DisconnectPlayer(string.Format(server.language.Get("Server_CommandTimeIPBanNotification"), duration, reason)));
            server.KillPlayer(targetClientId);
            return true;
        }
        server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandNonexistantID"), server.colorError, targetClientId));
        return false;
    }

    public bool BanOffline(Server server, int sourceClientId, string target)
    {
        return this.BanOffline(server, sourceClientId, target, "");
    }

    public bool BanOffline(Server server, int sourceClientId, string target, string reason)
    {
        if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.ban_offline))
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
            return false;
        }
        if (!reason.Equals(""))
        {
            reason = server.language.Get("Server_CommandKickBanReason") + reason;
        }

        if (server.GetClient(target) != null)
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandBanOfflineTargetOnline"), server.colorError, target));
            return false;
        }

        // Target is at the moment not online. Check if there is an entry in ServerClient

        // Get related client from config file
        ManicDigger.Client targetClient = server.serverClient.Clients.Find(
            delegate(ManicDigger.Client client)
            {
                return client.Name.Equals(target, StringComparison.InvariantCultureIgnoreCase);
            }
        );

        // Entry exists.
        if (targetClient != null)
        {
            // Get target's group.
            ManicDigger.Group targetGroup = server.serverClient.Groups.Find(
                delegate(ManicDigger.Group grp)
                {
                    return grp.Name.Equals(targetClient.Group);
                }
            );
            if (targetGroup == null)
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInvalidGroup"), server.colorError));
                return false;
            }

            // Check if target's group is superior.
            if (targetGroup.IsSuperior(server.GetClient(sourceClientId).clientGroup) || targetGroup.EqualLevel(server.GetClient(sourceClientId).clientGroup))
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandTargetUserSuperior"), server.colorError));
                return false;
            }

            // Remove target's entry.
            server.serverClient.Clients.Remove(targetClient);
            server.serverClientNeedsSaving = true;
        }

        // Finally ban user.
        server.banlist.BanPlayer(target, server.GetClient(sourceClientId).playername, reason);
        SaveBanlist(server);
        server.SendMessageToAll(string.Format(server.language.Get("Server_CommandBanOfflineMessage"), server.colorImportant, target, server.GetClient(sourceClientId).ColoredPlayername(server.colorImportant), reason));
        server.ServerEventLog(string.Format("{0} bans {1}.{2}", server.GetClient(sourceClientId).playername, target, reason));
        return true;
    }

    public bool Unban(Server server, int sourceClientId, string type, string target)
    {
        if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.unban))
        {
            server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
            return false;
        }
        // unban a playername
        if (type.Equals("-p"))
        {
            // case insensitive
            bool exists = server.banlist.UnbanPlayer(target);
            SaveBanlist(server);
            if (!exists)
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandPlayerNotFound"), server.colorError, target));
            }
            else
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandUnbanSuccess"), server.colorSuccess, target));
                server.ServerEventLog(string.Format("{0} unbans player {1}.", server.GetClient(sourceClientId).playername, target));
            }
            return true;
        }
        // unban an IP
        else if (type.Equals("-ip"))
        {
            bool exists = server.banlist.UnbanIP(target);
            SaveBanlist(server);
            if (!exists)
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandUnbanIPNotFound"), server.colorError, target));
            }
            else
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandUnbanIPSuccess"), server.colorSuccess, target));
                server.ServerEventLog(string.Format("{0} unbans IP {1}.", server.GetClient(sourceClientId).playername, target));
            }
            return true;
        }
        server.SendMessage(sourceClientId, string.Format("{0}Invalid type: {1}", server.colorError, type));
        return false;
    }

    public void LoadBanlist(Server server)
    {
        string filename = "ServerBanlist.txt";
        if (!File.Exists(Path.Combine(GameStorePath.gamepathconfig, filename)))
        {
            Console.WriteLine(server.language.ServerBanlistNotFound());
            SaveBanlist(server);
            return;
        }
        try
        {
            using (TextReader textReader = new StreamReader(Path.Combine(GameStorePath.gamepathconfig, filename)))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(ServerBanlist));
                server.banlist = (ServerBanlist)deserializer.Deserialize(textReader);
                textReader.Close();
            }
        }
        catch
        {
            //Banlist corrupt. Try to backup old, then create new one.
            try
            {
                File.Copy(Path.Combine(GameStorePath.gamepathconfig, filename), Path.Combine(GameStorePath.gamepathconfig, filename + ".old"));
                Console.WriteLine(server.language.ServerBanlistCorrupt());
            }
            catch
            {
                Console.WriteLine(server.language.ServerBanlistCorruptNoBackup());
            }
            server.banlist = null;
            SaveBanlist(server);
        }
        SaveBanlist(server);
        Console.WriteLine(server.language.ServerBanlistLoaded());
    }

    public void SaveBanlist(Server server)
    {
        //Verify that we have a directory to place the file into.
        if (!Directory.Exists(GameStorePath.gamepathconfig))
        {
            Directory.CreateDirectory(GameStorePath.gamepathconfig);
        }

        XmlSerializer serializer = new XmlSerializer(typeof(ServerBanlist));
        TextWriter textWriter = new StreamWriter(Path.Combine(GameStorePath.gamepathconfig, "ServerBanlist.txt"));

        //Check to see if banlist has been initialized
        if (server.banlist == null)
        {
            server.banlist = new ServerBanlist();
        }

        //Serialize the ServerBanlist class to XML
        serializer.Serialize(textWriter, server.banlist);
        textWriter.Close();
    }
}
