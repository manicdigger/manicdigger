public class Preferences
{
	public Preferences()
	{
		items = new DictionaryStringString();
	}
	internal GamePlatform platform;
	internal DictionaryStringString items;

	public void SetPlatform(GamePlatform value_) { platform = value_; }
	public DictionaryStringString GetItems() { return items; }

	public string GetKey(int i)
	{
		if (items.items[i] != null)
		{
			return items.items[i].key;
		}
		else
		{
			return null;
		}
	}

	public int GetKeysCount()
	{
		return items.size;
	}

	public string GetString(string key, string default_)
	{
		if (!items.ContainsKey(key))
		{
			return default_;
		}
		return items.Get(key);
	}
	public void SetString(string key, string value)
	{
		items.Set(key, value);
	}

	public bool GetBool(string key, bool default_)
	{
		string value = GetString(key, null);
		if (value == null)
		{
			return default_;
		}
		if (value == "0")
		{
			return false;
		}
		if (value == "1")
		{
			return true;
		}
		return default_;
	}

	public int GetInt(string key, int default_)
	{
		if (GetString(key, null) == null)
		{
			return default_;
		}
		FloatRef ret = new FloatRef();
		if (platform.FloatTryParse(GetString(key, null), ret))
		{
			return platform.FloatToInt(ret.value);
		}
		return default_;
	}

	public void SetBool(string key, bool value)
	{
		SetString(key, value ? "1" : "0");
	}

	public void SetInt(string key, int value)
	{
		SetString(key, platform.IntToString(value));
	}

	internal void Remove(string key)
	{
		items.Remove(key);
	}
}
