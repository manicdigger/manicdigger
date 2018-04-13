public class ConnectData
{
	internal string Username;
	internal string Ip;
	internal int Port;
	internal string Auth;
	internal string ServerPassword;
	internal bool IsServePasswordProtected;
	public static ConnectData FromUri(UriCi uri)
	{
		ConnectData c = new ConnectData();
		c = new ConnectData();
		c.Ip = uri.GetIp();
		c.Port = 25565;
		c.Username = "gamer";
		if (uri.GetPort() != -1)
		{
			c.Port = uri.GetPort();
		}
		if (uri.GetGet().ContainsKey("user"))
		{
			c.Username = uri.GetGet().Get("user");
		}
		if (uri.GetGet().ContainsKey("auth"))
		{
			c.Auth = uri.GetGet().Get("auth");
		}
		if (uri.GetGet().ContainsKey("serverPassword"))
		{
			c.IsServePasswordProtected = ConvertCi.StringToBool(uri.GetGet().Get("serverPassword"));
		}
		return c;
	}

	public void SetIp(string value)
	{
		Ip = value;
	}

	public void SetPort(int value)
	{
		Port = value;
	}

	public void SetUsername(string value)
	{
		Username = value;
	}

	public void SetIsServePasswordProtected(bool value)
	{
		IsServePasswordProtected = value;
	}

	public bool GetIsServePasswordProtected()
	{
		return IsServePasswordProtected;
	}

	public void SetServerPassword(string value)
	{
		ServerPassword = value;
	}
}
