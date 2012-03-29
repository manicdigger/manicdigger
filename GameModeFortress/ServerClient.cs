using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GameModeFortress
{
    [XmlRoot(ElementName = "ManicDiggerServerClient")]
    public class ServerClient
    {
        public int Format { get; set; }
        public string DefaultGroupGuests { get; set; } // ~ players
        public string DefaultGroupRegistered { get; set; } // registered MD players

        [XmlArrayItem(ElementName = "Group")]
        public List<Group> Groups { get; set; }

        [XmlArrayItem(ElementName = "Client")]
        public List<Client> Clients { get; set; }

        public ServerClient()
        {
            //Set Defaults
            this.Format = 1;
            this.DefaultGroupGuests = "Guest";
            this.DefaultGroupRegistered = "Registered";
            this.Groups = new List<Group>();
            this.Clients = new List<Client>();
        }
    }

    public class Group : IComparable<Group>
    {
        public string Name { get; set; }
        public int Level { get; set; }

        [XmlElement(IsNullable = true)]
        public string Password { get; set; }

        [XmlArrayItem(ElementName = "Privilege")]
        public List<ServerClientMisc.Privilege> GroupPrivileges { get; set; }

        public ServerClientMisc.ClientColor GroupColor { get; set; }

        public string GroupColorString()
        {
            return ServerClientMisc.ClientColorToString(this.GroupColor);
        }
        // Groups are sorted by levels (asc). Higher level groups are superior lower level groups.
        public int CompareTo(Group other)
        {
            return Level.CompareTo(other.Level);
        }

        public bool IsSuperior(Group clientGroup)
        {
            return this.Level > clientGroup.Level;
        }

        public bool EqualLevel(Group clientGroup)
        {
            return this.Level == clientGroup.Level;
        }

        public override string ToString()
        {
            string passwordString = "";
            if (string.IsNullOrEmpty(this.Password))
            {
                passwordString = "X";
            }
            return string.Format("{0}:{1}:{2}:{3}:{4}", this.Name, this.Level, ServerClientMisc.PrivilegesString(this.GroupPrivileges), this.GroupColor.ToString(), passwordString);
        }

    }

    public class Client
    {
        public string Name { get; set; }
        public string Group { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.Name, this.Group);
        }
    }

    public static class ServerClientMisc
    {
        public static string DefaultPlayerName = "Player name?";

        public enum ClientColor
        {
            White,
            Black,
            Red,
            Green,
            Blue,
            Yellow,
            Cyan,
            Purple,
            Grey
        };

        public enum Privilege
        {
            build,
            chat,
            pm,
            kick,
            kick_id,
            ban,
            ban_id,
            banip,
            banip_id,
            unban,
            run,
            chgrp,
            login,
            welcome,
            logging,
            list_clients,
            list_saved_clients,
            list_groups,
            list_banned_users,
            list_areas,
            give,
            giveall,
            monsters,
            announcement
        };

        public static List<Group> getDefaultGroups()
        {
            List<Group > defaultGroups = new List<Group>();
            // default guest group
            GameModeFortress.Group guest = new GameModeFortress.Group();
            guest.Name = "Guest";
            guest.Level = 0;
            guest.GroupPrivileges = new List<Privilege>();
            guest.GroupPrivileges.Add(Privilege.chat);
            guest.GroupPrivileges.Add(Privilege.pm);
            guest.GroupPrivileges.Add(Privilege.build);
            guest.GroupColor = ClientColor.Cyan;
            defaultGroups.Add(guest);
            // default registered group
            GameModeFortress.Group registered = new GameModeFortress.Group();
            registered.Name = "Registered";
            registered.Level = 0;
            registered.GroupPrivileges = new List<Privilege>();
            registered.GroupPrivileges.Add(Privilege.chat);
            registered.GroupPrivileges.Add(Privilege.pm);
            registered.GroupPrivileges.Add(Privilege.build);
            registered.GroupColor = ClientColor.Cyan;
            defaultGroups.Add(registered);
            // default builder group
            GameModeFortress.Group builder = new GameModeFortress.Group();
            builder.Name = "Builder";
            builder.Level = 1;
            builder.GroupPrivileges = new List<Privilege>();
            builder.GroupPrivileges.Add(Privilege.chat);
            builder.GroupPrivileges.Add(Privilege.pm);
            builder.GroupPrivileges.Add(Privilege.build);
            builder.GroupColor = ClientColor.Green;
            defaultGroups.Add(builder);
            // default admin group
            GameModeFortress.Group admin = new GameModeFortress.Group();
            admin.Name = "Admin";
            admin.Level = 2;
            admin.GroupPrivileges = new List<Privilege>();
            admin.GroupPrivileges.Add(Privilege.chat);
            admin.GroupPrivileges.Add(Privilege.pm);
            admin.GroupPrivileges.Add(Privilege.build);
            admin.GroupPrivileges.Add(Privilege.kick);
            admin.GroupPrivileges.Add(Privilege.ban);
            admin.GroupPrivileges.Add(Privilege.unban);
            admin.GroupPrivileges.Add(Privilege.announcement);
            admin.GroupPrivileges.Add(Privilege.welcome);
            admin.GroupPrivileges.Add(Privilege.list_clients);
            admin.GroupPrivileges.Add(Privilege.list_saved_clients);
            admin.GroupPrivileges.Add(Privilege.list_groups);
            admin.GroupPrivileges.Add(Privilege.list_banned_users);
            admin.GroupPrivileges.Add(Privilege.list_areas);
            admin.GroupPrivileges.Add(Privilege.chgrp);
            admin.GroupPrivileges.Add(Privilege.monsters);
            admin.GroupPrivileges.Add(Privilege.give);
            admin.GroupPrivileges.Add(Privilege.giveall);
            admin.GroupColor = ClientColor.Yellow;
            defaultGroups.Add(admin);

            defaultGroups.Sort();
            return defaultGroups;
        }

        public static List<Client> getDefaultClients()
        {
            List<Client > defaultClients = new List<Client>();
            Client defaultClient = new Client();
            defaultClient.Name = DefaultPlayerName;
            defaultClient.Group = getDefaultGroups()[0].Name;
            defaultClients.Add(defaultClient);

            return defaultClients;
        }

        public static string PrivilegesString(List<Privilege> privileges)
        {
            string privilegesString = "";
            if (privileges.Count > 0)
            {
                privilegesString = privileges[0].ToString();
                for (int i = 1; i < privileges.Count; i++)
                {
                    privilegesString += "," + privileges[i].ToString();
                }
            }
            return privilegesString;
        }

        public static string ClientColorToString(ClientColor color)
        {
            switch (color)
            {
                case ClientColor.Black:
                    return "&0";
                case ClientColor.White:
                    return "&f";
                case ClientColor.Red:
                    return "&4";
                case ClientColor.Green:
                    return "&2";
                case ClientColor.Blue:
                    return "&1";
                case ClientColor.Cyan:
                    return "&3";
                case ClientColor.Yellow:
                    return "&6";
                case ClientColor.Purple:
                    return "&5";
                case ClientColor.Grey:
                    return "&7";
                default:
                    return "&f"; // white
            }
        }
    }
}

