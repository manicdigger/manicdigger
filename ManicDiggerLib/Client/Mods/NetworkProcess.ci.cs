public class ModNetworkProcess : ClientMod
{
    public ModNetworkProcess()
    {
        CurrentChunk = new byte[1024 * 64];
        CurrentChunkCount = 0;
        receivedchunk = new int[32 * 32 * 32];
        decompressedchunk = new byte[32 * 32 * 32 * 2];
    }
    internal byte[] CurrentChunk;
    internal int CurrentChunkCount;
    int[] receivedchunk;
    byte[] decompressedchunk;

#if CITO
    macro Index3d(x, y, h, sizex, sizey) ((((((h) * (sizey)) + (y))) * (sizex)) + (x))
#else
    static int Index3d(int x, int y, int h, int sizex, int sizey)
    {
        return (h * sizey + y) * sizex + x;
    }
#endif

    Game game;
    public override void OnReadOnlyBackgroundThread(Game game_, float dt)
    {
        game = game_;
        NetworkProcess();
    }

    public void NetworkProcess()
    {
        game.currentTimeMilliseconds = game.platform.TimeMillisecondsFromStart();
        if (game.main == null)
        {
            return;
        }
        NetIncomingMessage msg;
        for (; ; )
        {
            if (game.invalidVersionPacketIdentification != null)
            {
                break;
            }
            msg = game.main.ReadMessage();
            if (msg == null)
            {
                break;
            }
            TryReadPacket(msg.message, msg.messageLength);
        }
    }

    public void TryReadPacket(byte[] data, int dataLength)
    {
        Packet_Server packet = new Packet_Server();
        Packet_ServerSerializer.DeserializeBuffer(data, dataLength, packet);

        ProcessInBackground(packet);

        ProcessPacketTask task = new ProcessPacketTask();
        task.game = game;
        task.packet_ = packet;
        game.QueueActionCommit(task);

        game.LastReceivedMilliseconds = game.currentTimeMilliseconds;
        //return lengthPrefixLength + packetLength;
    }

    void ProcessInBackground(Packet_Server packet)
    {
        switch (packet.Id)
        {
            case Packet_ServerIdEnum.ChunkPart:
                byte[] arr = packet.ChunkPart.CompressedChunkPart;
                int arrLength = game.platform.ByteArrayLength(arr); // todo
                for (int i = 0; i < arrLength; i++)
                {
                    CurrentChunk[CurrentChunkCount++] = arr[i];
                }
                break;
            case Packet_ServerIdEnum.Chunk_:
                {
                    Packet_ServerChunk p = packet.Chunk_;
                    if (CurrentChunkCount != 0)
                    {
                        game.platform.GzipDecompress(CurrentChunk, CurrentChunkCount, decompressedchunk);
                        {
                            int i = 0;
                            for (int zz = 0; zz < p.SizeZ; zz++)
                            {
                                for (int yy = 0; yy < p.SizeY; yy++)
                                {
                                    for (int xx = 0; xx < p.SizeX; xx++)
                                    {
                                        int block = (decompressedchunk[i + 1] << 8) + decompressedchunk[i];
                                        if (block < GlobalVar.MAX_BLOCKTYPES)
                                        {
                                            receivedchunk[Index3d(xx, yy, zz, p.SizeX, p.SizeY)] = block;
                                        }
                                        i += 2;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int size = p.SizeX * p.SizeY * p.SizeZ;
                        for (int i = 0; i < size; i++)
                        {
                            receivedchunk[i] = 0;
                        }
                    }
                    {
                        game.map.SetMapPortion(p.X, p.Y, p.Z, receivedchunk, p.SizeX, p.SizeY, p.SizeZ);
                        for (int xx = 0; xx < 2; xx++)
                        {
                            for (int yy = 0; yy < 2; yy++)
                            {
                                for (int zz = 0; zz < 2; zz++)
                                {
                                    //d_Shadows.OnSetChunk(p.X + 16 * xx, p.Y + 16 * yy, p.Z + 16 * zz);//todo
                                }
                            }
                        }
                    }
                    game.ReceivedMapLength += CurrentChunkCount;// lengthPrefixLength + packetLength;
                    CurrentChunkCount = 0;
                }
                break;
            case Packet_ServerIdEnum.HeightmapChunk:
                {
                    Packet_ServerHeightmapChunk p = packet.HeightmapChunk;
                    game.platform.GzipDecompress(p.CompressedHeightmap, game.platform.ByteArrayLength(p.CompressedHeightmap), decompressedchunk);
                    int[] decompressedchunk1 = Game.ByteArrayToUshortArray(decompressedchunk, p.SizeX * p.SizeY * 2);
                    for (int xx = 0; xx < p.SizeX; xx++)
                    {
                        for (int yy = 0; yy < p.SizeY; yy++)
                        {
                            int height = decompressedchunk1[MapUtilCi.Index2d(xx, yy, p.SizeX)];
                            game.d_Heightmap.SetBlock(p.X + xx, p.Y + yy, height);
                        }
                    }
                }
                break;
        }
    }
}

public class ProcessPacketTask : Action_
{
    internal Game game;
    internal Packet_Server packet_;

    public override void Run()
    {
        ProcessPacket(packet_);
    }

    internal void ProcessPacket(Packet_Server packet)
    {
        if (game.packetHandlers[packet.Id] != null)
        {
            game.packetHandlers[packet.Id].Handle(game, packet);
        }
        switch (packet.Id)
        {
            case Packet_ServerIdEnum.ServerIdentification:
                {
                    string invalidversionstr = game.language.InvalidVersionConnectAnyway();

                    game.serverGameVersion = packet.Identification.MdProtocolVersion;
                    if (game.serverGameVersion != game.platform.GetGameVersion())
                    {
                        game.ChatLog("[GAME] Different game versions");
                        string q = game.platform.StringFormat2(invalidversionstr, game.platform.GetGameVersion(), game.serverGameVersion);
                        game.invalidVersionDrawMessage = q;
                        game.invalidVersionPacketIdentification = packet;
                    }
                    else
                    {
                        game.ProcessServerIdentification(packet);
                    }
                    game.ReceivedMapLength = 0;
                }
                break;
            case Packet_ServerIdEnum.Ping:
                {
                    game.SendPingReply();
                    game.ServerInfo.ServerPing.Send(game.platform);
                }
                break;
            case Packet_ServerIdEnum.PlayerPing:
                {
                    game.ServerInfo.ServerPing.Receive(game.platform);
                }
                break;
            case Packet_ServerIdEnum.LevelInitialize:
                {
                    game.ChatLog("[GAME] Initialized map loading");
                    game.ReceivedMapLength = 0;
                    game.InvokeMapLoadingProgress(0, 0, game.language.Connecting());
                }
                break;
            case Packet_ServerIdEnum.LevelDataChunk:
                {
                    game.InvokeMapLoadingProgress(packet.LevelDataChunk.PercentComplete, game.ReceivedMapLength, packet.LevelDataChunk.Status);
                }
                break;
            case Packet_ServerIdEnum.LevelFinalize:
                {
                    game.ChatLog("[GAME] Finished map loading");
                }
                break;
            case Packet_ServerIdEnum.SetBlock:
                {
                    int x = packet.SetBlock.X;
                    int y = packet.SetBlock.Y;
                    int z = packet.SetBlock.Z;
                    int type = packet.SetBlock.BlockType;
                    //try
                    {
                        game.SetTileAndUpdate(x, y, z, type);
                    }
                    //catch { Console.WriteLine("Cannot update tile!"); }
                }
                break;
            case Packet_ServerIdEnum.FillArea:
                {
                    int ax = packet.FillArea.X1;
                    int ay = packet.FillArea.Y1;
                    int az = packet.FillArea.Z1;
                    int bx = packet.FillArea.X2;
                    int by = packet.FillArea.Y2;
                    int bz = packet.FillArea.Z2;

                    int startx = MathCi.MinInt(ax, bx);
                    int endx = MathCi.MaxInt(ax, bx);
                    int starty = MathCi.MinInt(ay, by);
                    int endy = MathCi.MaxInt(ay, by);
                    int startz = MathCi.MinInt(az, bz);
                    int endz = MathCi.MaxInt(az, bz);

                    int blockCount = packet.FillArea.BlockCount;
                    {
                        for (int x = startx; x <= endx; x++)
                        {
                            for (int y = starty; y <= endy; y++)
                            {
                                for (int z = startz; z <= endz; z++)
                                {
                                    // if creative mode is off and player run out of blocks
                                    if (blockCount == 0)
                                    {
                                        return;
                                    }
                                    //try
                                    {
                                        game.SetTileAndUpdate(x, y, z, packet.FillArea.BlockType);
                                    }
                                    //catch
                                    //{
                                    //    Console.WriteLine("Cannot update tile!");
                                    //}
                                    blockCount--;
                                }
                            }
                        }
                    }
                }
                break;
            case Packet_ServerIdEnum.FillAreaLimit:
                {
                    game.fillAreaLimit = packet.FillAreaLimit.Limit;
                    if (game.fillAreaLimit > 100000)
                    {
                        game.fillAreaLimit = 100000;
                    }
                }
                break;
            case Packet_ServerIdEnum.Freemove:
                {
                    game.AllowFreemove = packet.Freemove.IsEnabled != 0;
                    if (!game.AllowFreemove)
                    {
                        game.controls.SetFreemove(FreemoveLevelEnum.None);
                        game.movespeed = game.basemovespeed;
                        game.Log(game.language.MoveNormal());
                    }
                }
                break;
            case Packet_ServerIdEnum.PlayerSpawnPosition:
                {
                    int x = packet.PlayerSpawnPosition.X;
                    int y = packet.PlayerSpawnPosition.Y;
                    int z = packet.PlayerSpawnPosition.Z;
                    game.playerPositionSpawnX = x;
                    game.playerPositionSpawnY = z;
                    game.playerPositionSpawnZ = y;
                    game.Log(game.platform.StringFormat(game.language.SpawnPositionSetTo(), game.platform.StringFormat3("{0},{1},{2}", game.platform.IntToString(x), game.platform.IntToString(y), game.platform.IntToString(z))));
                }
                break;
            case Packet_ServerIdEnum.Message:
                {
                    game.AddChatline(packet.Message.Message);
                    game.ChatLog(packet.Message.Message);
                }
                break;
            case Packet_ServerIdEnum.DisconnectPlayer:
                {
                    game.ChatLog(game.platform.StringFormat("[GAME] Disconnected by the server ({0})", packet.DisconnectPlayer.DisconnectReason));
                    //Exit mouse pointer lock if necessary
                    if (game.platform.IsMousePointerLocked())
                    {
                        game.platform.ExitMousePointerLock();
                    }
                    //When server disconnects player, return to main menu
                    game.platform.MessageBoxShowError(packet.DisconnectPlayer.DisconnectReason, "Disconnected from server");
                    game.ExitToMainMenu_();
                    break;
                }
            case Packet_ServerIdEnum.PlayerStats:
                {
                    Packet_ServerPlayerStats p = packet.PlayerStats;
                    game.PlayerStats = p;
                }
                break;
            case Packet_ServerIdEnum.FiniteInventory:
                {
                    //check for null so it's possible to connect
                    //to old versions of game (before 2011-05-05)
                    if (packet.Inventory.Inventory != null)
                    {
                        //d_Inventory.CopyFrom(ConvertInventory(packet.Inventory.Inventory));
                        game.UseInventory(packet.Inventory.Inventory);
                    }
                    //FiniteInventory = packet.FiniteInventory.BlockTypeAmount;
                    //ENABLE_FINITEINVENTORY = packet.FiniteInventory.IsFinite;
                    //FiniteInventoryMax = packet.FiniteInventory.Max;
                }
                break;
            case Packet_ServerIdEnum.Season:
                {
                    packet.Season.Hour -= 1;
                    if (packet.Season.Hour < 0)
                    {
                        //shouldn't happen
                        packet.Season.Hour = 12 * Game.HourDetail;
                    }
                    int sunlight = game.NightLevels[packet.Season.Hour];
                    game.SkySphereNight = sunlight < 8;
                    game.d_SunMoonRenderer.day_length_in_seconds = 60 * 60 * 24 / packet.Season.DayNightCycleSpeedup;
                    int hour = packet.Season.Hour / Game.HourDetail;
                    if (game.d_SunMoonRenderer.GetHour() != hour)
                    {
                        game.d_SunMoonRenderer.SetHour(hour);
                    }

                    if (game.sunlight_ != sunlight)
                    {
                        game.sunlight_ = sunlight;
                        //d_Shadows.ResetShadows();
                        game.RedrawAllBlocks();
                    }
                }
                break;
            case Packet_ServerIdEnum.BlobInitialize:
                {
                    game.blobdownload = new CitoMemoryStream();
                    //blobdownloadhash = ByteArrayToString(packet.BlobInitialize.hash);
                    game.blobdownloadname = packet.BlobInitialize.Name;
                    game.blobdownloadmd5 = packet.BlobInitialize.Md5;
                }
                break;
            case Packet_ServerIdEnum.BlobPart:
                {
                    int length = game.platform.ByteArrayLength(packet.BlobPart.Data);
                    game.blobdownload.Write(packet.BlobPart.Data, 0, length);
                    game.ReceivedMapLength += length;
                }
                break;
            case Packet_ServerIdEnum.BlobFinalize:
                {
                    byte[] downloaded = game.blobdownload.ToArray();

                    if (game.blobdownloadname != null) // old servers
                    {
                        game.SetFile(game.blobdownloadname, game.blobdownloadmd5, downloaded, game.blobdownload.Length());
                    }
                    game.blobdownload = null;
                }
                break;
            case Packet_ServerIdEnum.Sound:
                {
                    game.PlaySoundAt(packet.Sound.Name, packet.Sound.X, packet.Sound.Y, packet.Sound.Z);
                }
                break;
            case Packet_ServerIdEnum.RemoveMonsters:
                {
                    for (int i = Game.entityMonsterIdStart; i < Game.entityMonsterIdStart + Game.entityMonsterIdCount; i++)
                    {
                        game.entities[i] = null;
                    }
                }
                break;
            case Packet_ServerIdEnum.Translation:
                game.language.Override(packet.Translation.Lang, packet.Translation.Id, packet.Translation.Translation);
                break;
            case Packet_ServerIdEnum.BlockType:
                game.NewBlockTypes[packet.BlockType.Id] = packet.BlockType.Blocktype;
                break;
            case Packet_ServerIdEnum.SunLevels:
                game.NightLevels = packet.SunLevels.Sunlevels;
                break;
            case Packet_ServerIdEnum.LightLevels:
                for (int i = 0; i < packet.LightLevels.LightlevelsCount; i++)
                {
                    game.mLightLevels[i] = game.DeserializeFloat(packet.LightLevels.Lightlevels[i]);
                }
                break;
            case Packet_ServerIdEnum.Follow:
                IntRef oldFollowId = game.FollowId();
                game.Follow = packet.Follow.Client;
                if (packet.Follow.Tpp != 0)
                {
                    game.SetCamera(CameraType.Overhead);
                    game.player.position.rotx = Game.GetPi();
                    game.GuiStateBackToGame();
                }
                else
                {
                    game.SetCamera(CameraType.Fpp);
                }
                break;
            case Packet_ServerIdEnum.Bullet:
                game.EntityAddLocal(game.CreateBulletEntity(
                   game.DeserializeFloat(packet.Bullet.FromXFloat),
                   game.DeserializeFloat(packet.Bullet.FromYFloat),
                   game.DeserializeFloat(packet.Bullet.FromZFloat),
                   game.DeserializeFloat(packet.Bullet.ToXFloat),
                   game.DeserializeFloat(packet.Bullet.ToYFloat),
                   game.DeserializeFloat(packet.Bullet.ToZFloat),
                   game.DeserializeFloat(packet.Bullet.SpeedFloat)));
                break;
            case Packet_ServerIdEnum.Ammo:
                if (!game.ammostarted)
                {
                    game.ammostarted = true;
                    for (int i = 0; i < packet.Ammo.TotalAmmoCount; i++)
                    {
                        Packet_IntInt k = packet.Ammo.TotalAmmo[i];
                        game.LoadedAmmo[k.Key_] = MathCi.MinInt(k.Value_, game.blocktypes[k.Key_].AmmoMagazine);
                    }
                }
                game.TotalAmmo = new int[GlobalVar.MAX_BLOCKTYPES];
                for (int i = 0; i < packet.Ammo.TotalAmmoCount; i++)
                {
                    game.TotalAmmo[packet.Ammo.TotalAmmo[i].Key_] = packet.Ammo.TotalAmmo[i].Value_;
                }
                break;
            case Packet_ServerIdEnum.Explosion:
                {
                    Entity entity = new Entity();
                    entity.expires = new Expires();
                    entity.expires.timeLeft = game.DeserializeFloat(packet.Explosion.TimeFloat);
                    entity.push = packet.Explosion;
                    game.EntityAddLocal(entity);
                }
                break;
            case Packet_ServerIdEnum.Projectile:
                {
                    Entity entity = new Entity();

                    Sprite sprite = new Sprite();
                    sprite.image = "ChemicalGreen.png";
                    sprite.size = 14;
                    sprite.animationcount = 0;
                    sprite.positionX = game.DeserializeFloat(packet.Projectile.FromXFloat);
                    sprite.positionY = game.DeserializeFloat(packet.Projectile.FromYFloat);
                    sprite.positionZ = game.DeserializeFloat(packet.Projectile.FromZFloat);
                    entity.sprite = sprite;

                    Grenade_ grenade = new Grenade_();
                    grenade.velocityX = game.DeserializeFloat(packet.Projectile.VelocityXFloat);
                    grenade.velocityY = game.DeserializeFloat(packet.Projectile.VelocityYFloat);
                    grenade.velocityZ = game.DeserializeFloat(packet.Projectile.VelocityZFloat);
                    grenade.block = packet.Projectile.BlockId;
                    grenade.sourcePlayer = packet.Projectile.SourcePlayerID;
                    entity.grenade = grenade;

                    entity.expires = Expires.Create(game.DeserializeFloat(packet.Projectile.ExplodesAfterFloat));

                    game.EntityAddLocal(entity);
                }
                break;
            case Packet_ServerIdEnum.BlockTypes:
                game.blocktypes = game.NewBlockTypes;
                game.NewBlockTypes = new Packet_BlockType[GlobalVar.MAX_BLOCKTYPES];

                int textureInAtlasIdsCount = 1024;
                string[] textureInAtlasIds = new string[textureInAtlasIdsCount];
                int lastTextureId = 0;
                for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
                {
                    if (game.blocktypes[i] != null)
                    {
                        string[] to_load = new string[7];
                        int to_loadLength = 7;
                        {
                            to_load[0] = game.blocktypes[i].TextureIdLeft;
                            to_load[1] = game.blocktypes[i].TextureIdRight;
                            to_load[2] = game.blocktypes[i].TextureIdFront;
                            to_load[3] = game.blocktypes[i].TextureIdBack;
                            to_load[4] = game.blocktypes[i].TextureIdTop;
                            to_load[5] = game.blocktypes[i].TextureIdBottom;
                            to_load[6] = game.blocktypes[i].TextureIdForInventory;
                        }
                        for (int k = 0; k < to_loadLength; k++)
                        {
                            if (!Contains(textureInAtlasIds, textureInAtlasIdsCount, to_load[k]))
                            {
                                textureInAtlasIds[lastTextureId++] = to_load[k];
                            }
                        }
                    }
                }
                game.d_Data.UseBlockTypes(game.platform, game.blocktypes, GlobalVar.MAX_BLOCKTYPES);
                for (int i = 0; i < GlobalVar.MAX_BLOCKTYPES; i++)
                {
                    Packet_BlockType b = game.blocktypes[i];
                    if (b == null)
                    {
                        continue;
                    }
                    //Indexed by block id and TileSide.
                    if (textureInAtlasIds != null)
                    {
                        game.TextureId[i][0] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdTop);
                        game.TextureId[i][1] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdBottom);
                        game.TextureId[i][2] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdFront);
                        game.TextureId[i][3] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdBack);
                        game.TextureId[i][4] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdLeft);
                        game.TextureId[i][5] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdRight);
                        game.TextureIdForInventory[i] = IndexOf(textureInAtlasIds, textureInAtlasIdsCount, b.TextureIdForInventory);
                    }
                }
                game.UseTerrainTextures(textureInAtlasIds, textureInAtlasIdsCount);
                game.handRedraw = true;
                game.RedrawAllBlocks();
                break;
            case Packet_ServerIdEnum.ServerRedirect:
                game.ChatLog("[GAME] Received server redirect");
                //Leave current server
                game.SendLeave(Packet_LeaveReasonEnum.Leave);
                //Exit game screen and create new game instance
                game.ExitAndSwitchServer(packet.Redirect);
                break;
        }
    }

    bool Contains(string[] arr, int arrLength, string value)
    {
        return IndexOf(arr, arrLength, value) != -1;
    }

    int IndexOf(string[] arr, int arrLength, string value)
    {
        for (int i = 0; i < arrLength; i++)
        {
            if (Game.StringEquals(arr[i], value))
            {
                return i;
            }
        }
        return -1;
    }
}
