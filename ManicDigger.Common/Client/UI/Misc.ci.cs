public class TextTexture
{
	internal FontCi font;
	internal string text;
	internal int texture;
	internal int texturewidth;
	internal int textureheight;
	internal int textwidth;
	internal int textheight;
}

public enum LoginResult
{
	None,
	Connecting,
	Failed,
	Ok
}

public class LoginResultRef
{
	internal LoginResult value;
}

public class HttpResponseCi
{
	internal bool done;
	internal byte[] value;
	internal int valueLength;

	internal string GetString(GamePlatform platform)
	{
		return platform.StringFromUtf8ByteArray(value, valueLength);
	}

	internal bool error;

	public bool GetDone() { return done; }
	public void SetDone(bool value_) { done = value_; }
	public byte[] GetValue() { return value; }
	public void SetValue(byte[] value_) { value = value_; }
	public int GetValueLength() { return valueLength; }
	public void SetValueLength(int value_) { valueLength = value_; }
	public bool GetError() { return error; }
	public void SetError(bool value_) { error = value_; }
}

public class ThumbnailResponseCi
{
	internal bool done;
	internal bool error;
	internal string serverMessage;
	internal byte[] data;
	internal int dataLength;

	public bool GetDone() { return done; }
	public void SetDone(bool value_) { done = value_; }
	public bool GetError() { return error; }
	public void SetError(bool value_) { error = value_; }
	public byte[] GetData() { return data; }
	public void SetData(byte[] value_) { data = value_; }
	public int GetDataLength() { return dataLength; }
	public void SetDataLength(int value_) { dataLength = value_; }
	public string GetServerMessage() { return serverMessage; }
	public void SetServerMessage(string value_) { serverMessage = value_; }
}

public class ServerOnList
{
	internal string hash;
	internal string name;
	internal string motd;
	internal int port;
	internal string ip;
	internal string version;
	internal int users;
	internal int max;
	internal string gamemode;
	internal string players;
	internal bool thumbnailDownloading;
	internal bool thumbnailError;
	internal bool thumbnailFetched;
}

public class DrawModeEnum
{
	public const int Triangles = 0;
	public const int Lines = 1;
}
