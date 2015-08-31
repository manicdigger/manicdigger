using System;
using System.Collections.Generic;
using System.Drawing;

namespace ManicDigger.Mods.War
{
	public class War : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager manager)
		{
			m = manager;
			CurrentRespawnTime = DateTime.UtcNow;
			//Basic settings
			m.SetCreative(false);
			m.SetWorldSize(256, 256, 128);
			m.SetWorldDatabaseReadOnly(true); // WarMode.TeamDeathmatch
			m.DisablePrivilege("tp");
			
			//Register specific functions
			m.RegisterOnPlayerJoin(PlayerJoin);
			m.RegisterOnDialogClick(DialogClickSelectTeam);
			m.RegisterOnDialogClick(DialogClickSelectClass);
			m.RegisterOnDialogClick(DialogClickSelectSubclass);
			m.RegisterOnWeaponHit(Hit);
			m.RegisterOnSpecialKey(RespawnKey);
			m.RegisterOnSpecialKey(OnTabKey);
			m.RegisterOnDialogClick(OnTabResponse);
			m.RegisterOnSpecialKey(OnSelectTeamKey);
			m.RegisterChangedActiveMaterialSlot(UpdatePlayerModel);
			m.RegisterOnWeaponShot(Shot);
			m.RegisterOnPlayerChat(OnChat);
			m.RegisterOnCommand(OnCommand);
			m.RegisterOnBlockBuild(OnBuild);
			m.RegisterOnPlayerDeath(OnPlayerDeath);
			
			//Register timers
			m.RegisterTimer(UpdateMedicalKitAmmoPack, 0.1);
			m.RegisterTimer(UpdateRespawnTimer, 1);
			m.RegisterTimer(UpdateTab, 1);
		}
		
		public enum WarMode
		{
			Edit,
			TeamDeathmatch,
		}
		
		WarMode warmode = WarMode.TeamDeathmatch;
		
		public enum PlayerClass
		{
			Soldier,
			Medic,
			Support,
		}
		public enum SoldierSubclass
		{
			SubmachineGun,
			Shotgun,
			Rifle,
		}
		
		TimeSpan RespawnTime = TimeSpan.FromSeconds(30);
		//TimeSpan RoundTime = TimeSpan.FromMinutes(30);
		DateTime CurrentRespawnTime;
		
		public bool EnableTeamkill = true;
		
		Dictionary<int, Player> players = new Dictionary<int, Player>();
		
		public class Player
		{
			public Team team = Team.Spectator;
			public int kills;
			public bool isdead;
			public int following = -1;
			public bool firstteam = true;
			public PlayerClass playerclass;
			public SoldierSubclass soldierSubclass;
			public Dictionary<int, int> totalAmmo = new Dictionary<int, int>();
		}
		
		ModManager m;
		
		void PlayerJoin(int playerid)
		{
			m.SetPlayerHealth(playerid, 100, 100);
			players[playerid] = new Player();
			switch (warmode)
			{
				case WarMode.Edit:
					m.EnableExtraPrivilegeToAll("build", false);
					m.EnableFreemove(playerid, true);
					int posx = m.GetMapSizeX() / 2;
					int posy = m.GetMapSizeY() / 2;
					int posz = BlockHeight(posx, posy);
					m.SetPlayerPosition(playerid, posx, posy, posz);
					ClearInventory(playerid);
					GiveAllBlocks(playerid);
					m.SetGlobalDataNotSaved("enablewater", false);
					break;
				case WarMode.TeamDeathmatch:
					m.SetCreative(false);
					m.EnableExtraPrivilegeToAll("build", true);
					m.EnableFreemove(playerid, false);
					ShowTeamSelectionDialog(playerid);
					m.SetGlobalDataNotSaved("enablewater", true);
					break;
			}
		}
		
		void GiveAllBlocks(int playerid)
		{
			for (int i = 1; i < m.GetMaxBlockTypes(); i++)
			{
				var b = m.GetBlockType(i);
				if (b != null)
				{
					m.GrabBlocks(playerid, i, 9999);
				}
			}
			m.NotifyInventory(playerid);
		}
		
		void ClearInventory(int playerid)
		{
			Inventory inv = m.GetInventory(playerid);
			inv.Boots = null;
			inv.DragDropItem = null;
			inv.Gauntlet = null;
			inv.Helmet = null;
			inv.Items.Clear();
			inv.MainArmor = null;
			Array.Clear(inv.RightHand, 0, inv.RightHand.Length);
			m.NotifyInventory(playerid);
		}
		
		void ShowTeamSelectionDialog(int playerid)
		{
			Dialog d = new Dialog();
			List<Widget> widgets = new List<Widget>();
			Widget background = new Widget();
			background.X = 0;
			background.Y = 0;
			background.Width = 800;
			background.Height = 800;
			background.Image = "SelectTeam";
			widgets.Add(background);
			Widget w1 = new Widget();
			w1.Id = "Team1";
			w1.Text = "Press 1 to join Blue";
			w1.X = 50;
			w1.Y = 400;
			w1.ClickKey = '1';
			widgets.Add(w1);
			Widget w2 = new Widget();
			w2.Text = "Press 2 to join Green";
			w2.Id = "Team2";
			w2.X = 600;
			w2.Y = 400;
			w2.ClickKey = '2';
			widgets.Add(w2);
			Widget w3 = new Widget();
			w3.Text = "Press 3 to spectate";
			w3.Id = "Team3";
			w3.X = 300;
			w3.Y = 400;
			w3.ClickKey = '3';
			widgets.Add(w3);
			d.Width = 800;
			d.Height = 600;
			d.Widgets = widgets.ToArray();
			m.SendDialog(playerid, "SelectTeam" + playerid, d);
		}
		
		void ShowClassSelectionDialog(int playerid)
		{
			Dialog d = new Dialog();
			List<Widget> widgets = new List<Widget>();
			Widget background = new Widget();
			background.X = 0;
			background.Y = 0;
			background.Width = 800;
			background.Height = 800;
			background.Image = "SelectClass";
			widgets.Add(background);
			string[] classes = { "Soldier", "Medic", "Support" };
			for (int i = 0; i < 3; i++)
			{
				Widget w = new Widget();
				w.Id = "Class" + (i + 1).ToString();
				w.Text = string.Format("Press {0} for {1}", i + 1, classes[i]);
				w.X = 50 + 250 * i;
				w.Y = 400;
				w.ClickKey = (i + 1).ToString()[0];
				widgets.Add(w);
			}
			d.Width = 800;
			d.Height = 600;
			d.Widgets = widgets.ToArray();
			m.SendDialog(playerid, "SelectClass" + playerid, d);
		}
		
		void ShowSubclassSelectionDialog(int playerid)
		{
			Dialog d = new Dialog();
			List<Widget> widgets = new List<Widget>();
			Widget background = new Widget();
			background.X = 0;
			background.Y = 0;
			background.Width = 800;
			background.Height = 800;
			background.Image = "SelectSubclass";
			widgets.Add(background);
			string[] subclasses = null;
			if (players[playerid].playerclass == PlayerClass.Soldier)
			{
				subclasses = new[] { "Submachine gun", "Shotgun", "Rifle" };
			}
			if (players[playerid].playerclass == PlayerClass.Medic)
			{
				subclasses = new[] { "Pistol" };
			}
			if (players[playerid].playerclass == PlayerClass.Support)
			{
				subclasses = new[] { "Pistol" };
			}
			for (int i = 0; i < subclasses.Length; i++)
			{
				Widget w = new Widget();
				w.Id = "Subclass" + (i + 1).ToString();
				w.Text = string.Format("Press {0} for {1}", i + 1, subclasses[i]);
				w.X = 50 + 275 * i;
				w.Y = 400;
				w.ClickKey = (i + 1).ToString()[0];
				widgets.Add(w);
			}
			d.Width = 800;
			d.Height = 600;
			d.Widgets = widgets.ToArray();
			m.SendDialog(playerid, "SelectSubclass" + playerid, d);
		}
		
		public enum Team
		{
			Blue,
			Green,
			Spectator,
		}
		
		string BlueColor = "&1";
		string GreenColor = "&2";
		string SpectatorColor = "&7";
		string GetTeamColorString(Team team)
		{
			switch (team)
			{
				case Team.Blue:
					return BlueColor;
				case Team.Green:
					return GreenColor;
				case Team.Spectator:
					return SpectatorColor;
			}
			throw new Exception();
		}
		
		void DialogClickSelectTeam(int playerid, string widget)
		{
			if (widget == "Team1")
			{
				m.SendDialog(playerid, "SelectTeam" + playerid, null);
				//if (players[playerid].team == Team.Blue && (!players[playerid].firstteam))
				{
					//return;
				}
				if (players[playerid].team != Team.Blue)
				{
					//Player changed team
					players[playerid].team = Team.Blue;
					players[playerid].kills = 0;
					m.SendMessageToAll(string.Format("{0} joins {1}&f team.", m.GetPlayerName(playerid), BlueColor + " " + "Blue"));
				}
				m.SetPlayerSpectator(playerid, false);
				UpdatePlayerModel(playerid);
				m.EnableFreemove(playerid, false);
				ShowClassSelectionDialog(playerid);
			}
			if (widget == "Team2")
			{
				m.SendDialog(playerid, "SelectTeam" + playerid, null);
				//if (players[playerid].team == Team.Green && (!players[playerid].firstteam))
				{
					//return;
				}
				if (players[playerid].team != Team.Green)
				{
					//Player changed team
					players[playerid].team = Team.Green;
					players[playerid].kills = 0;
					m.SendMessageToAll(string.Format("{0} joins {1}&f team.", m.GetPlayerName(playerid), GreenColor + " " + "Green"));
				}
				m.SetPlayerSpectator(playerid, false);
				UpdatePlayerModel(playerid);
				m.EnableFreemove(playerid, false);
				ShowClassSelectionDialog(playerid);
			}
			if (widget == "Team3")
			{
				m.SendDialog(playerid, "SelectTeam" + playerid, null);
				if (players[playerid].team == Team.Spectator && (!players[playerid].firstteam))
				{
					return;
				}
				players[playerid].team = Team.Spectator;
				players[playerid].kills = 0;
				m.SetPlayerSpectator(playerid, true);
				UpdatePlayerModel(playerid);
				m.EnableFreemove(playerid, true);
				m.SendMessageToAll(string.Format("{0} becomes a &7 spectator&f.", m.GetPlayerName(playerid)));
				ClearInventory(playerid);
			}
			if (widget == "Team1" || widget == "Team2" || widget == "Team3")
			{
				if (!spawnedBot)
				{
					spawnedBot = true;
					//if (System.Diagnostics.Debugger.IsAttached)
					//{
					//    int bot = m.AddBot("bot");
					//    PlayerJoin(bot);
					//    DialogClickSelectTeam(bot, "Team2");
					//    Respawn(bot);
					//}
				}
			}
		}
		bool spawnedBot = false;
		void DialogClickSelectClass(int playerid, string widget)
		{
			if (widget == "Class1")
			{
				players[playerid].playerclass = PlayerClass.Soldier;
				ShowSubclassSelectionDialog(playerid);
			}
			if (widget == "Class2")
			{
				players[playerid].playerclass = PlayerClass.Medic;
				ShowSubclassSelectionDialog(playerid);
			}
			if (widget == "Class3")
			{
				players[playerid].playerclass = PlayerClass.Support;
				ShowSubclassSelectionDialog(playerid);
			}
			if (widget == "Class1" || widget == "Class2" || widget == "Class3")
			{
				m.SendDialog(playerid, "SelectClass" + playerid, null);
			}
		}
		void DialogClickSelectSubclass(int playerid, string widget)
		{
			if (!(widget == "Subclass1" || widget == "Subclass2" || widget == "Subclass3"))
			{
				return;
			}
			
			if (players[playerid].firstteam)
			{
				Respawn(playerid);
			}
			else
			{
				Die(playerid);
			}
			players[playerid].firstteam = false;
			
			m.SendDialog(playerid, "SelectSubclass" + playerid, null);
			
			PlayerClass pclass = players[playerid].playerclass;
			if (pclass == PlayerClass.Soldier)
			{
				if (widget == "Subclass1")
				{
					players[playerid].soldierSubclass = SoldierSubclass.SubmachineGun;
				}
				if (widget == "Subclass2")
				{
					players[playerid].soldierSubclass = SoldierSubclass.Shotgun;
				}
				if (widget == "Subclass3")
				{
					players[playerid].soldierSubclass = SoldierSubclass.Rifle;
				}
			}
			if (pclass == PlayerClass.Medic)
			{
				if (widget == "Subclass1")
				{
					//todo medic subclass
				}
			}
			if (pclass == PlayerClass.Support)
			{
				if (widget == "Subclass1")
				{
					//todo support subclass
				}
			}
			ResetInventoryOnRespawn(playerid);
		}
		
		void ResetInventoryOnRespawn(int playerid)
		{
			ClearInventory(playerid);
			if (players[playerid].team == Team.Spectator)
			{
				//Don't give spectators weapons when they die.
				return;
			}
			
			PlayerClass pclass = players[playerid].playerclass;
			if (pclass == PlayerClass.Soldier)
			{
				SoldierSubclass sclass = players[playerid].soldierSubclass;
				if (sclass == SoldierSubclass.SubmachineGun)
				{
					m.GrabBlock(playerid, m.GetBlockId("SubmachineGun"));
					m.GrabBlock(playerid, m.GetBlockId("Pistol"));
					m.GrabBlock(playerid, m.GetBlockId("Grenade"));
				}
				if (sclass == SoldierSubclass.Shotgun)
				{
					m.GrabBlock(playerid, m.GetBlockId("Shotgun"));
					m.GrabBlock(playerid, m.GetBlockId("Pistol"));
					m.GrabBlock(playerid, m.GetBlockId("Grenade"));
				}
				if (sclass == SoldierSubclass.Rifle)
				{
					m.GrabBlock(playerid, m.GetBlockId("Rifle"));
					m.GrabBlock(playerid, m.GetBlockId("Pistol"));
					m.GrabBlock(playerid, m.GetBlockId("Grenade"));
				}
			}
			if (pclass == PlayerClass.Medic)
			{
				m.GrabBlock(playerid, m.GetBlockId("Pistol"));
				for (int i = 0; i < 4; i++)
				{
					m.GrabBlock(playerid, m.GetBlockId("MedicalKit"));
				}
			}
			if (pclass == PlayerClass.Support)
			{
				m.GrabBlock(playerid, m.GetBlockId("Pistol"));
				for (int i = 0; i < 5; i++)
				{
					m.GrabBlock(playerid, m.GetBlockId("AmmoPack"));
				}
			}
			m.NotifyInventory(playerid);
			Inventory inv = m.GetInventory(playerid);
			for (int i = 0; i < 10; i++)
			{
				Item item = inv.RightHand[i];
				if (item != null && item.ItemClass == ItemClass.Block)
				{
					BlockType block = m.GetBlockType(item.BlockId);
					if (block.IsPistol)
					{
						players[playerid].totalAmmo[item.BlockId] = block.AmmoTotal;
					}
				}
			}
			m.NotifyAmmo(playerid, players[playerid].totalAmmo);
		}
		
		void OnSelectTeamKey(int player, SpecialKey key)
		{
			if (key != SpecialKey.SelectTeam)
			{
				return;
			}
			if (warmode == WarMode.Edit)
			{
				return;
			}
			ShowTeamSelectionDialog(player);
		}
		
		void OnPlayerDeath(int player, DeathReason reason, int sourceID)
		{
			string deathMessage = "";
			switch (reason)
			{
				case DeathReason.FallDamage:
					Die(player);
					deathMessage = string.Format("{0}{1} &7was doomed to fall.", GetTeamColorString(players[player].team), m.GetPlayerName(player));
					break;
				case DeathReason.BlockDamage:
					if (sourceID == m.GetBlockId("Lava"))
					{
						Die(player);
						deathMessage = string.Format("{0}{1} &7thought they could swim in Lava.", GetTeamColorString(players[player].team), m.GetPlayerName(player));
					}
					else if (sourceID == m.GetBlockId("Fire"))
					{
						Die(player);
						deathMessage = string.Format("{0}{1} &7was burned alive.", GetTeamColorString(players[player].team), m.GetPlayerName(player));
					}
					else
					{
						Die(player);
						deathMessage = string.Format("{0}{1} &7was killed by {2}.", GetTeamColorString(players[player].team), m.GetPlayerName(player), m.GetBlockName(sourceID));
					}
					break;
				case DeathReason.Drowning:
					Die(player);
					deathMessage = string.Format("{0}{1} &7tried to breathe under water.", GetTeamColorString(players[player].team), m.GetPlayerName(player));
					break;
				case DeathReason.Explosion:
					if (!EnableTeamkill)
					{
						if (players[sourceID].team == players[player].team)
						{
							break;
						}
					}
					//Check if one of the players is spectator
					if (players[sourceID].team == Team.Spectator || players[player].team == Team.Spectator)
					{
						//Just here for safety. Spectators shouldn't have weapons...
						break;
					}
					//Check if one of the players is dead
					if (players[player].isdead || players[sourceID].isdead)
					{
						break;
					}
					Die(player);
					if (sourceID == player)
					{
						deathMessage = string.Format("{0}{1} &7blew himself up.", GetTeamColorString(players[player].team), m.GetPlayerName(player));
						break;
					}
					if (players[sourceID].team != players[player].team)
					{
						players[sourceID].kills = players[sourceID].kills + 1;
					}
					else
					{
						players[sourceID].kills = players[sourceID].kills - 2;
					}
					if (players[sourceID].team == players[player].team)
					{
						deathMessage = string.Format("{0}{1} &7was blown into pieces by {2}{3}. - {4}TEAMKILL", GetTeamColorString(players[player].team), m.GetPlayerName(player), GetTeamColorString(players[sourceID].team), m.GetPlayerName(sourceID), m.colorError());
					}
					else
					{
						deathMessage = string.Format("{0}{1} &7was blown into pieces by {2}{3}&7.", GetTeamColorString(players[player].team), m.GetPlayerName(player), GetTeamColorString(players[sourceID].team), m.GetPlayerName(sourceID));
					}
					break;
				default:
					Die(player);
					deathMessage = string.Format("{0}{1} &7died.", GetTeamColorString(players[player].team), m.GetPlayerName(player));
					break;
			}
			if (!string.IsNullOrEmpty(deathMessage))
			{
				m.SendMessageToAll(deathMessage);
			}
		}
		
		void Respawn(int playerid)
		{
			int posx = -1;
			int posy = -1;
			int posz = -1;
			switch (players[playerid].team)
			{
				case Team.Blue:
					posx = m.GetMapSizeX() / 2;
					posy = 50;
					break;
				case Team.Green:
					posx = m.GetMapSizeX() / 2;
					posy = m.GetMapSizeY() - 50;
					break;
				case Team.Spectator:
					posx = m.GetMapSizeX() / 2;
					posy = m.GetMapSizeY() / 2;
					break;
			}
			posz = BlockHeight(posx, posy);
			m.SetPlayerPosition(playerid, posx, posy, posz);
			ResetInventoryOnRespawn(playerid);
		}
		
		public int BlockHeight(int x, int y)
		{
			for (int z = m.GetMapSizeZ() - 1; z >= 0; z--)
			{
				if (m.GetBlock(x, y, z) != 0)
				{
					return z + 1;
				}
			}
			return m.GetMapSizeZ() / 2;
		}
		void Shot(int sourceplayer, int block)
		{
			if (!players[sourceplayer].totalAmmo.ContainsKey(block))
			{
				players[sourceplayer].totalAmmo[block] = 0;
			}
			players[sourceplayer].totalAmmo[block] = players[sourceplayer].totalAmmo[block] - 1;
			m.NotifyAmmo(sourceplayer, players[sourceplayer].totalAmmo);
		}
		void Hit(int sourceplayer, int targetplayer, int block, bool head)
		{
			if (!EnableTeamkill)
			{
				if (players[sourceplayer].team == players[targetplayer].team)
				{
					return;
				}
			}
			//Check if one of the players is a spectator
			if (players[sourceplayer].team == Team.Spectator || players[targetplayer].team == Team.Spectator)
			{
				return;
			}
			//Check if one of the players is dead
			if (players[targetplayer].isdead || players[sourceplayer].isdead)
			{
				return;
			}
			{
				float x1 = m.GetPlayerPositionX(sourceplayer);
				float y1 = m.GetPlayerPositionY(sourceplayer);
				float z1 = m.GetPlayerPositionZ(sourceplayer);
				float x2 = m.GetPlayerPositionX(targetplayer);
				float y2 = m.GetPlayerPositionY(targetplayer);
				float z2 = m.GetPlayerPositionZ(targetplayer);
				float dx = x1 - x2;
				float dy = y1 - y2;
				float dz = z1 - z2;
				float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
				dx = (dx / dist) * 0.1f;
				dy = (dy / dist) * 0.1f;
				dz = (dz / dist) * 0.1f;
				m.SendExplosion(targetplayer, dx, dy, dz, true, m.GetBlockType(block).ExplosionRange, m.GetBlockType(block).ExplosionTime);
			}
			int health = m.GetPlayerHealth(targetplayer);
			int dmghead = 50;
			int dmgbody = 15;
			if (m.GetBlockType(block).DamageHead != 0) { dmghead = (int)m.GetBlockType(block).DamageHead; }
			if (m.GetBlockType(block).DamageBody != 0) { dmgbody = (int)m.GetBlockType(block).DamageBody; }
			health -= head ? dmghead : dmgbody;
			if (health <= 0)
			{
				if (players[sourceplayer].team != players[targetplayer].team)
				{
					players[sourceplayer].kills = players[sourceplayer].kills + 1;
				}
				else
				{
					players[sourceplayer].kills = players[sourceplayer].kills - 2;
				}
				Die(targetplayer);
				if (players[sourceplayer].team == players[targetplayer].team)
				{
					m.SendMessageToAll(string.Format("{0} kills {1} - " + m.colorError() + "TEAMKILL", m.GetPlayerName(sourceplayer), m.GetPlayerName(targetplayer)));
					
				}
				else
				{
					m.SendMessageToAll(string.Format("{0} kills {1}", m.GetPlayerName(sourceplayer), m.GetPlayerName(targetplayer)));
				}
			}
			else
			{
				m.SetPlayerHealth(targetplayer, health, m.GetPlayerMaxHealth(targetplayer));
				m.PlaySoundAt((int)m.GetPlayerPositionX(targetplayer),
				              (int)m.GetPlayerPositionY(targetplayer),
				              (int)m.GetPlayerPositionZ(targetplayer), "grunt1.ogg");
			}
		}
		
		void Die(int player)
		{
			m.PlaySoundAt((int)m.GetPlayerPositionX(player),
			              (int)m.GetPlayerPositionY(player),
			              (int)m.GetPlayerPositionZ(player), "death.ogg");
			//Respawn(targetplayer);
			players[player].isdead = true;
			m.SetPlayerHealth(player, m.GetPlayerMaxHealth(player), m.GetPlayerMaxHealth(player));
			m.FollowPlayer(player, player, true);
			UpdatePlayerModel(player);
		}
		
		void RespawnKey(int player, SpecialKey key)
		{
			if (key != SpecialKey.Respawn)
			{
				return;
			}
			if (warmode == WarMode.Edit)
			{
				return;
			}
			if (players[player].isdead)
			{
				return;     //Don't allow dead players to respawn
			}
			m.SendMessage(player, "Respawn.");
			Die(player);
		}
		
		void OnTabKey(int player, SpecialKey key)
		{
			if (key != SpecialKey.TabPlayerList)
			{
				return;
			}
			tabOpen[m.GetPlayerName(player)] = true;
			Dialog d = new Dialog();
			d.IsModal = true;
			List<Widget> widgets = new List<Widget>();
			
			// table alignment
			float tableX = xcenter(m.GetScreenResolution(player)[0], tableWidth);
			float tableY = tableMarginTop;
			
			// text to draw
			string row1 = m.GetServerName();
			row1 = cutText(row1, HeadingFont, tableWidth - 2 * tablePadding);
			
			string row2 = m.GetServerMotd();
			row2 = cutText(row2, SmallFontBold, tableWidth - 2 * tablePadding);
			
			string row3_1 = "IP: " + m.GetServerIp() + ":" + m.GetServerPort();
			string row3_2 = (int)(m.GetPlayerPing(player) * 1000) + "ms";
			
			string row4_1 = "Players: " + m.AllPlayers().Length;
			string row4_2 = "Page: " + (page + 1) + "/" + (pageCount + 1);
			
			string row5_1 = "ID";
			//string row5_2 = "Player";
			//string row5_3 = "Ping";
			
			// row heights
			float row1Height = textHeight(row1, HeadingFont) + 2 * tablePadding;
			float row2Height = textHeight(row2, SmallFontBold) + 2 * tablePadding;
			float row3Height = textHeight(row3_1, SmallFont) + 2 * tablePadding;
			float row4Height = textHeight(row4_1, SmallFont) + 2 * tablePadding;
			float row5Height = textHeight(row5_1, NormalFontBold) + 2 * tablePadding;
			float listEntryHeight = textHeight("Player", NormalFont) + 2 * listEntryPaddingTopBottom;
			
			float heightOffset = 0;
			
			// determine how many entries can be displayed
			tableHeight = m.GetScreenResolution(player)[1] - tableMarginTop - tableMarginBottom;
			float availableEntrySpace = tableHeight - row1Height - row2Height - row3Height - row4Height - (row5Height + tableLineWidth);
			
			int entriesPerPage = (int)(availableEntrySpace / listEntryHeight);
			pageCount = (int)Math.Ceiling((float)(m.AllPlayers().Length / entriesPerPage));
			if (page > pageCount)
			{
				page = 0;
			}
			
			// 1 - heading: Servername
			widgets.Add(Widget.MakeSolid(tableX, tableY, tableWidth, row1Height, Color.DarkGreen.ToArgb()));
			widgets.Add(Widget.MakeText(row1, HeadingFont, tableX + xcenter(tableWidth, textWidth(row1, HeadingFont)), tableY + tablePadding, TEXT_COLOR.ToArgb()));
			heightOffset += row1Height;
			
			// 2 - MOTD
			widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, row2Height, Color.ForestGreen.ToArgb()));
			widgets.Add(Widget.MakeText(row2, SmallFontBold, tableX + xcenter(tableWidth, textWidth(row2, SmallFontBold)), tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
			heightOffset += row2Height;
			
			// 3 - server info: IP Motd Serverping
			widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, row3Height, Color.DarkSeaGreen.ToArgb()));
			// row3_1 - IP align left
			widgets.Add(Widget.MakeText(row3_1, SmallFont, tableX + tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
			// row3_2 - Serverping align right
			widgets.Add(Widget.MakeText(row3_2, SmallFont, tableX + tableWidth - textWidth(row3_2, SmallFont) - tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
			heightOffset += row3Height;
			
			// 4 - infoline: Playercount, Page
			widgets.Add(Widget.MakeSolid(tableX, tableY + heightOffset, tableWidth, row4Height, Color.DimGray.ToArgb()));
			// row4_1 PlayerCount
			widgets.Add(Widget.MakeText(row4_1, SmallFont, tableX + tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
			// row4_2 PlayerCount
			widgets.Add(Widget.MakeText(row4_2, SmallFont, tableX + tableWidth - textWidth(row4_2, SmallFont) - tablePadding, tableY + heightOffset + tablePadding, TEXT_COLOR.ToArgb()));
			heightOffset += row4Height;
			
			Dictionary<Team, List<int>> playersByTeam = new Dictionary<Team, List<int>>();
			playersByTeam[Team.Blue] = new List<int>();
			playersByTeam[Team.Spectator] = new List<int>();
			playersByTeam[Team.Green] = new List<int>();
			int[] AllPlayers = m.AllPlayers();
			foreach (int p in AllPlayers)
			{
				playersByTeam[players[p].team].Add(p);
			}
			Team[] allteams = new Team[] { Team.Blue, Team.Spectator, Team.Green };
			for (int t = 0; t < allteams.Length; t++)
			{
				List<int> players = playersByTeam[allteams[t]];
				players.Sort((a, b) => (this.players[b].kills.CompareTo(this.players[a].kills)));
				for (int i = 0; i < players.Count; i++)
				{
					string s = string.Format("{0} {1}ms {2} kills", m.GetPlayerName(players[i]), (int)(m.GetPlayerPing(players[i]) * 1000), this.players[players[i]].kills);
					widgets.Add(Widget.MakeText(s, NormalFont, tableX + 200 * t, tableY + heightOffset + listEntryHeight * i, Color.White.ToArgb()));
				}
			}
			
			
			var wtab = Widget.MakeSolid(0, 0, 0, 0, 0);
			wtab.ClickKey = '\t';
			wtab.Id = "Tab";
			widgets.Add(wtab);
			var wesc = Widget.MakeSolid(0, 0, 0, 0, 0);
			wesc.ClickKey = (char)27;
			wesc.Id = "Esc";
			widgets.Add(wesc);
			
			d.Width = m.GetScreenResolution(player)[0];
			d.Height = m.GetScreenResolution(player)[1];
			d.Widgets = widgets.ToArray();
			m.SendDialog(player, "PlayerList", d);
		}
		
		
		private int pageCount = 0; //number of pages for player table entries
		private int page = 0; // current displayed page
		
		// fonts
		public readonly Color TEXT_COLOR = Color.Black;
		public DialogFont HeadingFont = new DialogFont("Verdana", 11f, DialogFontStyle.Bold);
		public DialogFont NormalFont = new DialogFont("Verdana", 10f, DialogFontStyle.Regular);
		public DialogFont NormalFontBold = new DialogFont("Verdana", 10f, DialogFontStyle.Bold);
		public DialogFont SmallFont = new DialogFont("Verdana", 8f, DialogFontStyle.Regular);
		public DialogFont SmallFontBold = new DialogFont("Verdana", 8f, DialogFontStyle.Bold);
		
		private float tableMarginTop = 10;
		private float tableMarginBottom = 10;
		private float tableWidth = 500;
		private float tableHeight = 500;
		private float tablePadding = 5;
		private float listEntryPaddingTopBottom = 2;
		//private float tableIdColumnWidth = 50;
		//private float tablePlayerColumnWidth = 400;
		//private float tablePingColumnWidth = 50;
		private float tableLineWidth = 2;
		
		public bool NextPage()
		{
			if (this.page < this.pageCount)
			{
				this.page++;
				return true;
			}
			return false;
		}
		
		public bool PreviousPage()
		{
			if (this.page > 0)
			{
				this.page--;
				return true;
			}
			return false;
		}
		
		private float xcenter(float outerWidth, float innerWidth)
		{
			return (outerWidth / 2 - innerWidth / 2);
		}
		private float ycenter(float outerHeight, float innerHeight)
		{
			return (outerHeight / 2 - innerHeight / 2);
		}
		private float textWidth(string text, DialogFont font)
		{
			return m.MeasureTextSize(text, font)[0];
		}
		private float textHeight(string text, DialogFont font)
		{
			return m.MeasureTextSize(text, font)[1];
		}
		private string cutText(string text, DialogFont font, float maxWidth)
		{
			while (textWidth(text, font) > maxWidth && text.Length > 3)
			{
				text = text.Remove(text.Length - 1);
			}
			return text;
		}
		
		void OnTabResponse(int player, string widgetid)
		{
			if (widgetid == "Tab" || widgetid == "Esc")
			{
				m.SendDialog(player, "PlayerList", null);
				tabOpen.Remove(m.GetPlayerName(player));
			}
		}
		
		Dictionary<string, bool> tabOpen = new Dictionary<string, bool>();
		
		void UpdateTab()
		{
			foreach (var k in new Dictionary<string, bool>(tabOpen))
			{
				foreach (int p in m.AllPlayers())
				{
					if (k.Key == m.GetPlayerName(p))
					{
						OnTabKey(p, SpecialKey.TabPlayerList);
						goto nexttab;
					}
				}
				//player disconnected
				tabOpen.Remove(k.Key);
			nexttab:
				;
			}
		}
		
		void UpdatePlayerModel(int player)
		{
			Inventory inv = m.GetInventory(player);
			Item item = inv.RightHand[m.GetActiveMaterialSlot(player)];
			int blockid = 0;
			if (item != null && item.ItemClass == ItemClass.Block) { blockid = item.BlockId; }
			string model = "playerwar.txt";
			if (blockid == m.GetBlockId("Pistol")) { model = "playerwarpistol.txt"; }
			if (blockid == m.GetBlockId("SubmachineGun")) { model = "playerwarsubmachinegun.txt"; }
			if (blockid == m.GetBlockId("Shotgun")) { model = "playerwarshotgun.txt"; }
			if (blockid == m.GetBlockId("Rifle")) { model = "playerwarrifle.txt"; }
			if (players[player].isdead) { model = "playerwardead.txt"; }
			m.SetPlayerHeight(player, 2.2f, 2.4f);
			Team team = players[player].team;
			switch (team)
			{
				case Team.Blue:
					m.SetPlayerModel(player, model, "playerblue.png");
					break;
				case Team.Green:
					m.SetPlayerModel(player, model, "playergreen.png");
					break;
				case Team.Spectator:
					m.SetPlayerModel(player, model, "mineplayer.png");
					break;
			}
		}
		
		void UpdateRespawnTimer()
		{
			int[] allplayers = m.AllPlayers();
			int secondsToRespawn = (int)((CurrentRespawnTime + RespawnTime) - DateTime.UtcNow).TotalSeconds;
			if (secondsToRespawn <= 0)
			{
				for (int i = 0; i < allplayers.Length; i++)
				{
					int p = allplayers[i];
					if (!players.ContainsKey(p))
					{
						//Skip this player as he hasn't joined yet
						continue;
					}
					if (players[p].isdead)
					{
						m.SendDialog(p, "RespawnCountdown" + p, null);
						m.FollowPlayer(p, -1, false);
						Respawn(p);
						players[p].isdead = false;
						UpdatePlayerModel(p);
					}
				}
				CurrentRespawnTime = DateTime.UtcNow;
			}
			for (int i = 0; i < allplayers.Length; i++)
			{
				int p = allplayers[i];
				if (!players.ContainsKey(p))
				{
					//Skip this player as he hasn't joined yet
					continue;
				}
				if (players[p].isdead)
				{
					Dialog d = new Dialog();
					d.IsModal = false;
					string text = secondsToRespawn.ToString();
					DialogFont f = new DialogFont("Verdana", 60f, DialogFontStyle.Regular);
					Widget w = Widget.MakeText(text, f, -m.MeasureTextSize(text, f)[0] / 2, -200, Color.Red.ToArgb());
					d.Widgets = new Widget[1];
					d.Widgets[0] = w;
					m.SendDialog(p, "RespawnCountdown" + p, d);
				}
			}
		}
		
		void UpdateMedicalKitAmmoPack()
		{
			if (warmode == WarMode.Edit)
			{
				return;
			}
			int[] allplayers = m.AllPlayers();
			int medicalkit = m.GetBlockId("MedicalKit");
			int ammopack = m.GetBlockId("AmmoPack");
			foreach (int p in allplayers)
			{
				int px = (int)m.GetPlayerPositionX(p);
				int py = (int)m.GetPlayerPositionY(p);
				int pz = (int)m.GetPlayerPositionZ(p);
				if (m.IsValidPos(px, py, pz))
				{
					int block = m.GetBlock(px, py, pz);
					if (block == medicalkit)
					{
						int health = m.GetPlayerHealth(p);
						int maxhealth = m.GetPlayerMaxHealth(p);
						if (health >= maxhealth)
						{
							continue;
						}
						health += 30;
						if (health > maxhealth)
						{
							health = maxhealth;
						}
						m.SetPlayerHealth(p, health, maxhealth);
						m.SetBlock(px, py, pz, 0);
						//m.PlaySoundAt((int)m.GetPlayerPositionX(targetplayer),
						//    (int)m.GetPlayerPositionY(targetplayer),
						//    (int)m.GetPlayerPositionZ(targetplayer), "heal.ogg");
					}
					if (block == ammopack)
					{
						foreach (var k in new List<int>(players[p].totalAmmo.Keys))
						{
							int ammo = 0;
							if (players[p].totalAmmo.ContainsKey(k))
							{
								ammo = players[p].totalAmmo[k];
							}
							ammo += m.GetBlockType(k).AmmoTotal / 3;
							if (ammo > m.GetBlockType(k).AmmoTotal)
							{
								ammo = m.GetBlockType(k).AmmoTotal;
							}
							players[p].totalAmmo[k] = ammo;
						}
						m.NotifyAmmo(p, players[p].totalAmmo);
						m.SetBlock(px, py, pz, 0);
					}
				}
			}
		}
		
		string OnChat(int player, string message, bool toteam)
		{
			if (warmode == WarMode.Edit)
			{
				return message;
			}
			int[] allplayers = m.AllPlayers();
			string sender = m.GetPlayerName(player);
			string senderColorString = GetTeamColorString(players[player].team);
			string s = message;
			if (players[player].team == Team.Spectator)
			{
				toteam = true;
			}
			if (toteam)
			{
				s = GetTeamColorString(players[player].team) + s;
			}
			foreach (int p in allplayers)
			{
				if (toteam)
				{
					if (!(players[p].team == players[player].team || players[p].team == Team.Spectator))
					{
						continue;
					}
				}
				m.SendMessage(p, senderColorString + sender + "&f: " + s);
			}
			if (players[player].team == Team.Spectator)
			{
				Console.WriteLine("[Spectator] " + sender + ": " + s);
			}
			else
			{
				if (toteam)
				{
					if (players[player].team == Team.Blue)
						Console.WriteLine("[Blue] " + sender + ": " + s);
					else
						Console.WriteLine("[Green] " + sender + ": " + s);
				}
				else
					Console.WriteLine("[Players] " + sender + ": " + s);
			}
			m.LogChat(senderColorString + sender + "&f: " + s);
			return null;
		}
		
		bool OnCommand(int player, string command, string arguments)
		{
			if (command == "mode")
			{
				if (!m.PlayerHasPrivilege(player, "mode"))
				{
					m.SendMessage(player, m.colorError() + "No privilege: mode");
					return true;
				}
				if (arguments == "edit")
				{
					warmode = WarMode.Edit;
					m.LoadWorld(m.CurrentWorld());
					m.SetWorldDatabaseReadOnly(false);
					Restart();
				}
				else if (arguments == "tdm")
				{
					warmode = WarMode.TeamDeathmatch;
					m.LoadWorld(m.CurrentWorld());
					m.SetWorldDatabaseReadOnly(true);
					Restart();
				}
				else
				{
					m.SendMessage(player, m.colorError() + "Usage: /mode [edit/tdm]");
				}
				return true;
			}
			return false;
		}
		
		void Restart()
		{
			int[] allplayers = m.AllPlayers();
			foreach (int p in allplayers)
			{
				PlayerJoin(p);
			}
		}
		
		void OnBuild(int player, int x, int y, int z)
		{
			if (m.GetBlockNameAt(x, y, z) == "Water")
			{
				m.SetBlock(x, y, z, 0);
			}
		}
	}
}
