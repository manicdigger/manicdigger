using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        public GameData d_Data { get { return game.d_Data; } set { game.d_Data = value; } }
        public Config3d d_Config3d { get { return game.d_Config3d; } set { game.d_Config3d = value; } }
        public HudChat d_HudChat { get { return game.d_HudChat; } set { game.d_HudChat = value; } }
        public Packet_Inventory d_Inventory { get { return game.d_Inventory; } set { game.d_Inventory = value; } }
        public INetClient main { get { return game.main; } set { game.main = value; } }
        public InfiniteMapChunked2d d_Heightmap { get { return game.d_Heightmap; } set { game.d_Heightmap = value; } }
        public MeshBatcher d_Batcher { get { return game.d_Batcher; } set { game.d_Batcher = value; } }
        public TerrainChunkTesselatorCi d_TerrainChunkTesselator { get { return game.d_TerrainChunkTesselator; } set { game.d_TerrainChunkTesselator = value; } }
        public ServerInformation ServerInfo { get { return game.ServerInfo; } set { game.ServerInfo = value; } }
        public bool AllowFreemove { get { return game.AllowFreemove; } set { game.AllowFreemove = value; } }

        internal StackFloatArray mvMatrix { get { return game.mvMatrix; } set { game.mvMatrix = value; } }
        internal StackFloatArray pMatrix { get { return game.pMatrix; } set { game.pMatrix = value; } }

        private float currentfov()
        {
            return game.currentfov();
        }

        public void AudioPlay(string file)
        {
            game.AudioPlay(file);
        }

        public void AudioPlayAt(string file, float x, float y, float z)
        {
            game.AudioPlayAt(file, x, y, z);
        }

        public void AudioPlayLoop(string file, bool play, bool restart)
        {
            game.AudioPlayLoop(file, play, restart);
        }

        public void SetCamera(CameraType type)
        {
            game.SetCamera(type);
        }

        int maxdrawdistance { get { return game.maxdrawdistance; } set { game.maxdrawdistance = value; } }
        public void ToggleFog()
        {
            game.ToggleFog();
        }
        CameraType cameratype { get { return game.cameratype; } set { game.cameratype = value; } }
        public bool FreeMouse
        {
            get
            {
                return game.GetFreeMouse();
            }
            set
            {
                game.SetFreeMouse(value);
            }
        }
        float movespeed { get { return game.movespeed; } set { game.movespeed = value; } }
        public float basemovespeed { get { return game.basemovespeed; } }
        public bool ENABLE_FREEMOVE { get { return game.ENABLE_FREEMOVE; } set { game.ENABLE_FREEMOVE = value; } }
        public bool ENABLE_NOCLIP { get { return game.ENABLE_NOCLIP; } set { game.ENABLE_NOCLIP = value; } }
        public bool mouseleftclick { get { return game.mouseleftclick; } set { game.mouseleftclick = value; } }
        public bool mouseleftdeclick { get { return game.mouseleftdeclick; } set { game.mouseleftdeclick = value; } }
        public bool wasmouseleft { get { return game.wasmouseleft; } set { game.wasmouseleft = value; } }
        public bool mouserightclick { get { return game.mouserightclick; } set { game.mouserightclick = value; } }
        public bool mouserightdeclick { get { return game.mouserightdeclick; } set { game.mouserightdeclick = value; } }
        public bool wasmouseright { get { return game.wasmouseright; } set { game.wasmouseright = value; } }
        CharacterPhysicsState player { get { return game.player; } }
        public Vector3Ref playervelocity { get { return game.playervelocity; } set { game.playervelocity = value; } }
        private void UpdateFallDamageToPlayer()
        {
            game.UpdateFallDamageToPlayer();
        }

        bool IsWater(int blockType)
        {
            return game.IsWater(blockType);
        }

        void ApplyDamageToPlayer(int damage, int damageSource, int sourceId)
        {
            game.ApplyDamageToPlayer(damage, damageSource, sourceId);
        }

        private void Respawn()
        {
            game.Respawn();
        }

        float DeserializeFloat(int value)
        {
            return game.DeserializeFloat(value);
        }

        bool IronSights { get { return game.IronSights; } set { game.IronSights = value; } }
        IntRef BlockUnderPlayer()
        {
            return game.BlockUnderPlayer();
        }
        Vector3Ref playerdestination { get { return game.playerdestination; } set { game.playerdestination = value; } }
        public MenuState menustate { get { return game.menustate; } set { game.menustate = value; } }

        public bool IsTileEmptyForPhysics(int x, int y, int z)
        {
            return game.IsTileEmptyForPhysics(x, y, z);
        }
        bool IsTileEmptyForPhysicsClose(int x, int y, int z)
        {
            return game.IsTileEmptyForPhysicsClose(x, y, z);
        }
        bool IsUsableBlock(int blocktype)
        {
            return game.IsUsableBlock(blocktype);
        }
        bool IsWearingWeapon()
        {
            return game.IsWearingWeapon();
        }
        private Packet_InventoryPosition InventoryPositionMaterialSelector(int materialId)
        {
            return game.InventoryPositionMaterialSelector(materialId);
        }
        private Packet_InventoryPosition InventoryPositionMainArea(int x, int y)
        {
            return game.InventoryPositionMainArea(x, y);
        }
        float GetCurrentBlockHealth(int x, int y, int z) { return game.GetCurrentBlockHealth(x, y, z); }
        DictionaryVector3Float blockHealth { get { return game.blockHealth; } set { game.blockHealth = value; } }

        public Packet_ServerPlayerStats PlayerStats { get { return game.PlayerStats; } set { game.PlayerStats = value; } }

        public void DrawPlayerHealth()
        {
            game.DrawPlayerHealth();
        }

        public void DrawPlayerOxygen()
        {
            game.DrawPlayerOxygen();
        }

        void DrawCompass()
        {
            game.DrawCompass();
        }

        void DrawEnemyHealthUseInfo(string name, float progress, bool useInfo)
        {
            game.DrawEnemyHealthUseInfo(name, progress, useInfo);
        }

        Language language { get { return game.language; } }

        public float getblockheight(int x, int y, int z)
        {
            return game.getblockheight(x, y, z);
        }

        float BuildDelay
        {
            get
            {
                return game.BuildDelay();
            }
        }
        public int SelectedBlockPositionX { get { return game.SelectedBlockPositionX; } set { game.SelectedBlockPositionX = value; } }
        public int SelectedBlockPositionY { get { return game.SelectedBlockPositionY; } set { game.SelectedBlockPositionY = value; } }
        public int SelectedBlockPositionZ { get { return game.SelectedBlockPositionZ; } set { game.SelectedBlockPositionZ = value; } }
        public float CharacterEyesHeight { get { return game.GetCharacterEyesHeight(); } set { game.SetCharacterEyesHeight(value); } }
        bool currentMatrixModeProjection { get { return game.currentMatrixModeProjection; } set { game.currentMatrixModeProjection = value; } }
        void SetMatrixUniforms()
        {
            game.SetMatrixUniforms();
        }
        public void GLLoadMatrix(float[] m)
        {
            game.GLLoadMatrix(m);
        }

        public void GLPopMatrix()
        {
            game.GLPopMatrix();
        }

        public void GLScale(float x, float y, float z)
        {
            game.GLScale(x, y, z);
        }

        public void GLRotate(float angle, float x, float y, float z)
        {
            game.GLRotate(angle, x, y, z);
        }

        public void GLTranslate(float x, float y, float z)
        {
            game.GLTranslate(x, y, z);
        }

        public void GLPushMatrix()
        {
            game.GLPushMatrix();
        }

        public void GLLoadIdentity()
        {
            game.GLLoadIdentity();
        }

        void GLOrtho(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            game.GLOrtho(left, right, bottom, top, zNear, zFar);
        }

        public void GLMatrixModeModelView()
        {
            game.GLMatrixModeModelView();
        }

        void GLMatrixModeProjection()
        {
            game.GLMatrixModeProjection();
        }
        public bool ENABLE_TPP_VIEW { get { return game.ENABLE_TPP_VIEW; } set { game.ENABLE_TPP_VIEW = value; } }
        bool overheadcamera { get { return game.overheadcamera; } set { game.overheadcamera = value; } }
        public GuiState guistate { get { return game.guistate; } set { game.guistate = value; } }
        public int[] TotalAmmo { get { return game.TotalAmmo; } set { game.TotalAmmo = value; } }
        public int[] LoadedAmmo { get { return game.LoadedAmmo; } set { game.LoadedAmmo = value; } }
        private string ValidFont(string family)
        {
            return game.ValidFont(family);
        }
        private void DrawScreenshotFlash()
        {
            game.DrawScreenshotFlash();
        }
        public bool IsValid(int blocktype)
        {
            return game.IsValid(blocktype);
        }
        PointFloatRef GetAim() { return game.GetAim(); }
        float CurrentAimRadius { get { return game.CurrentAimRadius(); } }
        float RadiusWhenMoving { get { return game.RadiusWhenMoving; } set { game.RadiusWhenMoving = value; } }
        float CurrentRecoil { get { return game.CurrentRecoil(); } }
        public int xcenter(float width)
        {
            return game.xcenter(width);
        }
        public int ycenter(float height)
        {
            return game.ycenter(height);
        }
        public int ENABLE_LAG { get { return game.ENABLE_LAG; } set { game.ENABLE_LAG = value; } }
        public int ActiveMaterial { get { return game.ActiveMaterial; } set { game.ActiveMaterial = value; } }
        public DictionaryStringString PerformanceInfo { get { return game.performanceinfo; } set { game.performanceinfo = value; } }
        public void AddChatline(string s)
        {
            game.AddChatline(s);
        }
        bool IsLava(int blockType)
        {
            return game.IsLava(blockType);
        }
        IntRef BlockInHand()
        {
            return game.BlockInHand();
        }
        RailDirection PickHorizontalVertical(float xfract, float yfract)
        {
            return game.PickHorizontalVertical(xfract, yfract);
        }
        private RailDirection PickCorners(float xfract, float zfract)
        {
            return game.PickCorners(xfract, zfract);
        }
        public void SendPacket(byte[] packet, int packetLength)
        {
            game.SendPacket(packet, packetLength);
        }
        public void SendFillArea(int startX, int startY, int startZ, int endX, int endY, int endZ, int blockType)
        {
            game.SendFillArea(startX, startY, startZ, endX, endY, endZ, blockType);
        }
        public void SendPacketClient(Packet_Client p)
        {
            game.SendPacketClient(p);
        }
        public void SendChat(string s)
        {
            game.SendChat(s);
        }
        private void SendPingReply()
        {
            game.SendPingReply();
        }
        public byte[] Serialize(Packet_Client p, IntRef retLength)
        {
            return game.Serialize(p, retLength);
        }
        private int LocalPlayerId { get { return game.LocalPlayerId; } set { game.LocalPlayerId = value; } }
        int DialogsCount() { return game.DialogsCount(); }
        int GetDialogId(string name) { return game.GetDialogId(name); }
        VisibleDialog[] dialogs { get { return game.dialogs; } set { game.dialogs = value; } }
        int dialogsCount { get { return game.dialogsCount; } set { game.dialogsCount = value; } }
        public int MapSizeX { get { return game.MapSizeX; } set { game.MapSizeX = value; } }
        public int MapSizeY { get { return game.MapSizeY; } set { game.MapSizeY = value; } }
        public int MapSizeZ { get { return game.MapSizeZ; } set { game.MapSizeZ = value; } }
        public unsafe int GetBlock(int x, int y, int z)
        {
            return game.GetBlock(x, y, z);
        }

        public void SetBlock(int x, int y, int z, int tileType)
        {
            game.SetBlock(x, y, z, tileType);
        }

        public void ShadowsOnSetBlock(int x, int y, int z)
        {
            game.ShadowsOnSetBlock(x, y, z);
        }

        private void UpdateColumnHeight(int x, int y)
        {
            game.UpdateColumnHeight(x, y);
        }
        public Chunk GetChunk(int x, int y, int z)
        {
            return game.GetChunk(x, y, z);
        }
        public int chunksize { get { return game.chunksize; } set { game.chunksize = value; } }
        public void Reset(int sizex, int sizey, int sizez)
        {
            game.Reset(sizex, sizey, sizez);
        }
        private void SetChunksAroundDirty(int cx, int cy, int cz)
        {
            game.SetChunksAroundDirty(cx, cy, cz);
        }
        public void GetMapPortion(int[] outPortion, int x, int y, int z, int portionsizex, int portionsizey, int portionsizez)
        {
            game.GetMapPortion(outPortion, x, y, z, portionsizex, portionsizey, portionsizez);
        }
        public int terrainTexturesPerAtlas { get { return game.terrainTexturesPerAtlas; } set { game.terrainTexturesPerAtlas = value; } }
        public int WhiteTexture()
        {
            return game.WhiteTexture();
        }
        public float fov { get { return game.fov; } set { game.fov = value; } }
        public float znear { get { return game.znear; } set { game.znear = value; } }
        public float zfar() { return game.zfar(); }
        public bool ENABLE_ZFAR { get { return game.ENABLE_ZFAR; } set { game.ENABLE_ZFAR = value; } }

        public void OrthoMode(int width, int height)
        {
            game.OrthoMode(width, height);
        }

        public void PerspectiveMode()
        {
            game.PerspectiveMode();
        }
        internal int MaybeGetLight(int x, int y, int z)
        {
            return game.MaybeGetLight(x, y, z);
        }
        public TerrainRenderer terrainRenderer { get { return game.terrainRenderer; } set { game.terrainRenderer = value; } }
        public void MapLoadingStart()
        {
            game.MapLoadingStart();
        }
        private void MapLoadingDraw()
        {
            game.MapLoadingDraw();
        }
        MapLoadingProgressEventArgs maploadingprogress { get { return game.maploadingprogress; } set { game.maploadingprogress = value; } }
        public void Draw2dTexture(int textureid, float x1, float y1, float width, float height, IntRef inAtlasId, int atlastextures, int color, bool enabledepthtest)
        {
            game.Draw2dTexture(textureid, x1, y1, width, height, inAtlasId, atlastextures, color, enabledepthtest);
        }
        public void Draw2dTextures(Draw2dData[] todraw, int todrawLength, int textureid, float angle)
        {
            game.Draw2dTextures(todraw, todrawLength, textureid, angle);
        }
        public void Draw2dText(string text, FontCi font, float x, float y, IntRef color, bool enabledepthtest)
        {
            game.Draw2dText(text, font, x, y, color, enabledepthtest);
        }
        public void Draw2dBitmapFile(string filename, int x, int y, int w, int h)
        {
            game.Draw2dBitmapFile(filename, x, y, w, h);
        }
        private void DrawDialogs()
        {
            game.DrawDialogs();
        }
        Vector3IntRef currentAttackedBlock { get { return game.currentAttackedBlock; } set { game.currentAttackedBlock = value; } }
        void DrawEnemyHealthBlock()
        {
            game.DrawEnemyHealthBlock();
        }
        void DrawEnemyHealthCommon(string name, float progress)
        {
            game.DrawEnemyHealthCommon(name, progress);
        }
        private void SendRequestBlob()
        {
            game.SendRequestBlob();
        }
        public int MonsterIdFirst { get { return Game.MonsterIdFirst; } }
        int currentTimeMilliseconds { get { return game.currentTimeMilliseconds; } set { game.currentTimeMilliseconds = value; } }
        public GameDataMonsters d_DataMonsters { get { return game.d_DataMonsters; } set { game.d_DataMonsters = value; } }
        public int ReceivedMapLength { get { return game.ReceivedMapLength; } set { game.ReceivedMapLength = value; } }
        bool EnablePlayerUpdatePosition(int kKey)
        {
            return game.EnablePlayerUpdatePosition(kKey);
        }

        bool EnablePlayerUpdatePositionContainsKey(int kKey)
        {
            return game.EnablePlayerUpdatePositionContainsKey(kKey);
        }
        public bool AudioEnabled { get { return game.AudioEnabled; } set { game.AudioEnabled = value; } }
        public byte localstance { get { return game.localstance; } set { game.localstance = value; } }
        bool spawned { get { return game.spawned; } set { game.spawned = value; } }
        void ReadAndUpdatePlayerPosition(Packet_PositionAndOrientation positionAndOrientation, int playerid)
        {
            game.ReadAndUpdatePlayerPosition(positionAndOrientation, playerid);
        }
        int MapLoadingPercentComplete { get { return game.MapLoadingPercentComplete; } set { game.MapLoadingPercentComplete = value; } }
        string MapLoadingStatus { get { return game.MapLoadingStatus; } set { game.MapLoadingStatus = value; } }
        public int LastReceivedMilliseconds { get { return game.LastReceivedMilliseconds; } set { game.LastReceivedMilliseconds = value; } }
        public bool ENABLE_DRAW2D { get { return game.ENABLE_DRAW2D; } set { game.ENABLE_DRAW2D = value; } }
        int screenshotflash { get { return game.screenshotflash; } set { game.screenshotflash = value; } }
        int playertexturedefault { get { return game.playertexturedefault; } set { game.playertexturedefault = value; } }
        public bool ENABLE_DRAW_TEST_CHARACTER { get { return game.ENABLE_DRAW_TEST_CHARACTER; } set { game.ENABLE_DRAW_TEST_CHARACTER = value; } }
        AnimationState a { get { return game.a; } set { game.a = value; } }
        int skyspheretexture { get { return game.skyspheretexture; } set { game.skyspheretexture = value; } }
        int skyspherenighttexture { get { return game.skyspherenighttexture; } set { game.skyspherenighttexture = value; } }
        public SkySphere skysphere { get { return game.skysphere; } set { game.skysphere = value; } }
        int reloadblock { get { return game.reloadblock; } set { game.reloadblock = value; } }
        int reloadstartMilliseconds { get { return game.reloadstartMilliseconds; } set { game.reloadstartMilliseconds = value; } }
        int PreviousActiveMaterialBlock { get { return game.PreviousActiveMaterialBlock; } set { game.PreviousActiveMaterialBlock = value; } }
        int lastOxygenTickMilliseconds { get { return game.lastOxygenTickMilliseconds; } set { game.lastOxygenTickMilliseconds = value; } }
        bool freemousejustdisabled { get { return game.freemousejustdisabled; } set { game.freemousejustdisabled = value; } }
        int typinglogpos { get { return game.typinglogpos; } set { game.typinglogpos = value; } }
        TypingState GuiTyping { get { return game.GuiTyping; } set { game.GuiTyping = value; } }
        public ConnectData connectdata { get { return game.connectdata; } set { game.connectdata = value; } }
        public bool issingleplayer { get { return game.issingleplayer; } set { game.issingleplayer = value; } }
        public bool StartedSinglePlayerServer { get { return game.StartedSinglePlayerServer; } set { game.StartedSinglePlayerServer = value; } }
        bool IsShiftPressed { get { return game.IsShiftPressed; } set { game.IsShiftPressed = value; } }
        public bool reconnect { get { return game.reconnect; } set { game.reconnect = value; } }
        float rotation_speed { get { return game.rotation_speed; } }
        public void SendLeave(int reason)
        {
            game.SendLeave(reason);
        }
        public HudInventory d_HudInventory { get { return game.d_HudInventory; } set { game.d_HudInventory = value; } }
        public WeaponRenderer d_Weapon { get { return game.d_Weapon; } set { game.d_Weapon = value; } }
        public IFrustumCulling d_FrustumCulling { get { return game.d_FrustumCulling; } set { game.d_FrustumCulling = value; } }
        public CharacterPhysicsCi d_Physics { get { return game.d_Physics; } set { game.d_Physics = value; } }
        public ClientModManager1 modmanager { get { return game.modmanager; } set { game.modmanager = value; } }
        public ClientMod[] clientmods { get { return game.clientmods; } set { game.clientmods = value; } }
        public int clientmodsCount { get { return game.clientmodsCount; } set { game.clientmodsCount = value; } }
        public bool SkySphereNight { get { return game.SkySphereNight; } set { game.SkySphereNight = value; } }
        public ParticleEffectBlockBreak particleEffectBlockBreak = new ParticleEffectBlockBreak();
        int lastchunkupdates { get { return game.lastchunkupdates; } set { game.lastchunkupdates = value; } }
        int lasttitleupdateMilliseconds { get { return game.lasttitleupdateMilliseconds; } set { game.lasttitleupdateMilliseconds = value; } }
        bool ENABLE_DRAWPOSITION { get { return game.ENABLE_DRAWPOSITION; } set { game.ENABLE_DRAWPOSITION = value; } }
        int SerializeFloat(float p)
        {
            return game.SerializeFloat(p);
        }
        float WeaponAttackStrength()
        {
            return game.WeaponAttackStrength();
        }
        float NextFloat(float min, float max)
        {
            return game.NextFloat(min, max);
        }
        GamePlatform platform { get { return game.platform; } }
        public void SendPosition(float positionX, float positionY, float positionZ, float orientationX, float orientationY, float orientationZ)
        {
            game.SendPosition(positionX, positionY, positionZ, orientationX, orientationY, orientationZ);
        }
        void PlaySoundAt(string name, float x, float y, float z)
        {
            game.PlaySoundAt(name, x, y, z);
        }
        void InvokeMapLoadingProgress(int progressPercent, int progressBytes, string status)
        {
            game.InvokeMapLoadingProgress(progressPercent, progressBytes, status);
        }
        public void Log(string p)
        {
            game.Log(p);
        }
        public void SetTileAndUpdate(int x, int y, int z, int type)
        {
            game.SetTileAndUpdate(x, y, z, type);
        }
        void RedrawBlock(int x, int y, int z)
        {
            game.RedrawBlock(x, y, z);
        }
        bool IsFillBlock(int blocktype)
        {
            return game.IsFillBlock(blocktype);
        }
        float FloorFloat(float a)
        {
            return game.FloorFloat(a);
        }
        private bool IsAnyPlayerInPos(int blockposX, int blockposY, int blockposZ)
        {
            return game.IsAnyPlayerInPos(blockposX, blockposY, blockposZ);
        }
        void CraftingRecipeSelected(int x, int y, int z, IntRef recipe)
        {
            game.CraftingRecipeSelected(x, y, z, recipe);
        }
        internal float PICK_DISTANCE { get { return game.PICK_DISTANCE; } set { game.PICK_DISTANCE = value; } }
        internal bool leftpressedpicking { get { return game.leftpressedpicking; } set { game.leftpressedpicking = value; } }
        internal int selectedmodelid { get { return game.selectedmodelid; } set { game.selectedmodelid = value; } }
        internal int pistolcycle { get { return game.pistolcycle; } set { game.pistolcycle = value; } }
        internal int lastironsightschangeMilliseconds { get { return game.lastironsightschangeMilliseconds; } set { game.lastironsightschangeMilliseconds = value; } }
        internal int grenadecookingstartMilliseconds { get { return game.grenadecookingstartMilliseconds; } set { game.grenadecookingstartMilliseconds = value; } }
        internal float grenadetime { get { return game.grenadetime; } set { game.grenadetime = value; } }
        internal int lastpositionsentMilliseconds { get { return game.lastpositionsentMilliseconds; } set { game.lastpositionsentMilliseconds = value; } }
        float mouseDeltaX { get { return game.mouseDeltaX; } set { game.mouseDeltaX = value; } }
        float mouseDeltaY { get { return game.mouseDeltaY; } set { game.mouseDeltaY = value; } }
        string Follow { get { return game.Follow; } set { game.Follow = value; } }
        float Dist(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            return game.Dist(x1, y1, z1, x2, y2, z2);
        }
        void DrawBlockInfo()
        {
            game.DrawBlockInfo();
        }
        int TextSizeWidth(string s, int size)
        {
            return game.TextSizeWidth(s, size);
        }
        int TextSizeHeight(string s, int size)
        {
            return game.TextSizeHeight(s, size);
        }
        private void DrawAmmo()
        {
            game.DrawAmmo();
        }
        void Circle3i(float x, float y, float radius)
        {
            game.Circle3i(x, y, radius);
        }
        private void DrawAim()
        {
            game.DrawAim();
        }
        private void DrawSkySphere()
        {
            game.DrawSkySphere();
        }
        int totaltimeMilliseconds { get { return game.totaltimeMilliseconds; } set { game.totaltimeMilliseconds = value; } }
        int MathFloor(float p)
        {
            return game.MathFloor(p);
        }
        public Vector3Ref GrenadeBounce(Vector3Ref oldposition, Vector3Ref newposition, Vector3Ref velocity, float dt)
        {
            return game.GrenadeBounce(oldposition, newposition, velocity, dt);
        }
        void EntityExpire(float dt)
        {
            game.EntityExpire(dt);
        }
        Entity[] entities { get { return game.entities; } set { game.entities = value; } }
        int entitiesCount { get { return game.entitiesCount; } set { game.entitiesCount = value; } }
        void EntityAddLocal(Entity entity)
        {
            game.EntityAddLocal(entity);
        }
        void GetEntitiesPush(Vector3Ref push)
        {
            game.GetEntitiesPush(push);
        }
        void DrawSprites()
        {
            game.DrawSprites();
        }
        void UpdateGrenade(int grenadeEntityId, float dt)
        {
            game.UpdateGrenade(grenadeEntityId, dt);
        }
        void UpdateBullets(float dt)
        {
            game.UpdateBullets(dt);
        }
        Entity CreateBulletEntity(float fromX, float fromY, float fromZ, float toX, float toY, float toZ, float speed)
        {
            return game.CreateBulletEntity(fromX, fromY, fromZ, toX, toY, toZ, speed);
        }
        void InterpolatePositions(float dt)
        {
            game.InterpolatePositions(dt);
        }
        void SetPlayers()
        {
            game.SetPlayers();
        }
        bool[] keyboardState { get { return game.keyboardState; } set { game.keyboardState = value; } }
        void DrawPlayerNames()
        {
            game.DrawPlayerNames();
        }
        public bool Swimming()
        {
            return game.Swimming();
        }
        public bool WaterSwimming()
        {
            return game.WaterSwimming();
        }
        public bool LavaSwimming()
        {
            return game.LavaSwimming();
        }
        int terraincolor()
        {
            return game.terraincolor();
        }
        void SetAmbientLight(int color)
        {
            game.SetAmbientLight(color);
        }
        public OptionsCi options { get { return game.options; } set { game.options = value; } }
        public int GetKey(int key)
        {
            return game.GetKey(key);
        }
        float MoveSpeedNow()
        {
            return game.MoveSpeedNow();
        }
        void UpdateMouseViewportControl(float dt)
        {
            game.UpdateMouseViewportControl(dt);
        }
        float VectorAngleGet(float qX, float qY, float qZ)
        {
            return game.VectorAngleGet(qX, qY, qZ);
        }
        float Length(float x, float y, float z)
        {
            return game.Length(x, y, z);
        }
        public void HandleMaterialKeys(int eKey)
        {
            game.HandleMaterialKeys(eKey);
        }
        public void UseVsync()
        {
            game.UseVsync();
        }
        public void ToggleVsync()
        {
            game.ToggleVsync();
        }
        public void GuiStateBackToGame()
        {
            game.GuiStateBackToGame();
        }
        internal float overheadcameradistance { get { return game.overheadcameradistance; } set { game.overheadcameradistance = value; } }
        internal float tppcameradistance { get { return game.tppcameradistance; } set { game.tppcameradistance = value; } }
        internal int TPP_CAMERA_DISTANCE_MIN { get { return game.TPP_CAMERA_DISTANCE_MIN; } set { game.TPP_CAMERA_DISTANCE_MIN = value; } }
        internal int TPP_CAMERA_DISTANCE_MAX { get { return game.TPP_CAMERA_DISTANCE_MAX; } set { game.TPP_CAMERA_DISTANCE_MAX = value; } }
        Packet_Client CreateLoginPacket(string username, string verificationKey)
        {
            return game.CreateLoginPacket(username, verificationKey);
        }
        Packet_Client CreateLoginPacket_(string username, string verificationKey, string serverPassword)
        {
            return game.CreateLoginPacket_(username, verificationKey, serverPassword);
        }
        public void Connect(string serverAddress, int port, string username, string auth)
        {
            game.Connect(serverAddress, port, username, auth);
        }
        public void Connect_(string serverAddress, int port, string username, string auth, string serverPassword)
        {
            game.Connect_(serverAddress, port, username, auth, serverPassword);
        }
        public int sunlight_ { get { return game.sunlight_; } set { game.sunlight_ = value; } }
        public void RedrawAllBlocks() { game.RedrawAllBlocks(); }
        private void SetFog()
        {
            game.SetFog();
        }
        public int Height() { return game.Height(); }
        public int Width() { return game.Width(); }
        private void UpdateBlockDamageToPlayer(float dt)
        {
            game.UpdateBlockDamageToPlayer(dt);
        }
        BlockPosSide Nearest(BlockPosSide[] pick2, int pick2Count, float x, float y, float z)
        {
            return game.Nearest(pick2, pick2Count, x, y, z);
        }
        BlockOctreeSearcher s { get { return game.s; } set { game.s = value; } }
        //Don't allow to look through walls.
        void LimitThirdPersonCameraToWalls(Vector3Ref eye, Vector3Ref target, FloatRef curtppcameradistance)
        {
            game.LimitThirdPersonCameraToWalls(eye, target, curtppcameradistance);
        }
        Kamera overheadcameraK { get { return game.overheadcameraK; } set { game.overheadcameraK = value; } }
        Vector3Ref OverheadCamera_cameraEye { get { return game.OverheadCamera_cameraEye; } set { game.OverheadCamera_cameraEye = value; } }
        float[] OverheadCamera()
        {
            return game.OverheadCamera();
        }
        float[] FppCamera()
        {
            return game.FppCamera();
        }
        void FillChunk(Chunk destination, int destinationchunksize,
             int sourcex, int sourcey, int sourcez, int[] source,
             int sourcechunksizeX, int sourcechunksizeY, int sourcechunksizeZ)
        {
            game.FillChunk(destination, destinationchunksize,
                sourcex, sourcey, sourcez, source,
                sourcechunksizeX, sourcechunksizeY, sourcechunksizeZ);
        }
        public void SetMapPortion(int x, int y, int z, int[] chunk, int sizeX, int sizeY, int sizeZ)
        {
            game.SetMapPortion(x, y, z, chunk, sizeX, sizeY, sizeZ);
        }
        void ChatLog(string p)
        {
            game.ChatLog(p);
        }
        IntRef FollowId()
        {
            return game.FollowId();
        }
        internal DictionaryVector3Float fillarea { get { return game.fillarea; } set { game.fillarea = value; } }
        internal Vector3IntRef fillstart { get { return game.fillstart; } set { game.fillstart = value; } }
        internal Vector3IntRef fillend { get { return game.fillend; } set { game.fillend = value; } }
        internal int fillAreaLimit { get { return game.fillAreaLimit; } set { game.fillAreaLimit = value; } }
        void ClearFillArea()
        {
            game.ClearFillArea();
        }

        void FillFill(Vector3IntRef a, Vector3IntRef b)
        {
            game.FillFill(a, b);
        }
        public void SendSetBlock(int positionX, int positionY, int positionZ, int mode, int type, int materialslot)
        {
            game.SendSetBlock(positionX, positionY, positionZ, mode, type, materialslot);
        }
        public float HeadingToOrientationX(byte heading)
        {
            return game.HeadingToOrientationX(heading);
        }
        public float PitchToOrientationY(byte pitch)
        {
            return game.PitchToOrientationY(pitch);
        }
        void OnPickUseWithTool(int posX, int posY, int posZ)
        {
            game.OnPickUseWithTool(posX, posY, posZ);
        }
        void KeyUp(int eKey)
        {
            game.KeyUp(eKey);
        }
        float playerPositionSpawnX { get { return game.playerPositionSpawnX; } set { game.playerPositionSpawnX = value; } }
        float playerPositionSpawnY { get { return game.playerPositionSpawnY; } set { game.playerPositionSpawnY = value; } }
        float playerPositionSpawnZ { get { return game.playerPositionSpawnZ; } set { game.playerPositionSpawnZ = value; } }
        int[] materialSlots { get { return game.materialSlots; } set { game.materialSlots = value; } }
        void MapLoaded()
        {
            game.MapLoaded();
        }
        void UpdateWalkSound(float dt)
        {
            game.UpdateWalkSound(dt);
        }
        int GetSoundCount(string[] soundwalk)
        {
            return game.GetSoundCount(soundwalk);
        }
        string[] soundwalkcurrent()
        {
            return game.soundwalkcurrent();
        }
        public byte HeadingByte(float orientationX, float orientationY, float orientationZ)
        {
            return game.HeadingByte(orientationX, orientationY, orientationZ);
        }
        public byte PitchByte(float orientationX, float orientationY, float orientationZ)
        {
            return game.PitchByte(orientationX, orientationY, orientationZ);
        }

        internal void Draw2dText1(string text, int x, int y, int fontsize, IntRef color, bool enabledepthtest)
        {
            game.Draw2dText1(text, x, y, fontsize, color, enabledepthtest);
        }
        public InventoryUtilClient d_InventoryUtil { get { return game.d_InventoryUtil; } set { game.d_InventoryUtil = value; } }
        void UseInventory(Packet_Inventory packet_Inventory)
        {
            game.UseInventory(packet_Inventory);
        }
        float EyesPosX() { return game.EyesPosX(); }
        float EyesPosY() { return game.EyesPosY(); }
        float EyesPosZ() { return game.EyesPosZ(); }
        internal int mouseCurrentX { get { return game.mouseCurrentX; } set { game.mouseCurrentX = value; } }
        internal int mouseCurrentY { get { return game.mouseCurrentY; } set { game.mouseCurrentY = value; } }
        void DrawMouseCursor()
        {
            game.DrawMouseCursor();
        }
    }
}
