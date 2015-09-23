using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ManicDigger
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

		[XmlElement(IsNullable = true)]
		public Spawn DefaultSpawn { get; set; }

		[XmlElement(IsNullable = true)]
		public int? DefaultFillLimit { get; set; }

		public ServerClient()
		{
			//Set Defaults
			this.Format = 1;
			this.DefaultGroupGuests = "Guest";
			this.DefaultGroupRegistered = "Guest";
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
		[XmlElement(IsNullable = true)]
		public Spawn Spawn { get; set; }
		[XmlElement(IsNullable = true)]
		public int? FillLimit { get; set; }

		[XmlArrayItem(ElementName = "Privilege")]
		public List<string> GroupPrivileges { get; set; }

		public ServerClientMisc.ClientColor GroupColor { get; set; }

		public Group()
		{
			this.Name = "";
			this.Level = 0;
			this.GroupPrivileges = new List<string>();
			this.GroupColor = ServerClientMisc.ClientColor.White;
		}

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

	public class Client : IComparable<Client>
	{
		public string Name { get; set; }
		public string Group { get; set; }
		[XmlElement(IsNullable = true)]
		public Spawn Spawn{ get; set; }
		[XmlElement(IsNullable = true)]
		public int? FillLimit { get; set; }

		public Client()
		{
			this.Name = "";
			this.Group = "";
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}", this.Name, this.Group);
		}

		// Clients are sorted by groups.
		public int CompareTo(Client other)
		{
			return Group.CompareTo(other.Group);
		}
	}

	public class Spawn
	{
		[XmlIgnoreAttribute]
		public int x;
		[XmlIgnoreAttribute]
		public int y;
		// z is optional
		[XmlIgnoreAttribute]
		public int? z;

		public string Coords
		{
			get
			{
				string zString = "";
				if (this.z != null)
				{
					zString = "," + this.z.ToString();
				}
				return this.x.ToString() + "," + this.y.ToString() + zString;
			}
			set
			{
				string coords = value;
				string[] ss = coords.Split(new char[] { ',' });

				try
				{
					this.x = Convert.ToInt32(ss[0]);
					this.y = Convert.ToInt32(ss[1]);
				}
				catch (FormatException e)
				{
					throw new FormatException("Invalid spawn position.", e);
				}
				catch (OverflowException e)
				{
					throw new FormatException("Invalid spawn position.", e);
				}
				catch(IndexOutOfRangeException ex)
				{
					throw new IndexOutOfRangeException("Invalid spawn position.", ex);
				}

				try
				{
					this.z = Convert.ToInt32(ss[2]);
				}
				catch(IndexOutOfRangeException)
				{
					this.z = null;
				}
				catch (FormatException e)
				{
					throw new FormatException("Invalid spawn position.", e);
				}
				catch (OverflowException e)
				{
					throw new FormatException("Invalid spawn position.", e);
				}
			}
		}
		public Spawn()
		{
			this.x = 0;
			this.y = 0;
		}
		public override string ToString()
		{
			return this.Coords;
		}
	}

	public class ServerClientMisc
	{
		public static string DefaultPlayerName = "Player name?";

		public enum ClientColor
		{
			Black,		// &0
			Blue,		// &1
			Green,		// &2
			Cyan,		// &3
			Red,		// &4
			Purple,		// &5
			Yellow,		// &6
			Grey,		// &7
			DarkGrey,	// &8
			LightBlue,	// &9
			LightGreen,	// &a
			LightCyan,	// &b
			LightRed,	// &c
			LightPink,	// &d
			LightYellow,// &e
			White		// &f
		};

		public class Privilege
		{
			public static string[] All()
			{
				return new string[]
				{
					build,
					use,
					freemove,
					chat,
					pm,
					kick,
					kick_id,
					ban,
					ban_id,
					ban_id,
					banip,
					banip_id,
					ban_offline,
					unban,
					run,
					chgrp,
					chgrp_offline,
					remove_client,
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
					area_add,
					area_delete,
					announcement,
					set_spawn,
					set_home,
					use_tnt,
					privilege_add,
					privilege_remove,
					restart,
					shutdown,
					tp,
					tp_pos,
					teleport_player,
					backup_database,
					reset_inventory,
					fill_limit,
					mode,
					load,
					time,
				};
			}
			public static string build = "build";
			public static string use = "use";
			public static string freemove = "freemove";
			public static string chat = "chat";
			public static string pm = "pm";
			public static string kick = "kick";
			public static string kick_id = "kick_id";
			public static string ban = "ban";
			public static string ban_id = "ban_id";
			public static string banip = "banip";
			public static string banip_id = "banip_id";
			public static string ban_offline = "ban_offline";
			public static string unban = "unban";
			public static string run = "run";
			public static string chgrp = "chgrp";
			public static string chgrp_offline = "chgrp_offline";
			public static string remove_client = "remove_client";
			public static string login = "login";
			public static string welcome = "welcome";
			public static string logging = "logging";
			public static string list_clients = "list_clients";
			public static string list_saved_clients = "list_saved_clients";
			public static string list_groups = "list_groups";
			public static string list_banned_users = "list_banned_users";
			public static string list_areas = "list_areas";
			public static string give = "give";
			public static string giveall = "giveall";
			public static string monsters = "monsters";
			public static string area_add = "area_add";
			public static string area_delete = "area_delete";
			public static string announcement = "announcement";
			public static string set_spawn = "set_spawn";
			public static string set_home = "set_home";
			public static string use_tnt = "use_tnt";
			public static string privilege_add = "privilege_add";
			public static string privilege_remove = "privilege_remove";
			public static string restart = "restart";
			public static string shutdown = "shutdown";
			public static string tp = "tp";
			public static string tp_pos = "tp_pos";
			public static string teleport_player = "teleport_player";
			public static string backup_database = "backup_database";
			public static string reset_inventory = "reset_inventory";
			public static string fill_limit = "fill_limit";
			public static string mode = "mode";
			public static string load = "load";
			public static string time = "time";
		};

		public static List<Group> getDefaultGroups()
		{
			List<Group > defaultGroups = new List<Group>();
			// default guest group
			ManicDigger.Group guest = new ManicDigger.Group();
			guest.Name = "Guest";
			guest.Level = 0;
			guest.GroupPrivileges = new List<string>();
			guest.GroupPrivileges.Add(Privilege.chat);
			guest.GroupPrivileges.Add(Privilege.pm);
			guest.GroupPrivileges.Add(Privilege.build);
			guest.GroupPrivileges.Add(Privilege.use);
			guest.GroupPrivileges.Add(Privilege.login);
			guest.GroupPrivileges.Add(Privilege.tp);
			guest.GroupPrivileges.Add(Privilege.tp_pos);
			guest.GroupPrivileges.Add(Privilege.freemove);
			guest.GroupColor = ClientColor.Grey;
			defaultGroups.Add(guest);
			// default builder group
			ManicDigger.Group builder = new ManicDigger.Group();
			builder.Name = "Builder";
			builder.Level = 1;
			builder.GroupPrivileges = new List<string>();
			builder.GroupPrivileges.Add(Privilege.chat);
			builder.GroupPrivileges.Add(Privilege.pm);
			builder.GroupPrivileges.Add(Privilege.build);
			builder.GroupPrivileges.Add(Privilege.use);
			builder.GroupPrivileges.Add(Privilege.login);
			builder.GroupPrivileges.Add(Privilege.tp);
			builder.GroupPrivileges.Add(Privilege.tp_pos);
			builder.GroupPrivileges.Add(Privilege.set_home);
			builder.GroupPrivileges.Add(Privilege.freemove);
			builder.GroupColor = ClientColor.Green;
			defaultGroups.Add(builder);
			// default moderator group
			ManicDigger.Group moderator = new ManicDigger.Group();
			moderator.Name = "Moderator";
			moderator.Level = 2;
			moderator.GroupPrivileges = new List<string>();
			moderator.GroupPrivileges.Add(Privilege.chat);
			moderator.GroupPrivileges.Add(Privilege.pm);
			moderator.GroupPrivileges.Add(Privilege.build);
			moderator.GroupPrivileges.Add(Privilege.use);
			moderator.GroupPrivileges.Add(Privilege.freemove);
			moderator.GroupPrivileges.Add(Privilege.kick);
			moderator.GroupPrivileges.Add(Privilege.ban);
			moderator.GroupPrivileges.Add(Privilege.banip);
			moderator.GroupPrivileges.Add(Privilege.ban_offline);
			moderator.GroupPrivileges.Add(Privilege.unban);
			moderator.GroupPrivileges.Add(Privilege.list_clients);
			moderator.GroupPrivileges.Add(Privilege.list_saved_clients);
			moderator.GroupPrivileges.Add(Privilege.list_groups);
			moderator.GroupPrivileges.Add(Privilege.list_banned_users);
			moderator.GroupPrivileges.Add(Privilege.list_areas);
			moderator.GroupPrivileges.Add(Privilege.chgrp);
			moderator.GroupPrivileges.Add(Privilege.chgrp_offline);
			moderator.GroupPrivileges.Add(Privilege.remove_client);
			moderator.GroupPrivileges.Add(Privilege.use_tnt);
			moderator.GroupPrivileges.Add(Privilege.restart);
			moderator.GroupPrivileges.Add(Privilege.login);
			moderator.GroupPrivileges.Add(Privilege.tp);
			moderator.GroupPrivileges.Add(Privilege.tp_pos);
			moderator.GroupPrivileges.Add(Privilege.set_home);
			moderator.GroupPrivileges.Add(Privilege.mode);
			moderator.GroupColor = ClientColor.Cyan;
			defaultGroups.Add(moderator);
			// default admin group
			ManicDigger.Group admin = new ManicDigger.Group();
			admin.Name = "Admin";
			admin.Level = 3;
			admin.GroupPrivileges = new List<string>();
			admin.GroupPrivileges.Add(Privilege.chat);
			admin.GroupPrivileges.Add(Privilege.pm);
			admin.GroupPrivileges.Add(Privilege.build);
			admin.GroupPrivileges.Add(Privilege.use);
			admin.GroupPrivileges.Add(Privilege.freemove);
			admin.GroupPrivileges.Add(Privilege.kick);
			admin.GroupPrivileges.Add(Privilege.ban);
			admin.GroupPrivileges.Add(Privilege.banip);
			admin.GroupPrivileges.Add(Privilege.ban_offline);
			admin.GroupPrivileges.Add(Privilege.unban);
			admin.GroupPrivileges.Add(Privilege.announcement);
			admin.GroupPrivileges.Add(Privilege.welcome);
			admin.GroupPrivileges.Add(Privilege.list_clients);
			admin.GroupPrivileges.Add(Privilege.list_saved_clients);
			admin.GroupPrivileges.Add(Privilege.list_groups);
			admin.GroupPrivileges.Add(Privilege.list_banned_users);
			admin.GroupPrivileges.Add(Privilege.list_areas);
			admin.GroupPrivileges.Add(Privilege.chgrp);
			admin.GroupPrivileges.Add(Privilege.chgrp_offline);
			admin.GroupPrivileges.Add(Privilege.remove_client);
			admin.GroupPrivileges.Add(Privilege.monsters);
			admin.GroupPrivileges.Add(Privilege.give);
			admin.GroupPrivileges.Add(Privilege.giveall);
			admin.GroupPrivileges.Add(Privilege.use_tnt);
			admin.GroupPrivileges.Add(Privilege.area_add);
			admin.GroupPrivileges.Add(Privilege.area_delete);
			admin.GroupPrivileges.Add(Privilege.restart);
			admin.GroupPrivileges.Add(Privilege.login);
			admin.GroupPrivileges.Add(Privilege.tp);
			admin.GroupPrivileges.Add(Privilege.tp_pos);
			admin.GroupPrivileges.Add(Privilege.set_home);
			admin.GroupPrivileges.Add(Privilege.mode);
			admin.GroupPrivileges.Add(Privilege.load);
			admin.GroupPrivileges.Add(Privilege.time);
			admin.GroupPrivileges.Add("revert");
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

		public static string PrivilegesString(List<string> privileges)
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
				case ClientColor.Blue:
					return "&1";
				case ClientColor.Green:
					return "&2";
				case ClientColor.Cyan:
					return "&3";
				case ClientColor.Red:
					return "&4";
				case ClientColor.Purple:
					return "&5";
				case ClientColor.Yellow:
					return "&6";
				case ClientColor.Grey:
					return "&7";
				case ClientColor.DarkGrey:
					return "&8";
				case ClientColor.LightBlue:
					return "&9";
				case ClientColor.LightGreen:
					return "&a";
				case ClientColor.LightCyan:
					return "&b";
				case ClientColor.LightRed:
					return "&c";
				case ClientColor.LightPink:
					return "&d";
				case ClientColor.LightYellow:
					return "&e";
				case ClientColor.White:
					return "&f";
				default:
					return "&f"; // white
			}
		}
	}
}
