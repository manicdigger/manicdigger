/// <summary>
/// Calculates various ping related values
/// </summary>
public class Ping_
{
	public Ping_()
	{
		RoundtripTimeMilliseconds = 0;
		ready = true;
		timeSendMilliseconds = 0;
		timeout = 10;
	}

	int RoundtripTimeMilliseconds;

	bool ready;
	int timeSendMilliseconds;
	int timeout; //in seconds

	public int GetTimeoutValue()
	{
		return timeout;
	}
	public void SetTimeoutValue(int value)
	{
		timeout = value;
	}

	public bool Send(int currentTimeMilliseconds)
	{
		if (!ready)
		{
			return false;
		}
		ready = false;
		this.timeSendMilliseconds = currentTimeMilliseconds;
		return true;
	}

	public bool Receive(int currentTimeMilliseconds)
	{
		if (ready)
		{
			return false;
		}
		this.RoundtripTimeMilliseconds = currentTimeMilliseconds - timeSendMilliseconds;
		ready = true;
		return true;
	}

	public bool Timeout(int currentTimeMilliseconds)
	{
		if ((currentTimeMilliseconds - timeSendMilliseconds) / 1000 > this.timeout)
		{
			this.ready = true;
			return true;
		}
		return false;
	}

	internal int RoundtripTimeTotalMilliseconds()
	{
		return RoundtripTimeMilliseconds;
	}
}
