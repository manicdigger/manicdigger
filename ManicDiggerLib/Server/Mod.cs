using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using Jint.Delegates;
using System.Net;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ManicDigger
{
	public class ModManager1 : ModManager
	{
		public int GetMaxBlockTypes()
		{
			return GlobalVar.MAX_BLOCKTYPES;
		}

		public void SetBlockType(int id, string name, BlockType block)
		{
			if (block.Sounds == null)
			{
				block.Sounds = defaultSounds;
			}
			server.SetBlockType(id, name, block);
		}

		public void SetBlockType(string name, BlockType block)
		{
			if (block.Sounds == null)
			{
				block.Sounds = defaultSounds;
			}
			server.SetBlockType(name, block);
		}

		public int GetBlockId(string name)
		{
			for (int i = 0; i < server.BlockTypes.Length; i++)
			{
				if (server.BlockTypes[i].Name == name)
				{
					return i;
				}
			}
			return -1;
			//throw new Exception(name);
		}

		public void AddToCreativeInventory(string blockType)
		{
			int id = GetBlockId(blockType);
			if (id == -1)
			{
				throw new Exception(blockType);
			}
			server.BlockTypes[id].IsBuildable = true;
			server.d_Data.UseBlockType(server.platform, id, BlockTypeConverter.GetBlockType(server.BlockTypes[id]));
		}

		public void RegisterOnBlockBuild(ModDelegates.BlockBuild f)
		{
			server.modEventHandlers.onbuild.Add(f);
		}

		public void RegisterOnBlockDelete(ModDelegates.BlockDelete f)
		{
			server.modEventHandlers.ondelete.Add(f);
		}

		public void RegisterOnBlockUse(ModDelegates.BlockUse f)
		{
			server.modEventHandlers.onuse.Add(f);
		}

		public void RegisterOnBlockUseWithTool(ModDelegates.BlockUseWithTool f)
		{
			server.modEventHandlers.onusewithtool.Add(f);
		}

		public int GetMapSizeX() { return server.d_Map.MapSizeX; }
		public int GetMapSizeY() { return server.d_Map.MapSizeY; }
		public int GetMapSizeZ() { return server.d_Map.MapSizeZ; }

		public int GetBlock(int x, int y, int z)
		{
			return server.d_Map.GetBlock(x, y, z);
		}

		public string GetBlockName(int blockType)
		{
			return server.BlockTypes[blockType].Name;
		}

		public string GetBlockNameAt(int x, int y, int z)
		{
			return GetBlockName(GetBlock(x, y, z));
		}

		public void SetBlock(int x, int y, int z, int tileType)
		{
			server.SetBlockAndNotify(x, y, z, tileType);
		}

		private Server server;
		internal void Start(Server server)
		{
			this.server = server;
		}

		public void SetSunLevels(int[] sunLevels)
		{
			server.SetSunLevels(sunLevels);
		}

		public void SetLightLevels(float[] lightLevels)
		{
			server.SetLightLevels(lightLevels);
		}
		
		const string recipeError = "Recipe error:";

		public void AddCraftingRecipe(string output, int outputAmount, string Input0, int Input0Amount)
		{
			if (GetBlockId(output) == -1) { Console.WriteLine(recipeError + output); return; }
			if (GetBlockId(Input0) == -1) { Console.WriteLine(recipeError + Input0); return; }
			CraftingRecipe r = new CraftingRecipe();
			r.ingredients = new Ingredient[]
			{
				new Ingredient(){Type=GetBlockId(Input0), Amount=Input0Amount},
			};
			r.output = new Ingredient() { Type = GetBlockId(output), Amount = outputAmount };
			server.craftingrecipes.Add(r);
		}

		public void AddCraftingRecipe2(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount)
		{
			if (GetBlockId(output) == -1) { Console.WriteLine(recipeError + output); return; }
			if (GetBlockId(Input0) == -1) { Console.WriteLine(recipeError + Input0); return; }
			if (GetBlockId(Input1) == -1) { Console.WriteLine(recipeError + Input1); return; }
			CraftingRecipe r = new CraftingRecipe();
			r.ingredients = new Ingredient[]
			{
				new Ingredient(){Type=GetBlockId(Input0), Amount=Input0Amount},
				new Ingredient(){Type=GetBlockId(Input1), Amount=Input1Amount},
			};
			r.output = new Ingredient() { Type = GetBlockId(output), Amount = outputAmount };
			server.craftingrecipes.Add(r);
		}

		public void AddCraftingRecipe3(string output, int outputAmount, string Input0, int Input0Amount, string Input1, int Input1Amount, string Input2, int Input2Amount)
		{
			if (GetBlockId(output) == -1) { Console.WriteLine(recipeError + output); return; }
			if (GetBlockId(Input0) == -1) { Console.WriteLine(recipeError + Input0); return; }
			if (GetBlockId(Input1) == -1) { Console.WriteLine(recipeError + Input1); return; }
			if (GetBlockId(Input2) == -1) { Console.WriteLine(recipeError + Input2); return; }
			CraftingRecipe r = new CraftingRecipe();
			r.ingredients = new Ingredient[]
			{
				new Ingredient(){Type=GetBlockId(Input0), Amount=Input0Amount},
				new Ingredient(){Type=GetBlockId(Input1), Amount=Input1Amount},
				new Ingredient(){Type=GetBlockId(Input2), Amount=Input2Amount},
			};
			r.output = new Ingredient() { Type = GetBlockId(output), Amount = outputAmount };
			server.craftingrecipes.Add(r);
		}

		public void SetString(string language, string id, string translation)
		{
			server.language.Override(language, id, translation);
		}

		public string GetString(string id)
		{
			//Returns string depending on server language
			return server.language.Get(id);
		}

		public bool IsValidPos(int x, int y, int z)
		{
			return MapUtil.IsValidPos(server.d_Map, x, y, z);
		}

		public void RegisterTimer(ManicDigger.Action a, double interval)
		{
			server.timers[new Timer() { INTERVAL = interval }] = delegate { a(); };
		}

		public void PlaySoundAt(int posx, int posy, int posz, string sound)
		{
			server.PlaySoundAt(posx, posy, posz, sound);
		}

		public void PlaySoundAt(int x, int y, int z, string sound, int range)
		{
			server.PlaySoundAt(x, y, z, sound, range);
		}

		public int NearestPlayer(int x, int y, int z)
		{
			int closeplayer = -1;
			int closedistance = -1;
			foreach (var k in server.clients)
			{
				int distance = server.DistanceSquared(new Vector3i((int)k.Value.PositionMul32GlX / 32, (int)k.Value.PositionMul32GlZ / 32, (int)k.Value.PositionMul32GlY / 32), new Vector3i(x, y, z));
				if (closedistance == -1 || distance < closedistance)
				{
					closedistance = distance;
					closeplayer = k.Key;
				}
			}
			return closeplayer;
		}

		public void GrabBlock(int player, int block)
		{
			GrabBlocks(player, block, 1);
		}

		public void GrabBlocks(int player, int block, int amount)
		{
			Inventory inventory = server.GetPlayerInventory(server.GetClient(player).playername).Inventory;

			var item = new Item();
			item.ItemClass = ItemClass.Block;
			item.BlockCount = amount;
			item.BlockId = server.d_Data.WhenPlayerPlacesGetsConvertedTo()[block];
			server.GetInventoryUtil(inventory).GrabItem(item, 0);
		}

		public bool PlayerHasPrivilege(int player, string privilege)
		{
			return server.PlayerHasPrivilege(player, privilege);
		}

		public bool IsCreative()
		{
			return server.config.IsCreative;
		}

		public bool IsBlockFluid(int block)
		{
			return server.BlockTypes[block].IsFluid();
		}

		public void NotifyInventory(int player)
		{
			server.GetClient(player).IsInventoryDirty = true;
			server.NotifyInventory(player);
		}

		public string colorError()
		{
			return server.colorError;
		}

		public void SendMessage(int player, string p)
		{
			server.SendMessage(player, p);
		}

		public void RegisterPrivilege(string p)
		{
			// Add to list of all available privileges on server
			if (!server.all_privileges.Contains(p))
			{
				server.all_privileges.Add(p);
			}
			// Direct modification of console client as mods are loaded after privileges are assigned
			if (!server.serverConsoleClient.privileges.Contains(p))
			{
				server.serverConsoleClient.privileges.Add(p);
			}
		}

		public void RegisterOnBlockUpdate(ModDelegates.BlockUpdate f)
		{
			server.modEventHandlers.blockticks.Add(f);
		}

		public bool IsTransparentForLight(int p)
		{
			return Server.IsTransparentForLight(server.BlockTypes[p]);
		}

		public void RegisterWorldGenerator(ModDelegates.WorldGenerator f)
		{
			server.modEventHandlers.getchunk.Add(f);
		}

		public void RegisterOptionBool(string optionname, bool default_)
		{
			modoptions[optionname] = default_;
		}

		Dictionary<string, object> modoptions = new Dictionary<string, object>();

		public int GetChunkSize()
		{
			return Server.chunksize;
		}

		public object GetOption(string optionname)
		{
			return modoptions[optionname];
		}

		public int GetSeed()
		{
			return server.Seed;
		}

		public int Index3d(int x, int y, int h, int sizex, int sizey)
		{
			return (h * sizey + y) * sizex + x;
		}

		public void RegisterPopulateChunk(ModDelegates.PopulateChunk f)
		{
			server.modEventHandlers.populatechunk.Add(f);
		}

		public void SetDefaultSounds(SoundSet defaultSounds)
		{
			this.defaultSounds = defaultSounds;
		}
		SoundSet defaultSounds;

		public byte[] GetGlobalData(string name)
		{
			if (server.moddata.ContainsKey(name))
			{
				return server.moddata[name];
			}
			return null;
		}

		public void SetGlobalData(string name, byte[] value)
		{
			server.moddata[name] = value;
		}

		public void RegisterOnLoad(Action f)
		{
			server.onload.Add(f);
		}

		public void RegisterOnSave(Action f)
		{
			server.onsave.Add(f);
		}

		public void RegisterOnCommand(ModDelegates.Command f)
		{
			server.modEventHandlers.oncommand.Add(f);
		}

		public string GetPlayerIp(int player)
		{
			return (server.GetClient(player).socket.RemoteEndPoint()).AddressToString();
		}

		public string GetPlayerName(int player)
		{
			return server.GetClient(player).playername;
		}

		public List<string> required = new List<string>();

		public void RequireMod(string modname)
		{
			required.Add(modname);
		}

		public void SetGlobalDataNotSaved(string name, object value)
		{
			notsaved[name] = value;
		}

		public object GetGlobalDataNotSaved(string name)
		{
			if (!notsaved.ContainsKey(name))
			{
				return null;
			}
			return notsaved[name];
		}
		Dictionary<string, object> notsaved = new Dictionary<string, object>();

		public void SendMessageToAll(string message)
		{
			server.SendMessageToAll(message);
		}

		public void RegisterCommandHelp(string command, string help)
		{
			server.commandhelps[command] = help;
		}

		public void AddToStartInventory(string blocktype, int amount)
		{
			server.d_Data.StartInventoryAmount()[GetBlockId(blocktype)] = amount;
		}

		public long GetCurrentTick()
		{
			return server.GetSimulationCurrentFrame();
		}

		public void SetDaysPerYear(int days)
		{
			if (days > 0)
			{
				server.GetTime().DaysPerYear = days;
			}
			else
			{
				throw new ArgumentOutOfRangeException("The number of days per year must be greater than 0!");
			}
		}

		public int GetDaysPerYear()
		{
			return server.GetTime().DaysPerYear;
		}

		public int GetHour()
		{
			return server.GetTime().Hour;
		}

		public double GetTotalHours()
		{
			return server.GetTime().HourTotal;
		}

		public int GetDay()
		{
			return server.GetTime().Day;
		}

		public double GetTotalDays()
		{
			return server.GetTime().DaysTotal;
		}

		public int GetYear()
		{
			return server.GetTime().Year;
		}

		public int GetSeason()
		{
			return server.GetTime().Season;
		}

		public void UpdateBlockTypes()
		{
			foreach (var k in server.clients)
			{
				server.SendBlockTypes(k.Key);
			}
		}

		public double GetGameDayRealHours()
		{
			double nSecondsPerDay = TimeSpan.FromDays(1).TotalSeconds;

			int nSpeed = server.GetTime().SpeedOfTime;
			double nSeconds = nSecondsPerDay / nSpeed;

			double nHours = TimeSpan.FromSeconds(nSeconds).TotalHours;

			return nHours;
		}

		public void SetGameDayRealHours(double hours)
		{
			double nSecondsPerDay = TimeSpan.FromDays(1).TotalSeconds;

			double nSecondsGiven = TimeSpan.FromHours(hours).TotalSeconds;

			server.GetTime().SpeedOfTime = (int)(nSecondsPerDay / nSecondsGiven);
		}

		public void EnableShadows(bool value)
		{
			server.enableshadows = value;
		}

		public float GetPlayerPositionX(int player)
		{
			return (float)server.GetClient(player).PositionMul32GlX / 32;
		}

		public float GetPlayerPositionY(int player)
		{
			return (float)server.GetClient(player).PositionMul32GlZ / 32;
		}

		public float GetPlayerPositionZ(int player)
		{
			return (float)server.GetClient(player).PositionMul32GlY / 32;
		}

		public void SetPlayerPosition(int player, float x, float y, float z)
		{
			ServerEntityPositionAndOrientation pos;
			if (server.clients[player].positionOverride == null)
			{
				//No position override so far. Clone from player position
				pos = server.clients[player].entity.position.Clone();
			}
			else
			{
				//Position has already been modified. Clone from override to prevent data loss
				pos = server.clients[player].positionOverride.Clone();
			}
			pos.x = x;
			pos.y = z;
			pos.z = y;
			server.clients[player].positionOverride = pos;
		}

		public int GetPlayerHeading(int player)
		{
			return server.GetClient(player).positionheading;
		}

		public int GetPlayerPitch(int player)
		{
			return server.GetClient(player).positionpitch;
		}

		public int GetPlayerStance(int player)
		{
			return server.GetClient(player).stance;
		}

		public void SetPlayerOrientation(int player, int heading, int pitch, int stance)
		{
			ServerEntityPositionAndOrientation pos;
			if (server.clients[player].positionOverride == null)
			{
				//No position override so far. Clone from player position
				pos = server.clients[player].entity.position.Clone();
			}
			else
			{
				//Position has already been modified. Clone from override to prevent data loss
				pos = server.clients[player].positionOverride.Clone();
			}
			pos.heading = (byte)heading;
			pos.pitch = (byte)pitch;
			pos.stance = (byte)stance;
			server.clients[player].positionOverride = pos;
		}

		public int[] AllPlayers()
		{
			List<int> players = new List<int>();
			foreach (var k in server.clients)
			{
				players.Add(k.Key);
			}
			return players.ToArray();
		}

		public void SetPlayerAreaSize(int size)
		{
			server.playerareasize = size;
			server.centerareasize = size / 2;
			server.drawdistance = size / 2;
		}

		public bool IsSinglePlayer()
		{
			return server.IsSinglePlayer;
		}

		public void AddPermissionArea(int x1, int y1, int z1, int x2, int y2, int z2, int permissionLevel)
		{
			AreaConfig area = new AreaConfig();
			area.Level = permissionLevel;
			area.Coords = string.Format("{0},{1},{2},{3},{4},{5}", x1, y1, z1, x2, y2, z2);
			server.config.Areas.Add(area);
			server.configNeedsSaving = true;
		}

		public void RemovePermissionArea(int x1, int y1, int z1, int x2, int y2, int z2)
		{
			for (int i = server.config.Areas.Count - 1; i >= 0; i--)
			{
				string coords = string.Format("{0},{1},{2},{3},{4},{5}", x1, y1, z1, x2, y2, z2);
				if (server.config.Areas[i].Coords == coords)
				{
					server.config.Areas.RemoveAt(i);
					server.configNeedsSaving = true;
				}
			}
		}

		public int GetPlayerPermissionLevel(int player)
		{
			return server.clients[player].clientGroup.Level;
		}

		public void SetCreative(bool value)
		{
			server.config.IsCreative = value;
		}

		public void SetWorldSize(int x, int y, int z)
		{
			server.d_Map.Reset(x, y, z);
		}

		public void RegisterOnPlayerJoin(ModDelegates.PlayerJoin a)
		{
			server.modEventHandlers.onplayerjoin.Add(a);
		}

		public void RegisterOnPlayerLeave(ModDelegates.PlayerLeave a)
		{
			server.modEventHandlers.onplayerleave.Add(a);
		}

		public void RegisterOnPlayerDisconnect(ModDelegates.PlayerDisconnect a)
		{
			server.modEventHandlers.onplayerdisconnect.Add(a);
		}

		public void RegisterOnPlayerChat(ModDelegates.PlayerChat a)
		{
			server.modEventHandlers.onplayerchat.Add(a);
		}

		public void RegisterOnPlayerDeath(ModDelegates.PlayerDeath a)
		{
			server.modEventHandlers.onplayerdeath.Add(a);
		}

		public int[] GetScreenResolution(int player)
		{
			return server.clients[player].WindowSize;
		}

		public void SendDialog(int player, string id, Dialog dialog)
		{
			server.SendDialog(player, id, dialog);
		}

		public void RegisterOnDialogClick(ModDelegates.DialogClick a)
		{
			server.modEventHandlers.ondialogclick.Add(a);
		}

		public void SetPlayerModel(int player, string model, string texture)
		{
			server.clients[player].Model = model;
			server.clients[player].Texture = texture;
			server.PlayerEntitySetDirty(player);
		}
		public void RenderHint(RenderHint hint)
		{
			server.RenderHint = hint;
		}
		public void EnableFreemove(int player, bool enable)
		{
			server.SendFreemoveState(player, enable);
		}

		public int GetPlayerHealth(int player)
		{
			string name = GetPlayerName(player);
			return server.GetPlayerStats(name).CurrentHealth;
		}

		public int GetPlayerMaxHealth(int player)
		{
			string name = GetPlayerName(player);
			return server.GetPlayerStats(name).MaxHealth;
		}

		public void SetPlayerHealth(int player, int health, int maxhealth)
		{
			string name = GetPlayerName(player);
			server.GetPlayerStats(name).CurrentHealth = health;
			server.GetPlayerStats(name).MaxHealth = maxhealth;
			server.clients[player].IsPlayerStatsDirty = true;
			server.NotifyPlayerStats(player);
		}

		public int GetPlayerOxygen(int player)
		{
			string name = GetPlayerName(player);
			return server.GetPlayerStats(name).CurrentOxygen;
		}

		public int GetPlayerMaxOxygen(int player)
		{
			string name = GetPlayerName(player);
			return server.GetPlayerStats(name).MaxOxygen;
		}

		public void SetPlayerOxygen(int player, int oxygen, int maxoxygen)
		{
			string name = GetPlayerName(player);
			server.GetPlayerStats(name).CurrentOxygen = oxygen;
			server.GetPlayerStats(name).MaxOxygen = maxoxygen;
			server.clients[player].IsPlayerStatsDirty = true;
			server.NotifyPlayerStats(player);
		}

		public void RegisterOnWeaponHit(ModDelegates.WeaponHit a)
		{
			server.modEventHandlers.onweaponhit.Add(a);
		}

		public void RegisterOnSpecialKey(ModDelegates.SpecialKey1 a)
		{
			server.modEventHandlers.onspecialkey.Add(a);
		}

		public float[] GetDefaultSpawnPosition(int player)
		{
			Vector3i pos = server.GetPlayerSpawnPositionMul32(player);
			return new float[3] { (float)pos.x / 32, (float)pos.z / 32, (float)pos.y / 32 };
		}

		public int[] GetDefaultSpawnPosition()
		{
			return new int[3] { server.defaultPlayerSpawn.x, server.defaultPlayerSpawn.y, server.defaultPlayerSpawn.z };
		}

		public void SetDefaultSpawnPosition(int x, int y, int z)
		{
			if (IsValidPos(x, y, z))
			{
				// Will fail for numbers it cannot parse. Should not happen due to previous check.
				server.serverClient.DefaultSpawn.Coords = string.Format("{0},{1},{2}", x, y, z);
				server.defaultPlayerSpawn.x = x;
				server.defaultPlayerSpawn.y = y;
				server.defaultPlayerSpawn.z = z;
				// Mark ServerClient as dirty for saving
				server.serverClientNeedsSaving = true;
			}
			else
			{
				Console.WriteLine("[Mod API] Invalid default spawn position given!");
			}
		}

		public string GetServerName()
		{
			return server.config.Name;
		}

		public string GetServerMotd()
		{
			return server.config.Motd;
		}

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		static bool checkedIsArm;
		static bool isArm;

		public static bool IsArm
		{
			get
			{
				if (Environment.OSVersion.Platform != PlatformID.Unix)
					return false;

				if (!checkedIsArm)
				{
					IntPtr buf = Marshal.AllocHGlobal(8192);
					if (uname(buf) == 0)
					{
						// todo
						for (int i = 0; i < 8192 - 3; i++)
						{
							if (Marshal.ReadByte(new IntPtr(buf.ToInt64() + i + 0)) == 'a'
							    && Marshal.ReadByte(new IntPtr(buf.ToInt64() + i + 1)) == 'r'
							    && Marshal.ReadByte(new IntPtr(buf.ToInt64() + i + 2)) == 'm')
							{
								isArm = true;
							}
						}
					}
					Marshal.FreeHGlobal(buf);
					checkedIsArm = true;
				}

				return isArm;
			}
		}

		public float[] MeasureTextSize(string text, DialogFont font)
		{
			if (IsArm)
			{
				// fixes crash
				return new float[] { text.Length * 1f * font.Size, 1.7f * font.Size };
			}
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '&')
				{
					if (i + 1 < text.Length && isCharHex(text[i + 1]))
					{
						i++;
					}
					else
					{
						builder.Append(text[i]);
					}
				}
				else
				{
					builder.Append(text[i]);
				}
			}
			using (Bitmap bmp = new Bitmap(1, 1))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					SizeF size = g.MeasureString(builder.ToString(), new System.Drawing.Font(font.FamilyName, font.Size, (FontStyle)font.FontStyle), new PointF(0, 0), new StringFormat(StringFormatFlags.MeasureTrailingSpaces));
					return new float[] { size.Width, size.Height };
				}
			}
		}

		private bool isCharHex(char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
		}

		public string GetServerIp()
		{
			return "!SERVER_IP!";
		}

		public string GetServerPort()
		{
			return "!SERVER_PORT!";
		}

		public float GetPlayerPing(int player)
		{
			return server.clients[player].LastPing;
		}

		public int AddBot(string name)
		{
			int id = server.GenerateClientId();
			ClientOnServer c = new ClientOnServer();
			c.Id = id;
			c.IsBot = true;
			c.playername = name;
			server.clients[id] = c;
			c.state = ClientStateOnServer.Playing;
			DummyNetwork network = new DummyNetwork() { ClientReceiveBufferLock = new MonitorObject(), ServerReceiveBufferLock = new MonitorObject() };
			c.socket = new DummyNetConnection() { network = network, platform = new GamePlatformNative() };
			c.Ping.SetTimeoutValue(int.MaxValue);
			c.chunksseen = new bool[server.d_Map.MapSizeX / Server.chunksize
			                        * server.d_Map.MapSizeY / Server.chunksize * server.d_Map.MapSizeZ / Server.chunksize];
			c.AssignGroup(server.defaultGroupRegistered);
			server.PlayerEntitySetDirty(id);
			return id;
		}

		public bool IsBot(int player)
		{
			return server.clients[player].IsBot;
		}

		public void SetPlayerHeight(int player, float eyeheight, float modelheight)
		{
			server.clients[player].EyeHeight = eyeheight;
			server.clients[player].ModelHeight = modelheight;
			server.PlayerEntitySetDirty(player);
		}

		public void DisablePrivilege(string privilege)
		{
			server.disabledprivileges[privilege] = true;
		}

		public void RegisterChangedActiveMaterialSlot(ModDelegates.ChangedActiveMaterialSlot a)
		{
			server.modEventHandlers.changedactivematerialslot.Add(a);
		}

		public Inventory GetInventory(int player)
		{
			return server.GetPlayerInventory(server.clients[player].playername).Inventory;
		}

		public int GetActiveMaterialSlot(int player)
		{
			return server.clients[player].ActiveMaterialSlot;
		}

		public void FollowPlayer(int player, int target, bool tpp)
		{
			server.SendPacketFollow(player, target, tpp);
		}

		public void SetPlayerSpectator(int player, bool isSpectator)
		{
			server.clients[player].IsSpectator = isSpectator;
		}

		public bool IsPlayerSpectator(int player)
		{
			return server.clients[player].IsSpectator;
		}

		public BlockType GetBlockType(int block)
		{
			return server.BlockTypes[block];
		}

		public void NotifyAmmo(int player, Dictionary<int, int> totalAmmo)
		{
			server.SendAmmo(player, totalAmmo);
		}

		public void RegisterOnWeaponShot(ModDelegates.WeaponShot a)
		{
			server.modEventHandlers.onweaponshot.Add(a);
		}

		public void LogChat(string s)
		{
			server.ChatLog(s);
		}

		public void EnableExtraPrivilegeToAll(string privilege, bool enable)
		{
			if (enable)
			{
				server.extraPrivileges[privilege] = true;
			}
			else
			{
				server.extraPrivileges.Remove(privilege);
			}
		}

		public void LogServerEvent(string serverEvent)
		{
			server.ServerEventLog(serverEvent);
		}


		public void RegisterOnLoadWorld(ModDelegates.LoadWorld a)
		{
			server.modEventHandlers.onloadworld.Add(a);
		}

		public void SetWorldDatabaseReadOnly(bool readOnly)
		{
			server.d_ChunkDb.SetReadOnly(readOnly);
		}

		public string CurrentWorld()
		{
			return server.GetSaveFilename();
		}

		public void LoadWorld(string filename)
		{
			server.LoadDatabase(filename);
		}

		public string[] GetModPaths()
		{
			return server.ModPaths.ToArray();
		}

		public void SendExplosion(int player, float x, float y, float z, bool relativeposition, float range, float time)
		{
			server.SendExplosion(player, x, y, z, relativeposition, range, time);
		}

		public void DisconnectPlayer(int player)
		{
			server.KillPlayer(player);
		}

		public void DisconnectPlayer(int player, string message)
		{
			server.SendPacket(player, ServerPackets.DisconnectPlayer(message));
			server.KillPlayer(player);
		}

		public string GetGroupColor(int player)
		{
			return server.GetGroupColor(player);
		}

		public string GetGroupName(int player)
		{
			return server.GetGroupName(player);
		}

		public void InstallHttpModule(string name, ManicDigger.Func<string> description, FragLabs.HTTP.IHttpModule module)
		{
			server.InstallHttpModule(name, description, module);
		}

		public int GetMaxPlayers()
		{
			return server.config.MaxClients;
		}

		public ServerClient GetServerClient()
		{
			return server.serverClient;
		}

		public long TotalReceivedBytes()
		{
			return server.TotalReceivedBytes;
		}

		public long TotalSentBytes()
		{
			return server.TotalSentBytes;
		}

		public void SetPlayerNameColor(int player, string color)
		{
			if (color.Equals("&0") || color.Equals("&1") || color.Equals("&2") || color.Equals("&3") ||
			    color.Equals("&4") || color.Equals("&5") || color.Equals("&6") || color.Equals("&7") ||
			    color.Equals("&8") || color.Equals("&9") || color.Equals("&a") || color.Equals("&b") ||
			    color.Equals("&c") || color.Equals("&d") || color.Equals("&e") || color.Equals("&f"))
			{
				server.clients[player].displayColor = color;
				server.PlayerEntitySetDirty(player);
			}
		}

		public int GetAutoRestartInterval()
		{
			return server.config.AutoRestartCycle;
		}

		public int GetServerUptimeSeconds()
		{
			return (int)server.serverUptime.Elapsed.TotalSeconds;
		}

		public void SendPlayerRedirect(int player, string ip, int port)
		{
			server.SendServerRedirect(player, ip, port);
		}

		public bool IsShuttingDown()
		{
			return server.exit.GetExit();
		}

		public void RegisterCheckOnBlockBuild(ModDelegates.CheckBlockBuild f)
		{
			server.modEventHandlers.checkonbuild.Add(f);
		}

		public void RegisterCheckOnBlockDelete(ModDelegates.CheckBlockDelete f)
		{
			server.modEventHandlers.checkondelete.Add(f);
		}

		public void RegisterCheckOnBlockUse(ModDelegates.CheckBlockUse f)
		{
			server.modEventHandlers.checkonuse.Add(f);
		}

		#region Deprecated methods
		public double GetCurrentYearTotal()
		{
			return server.GetTime().Year;
		}
		public double GetCurrentHourTotal()
		{
			return server.GetTime().Hour;
		}
		public double GetGameYearRealHours()
		{
			return GetGameDayRealHours() * GetDaysPerYear();
		}
		public void SetGameYearRealHours(double hours)
		{
			throw new NotImplementedException("SetGameYearRealHours is no longer supported!");
		}
		#endregion
	}
}
