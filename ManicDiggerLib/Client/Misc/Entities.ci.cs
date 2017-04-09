public class Entity
{
	public Entity()
	{
		scripts = new EntityScript[8];
		scriptsCount = 0;
	}

	internal Expires expires;
	internal Sprite sprite;
	internal Grenade_ grenade;
	internal Bullet_ bullet;
	internal Minecart minecart;
	internal PlayerDrawInfo playerDrawInfo;

	internal EntityScript[] scripts;
	internal int scriptsCount;

	// network
	internal EntityPosition_ networkPosition;
	internal EntityPosition_ position;
	internal DrawName drawName;
	internal EntityDrawModel drawModel;
	internal EntityDrawText drawText;
	internal Packet_ServerExplosion push;
	internal bool usable;
	internal Packet_ServerPlayerStats playerStats;
	internal EntityDrawArea drawArea;
}

public class EntityDrawArea
{
	internal int x;
	internal int y;
	internal int z;
	internal int sizex;
	internal int sizey;
	internal int sizez;
	internal bool visible;
}

public class EntityPosition_
{
	internal float x;
	internal float y;
	internal float z;
	internal float rotx;
	internal float roty;
	internal float rotz;

	internal bool PositionLoaded;
	internal int LastUpdateMilliseconds;
}

public class EntityDrawModel
{
	public EntityDrawModel()
	{
		CurrentTexture = -1;
	}
	internal float eyeHeight;
	internal string Model_;
	internal float ModelHeight;
	internal string Texture_;
	internal bool DownloadSkin;

	internal int CurrentTexture;
	internal HttpResponseCi SkinDownloadResponse;
	internal AnimatedModelRenderer renderer;
}

public class EntityDrawText
{
	internal float dx;
	internal float dy;
	internal float dz;
	internal float rotx;
	internal float roty;
	internal float rotz;
	internal string text;
}
