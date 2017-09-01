public class LoginData
{
	internal string ServerAddress;
	internal int Port;
	internal string AuthCode; //Md5(private server key + player name)
	internal string Token;

	internal bool PasswordCorrect;
	internal bool ServerCorrect;
}

public class LoginClientCi
{
	internal LoginResultRef loginResult;
	public void Login(GamePlatform platform, string user, string password, string publicServerKey, string token, LoginResultRef result, LoginData resultLoginData_)
	{
		loginResult = result;
		resultLoginData = resultLoginData_;
		result.value = LoginResult.Connecting;

		LoginUser = user;
		LoginPassword = password;
		LoginToken = token;
		LoginPublicServerKey = publicServerKey;
		shouldLogin = true;
	}
	string LoginUser;
	string LoginPassword;
	string LoginToken;
	string LoginPublicServerKey;

	bool shouldLogin;
	string loginUrl;
	HttpResponseCi loginUrlResponse;
	HttpResponseCi loginResponse;
	LoginData resultLoginData;
	public void Update(GamePlatform platform)
	{
		if (loginResult == null)
		{
			return;
		}

		if (loginUrlResponse == null && loginUrl == null)
		{
			loginUrlResponse = new HttpResponseCi();
			platform.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/login.php", loginUrlResponse);
		}
		if (loginUrlResponse != null && loginUrlResponse.done)
		{
			loginUrl = platform.StringFromUtf8ByteArray(loginUrlResponse.value, loginUrlResponse.valueLength);
			loginUrlResponse = null;
		}

		if (loginUrl != null)
		{
			if (shouldLogin)
			{
				shouldLogin = false;
				string requestString = platform.StringFormat4("username={0}&password={1}&server={2}&token={3}"
					, LoginUser, LoginPassword, LoginPublicServerKey, LoginToken);
				IntRef byteArrayLength = new IntRef();
				byte[] byteArray = platform.StringToUtf8ByteArray(requestString, byteArrayLength);
				loginResponse = new HttpResponseCi();
				platform.WebClientUploadDataAsync(loginUrl, byteArray, byteArrayLength.value, loginResponse);
			}
			if (loginResponse != null && loginResponse.done)
			{
				string responseString = platform.StringFromUtf8ByteArray(loginResponse.value, loginResponse.valueLength);
				resultLoginData.PasswordCorrect = !(platform.StringContains(responseString, "Wrong username") || platform.StringContains(responseString, "Incorrect username"));
				resultLoginData.ServerCorrect = !platform.StringContains(responseString, "server");
				if (resultLoginData.PasswordCorrect)
				{
					loginResult.value = LoginResult.Ok;
				}
				else
				{
					loginResult.value = LoginResult.Failed;
				}
				IntRef linesCount = new IntRef();
				string[] lines = platform.ReadAllLines(responseString, linesCount);
				if (linesCount.value >= 3)
				{
					resultLoginData.AuthCode = lines[0];
					resultLoginData.ServerAddress = lines[1];
					resultLoginData.Port = platform.IntParse(lines[2]);
					resultLoginData.Token = lines[3];
				}
				loginResponse = null;
			}
		}
	}
}
