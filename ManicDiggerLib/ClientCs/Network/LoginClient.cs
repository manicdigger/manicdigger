using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace ManicDigger
{
    public class LoginData
    {
        public string ServerAddress;
        public int Port;
        public string AuthCode; //Md5(private server key + player name)

        public bool PasswordCorrect;
        public bool ServerCorrect;
    }
    public class LoginClientManicDigger
    {
        public string LoginUrl = null;
        public LoginData Login(string username, string password, string publicServerKey)
        {
            if (LoginUrl == null)
            {
                WebClient c = new WebClient();
                LoginUrl = c.DownloadString("http://manicdigger.sourceforge.net/login.txt");
            }

            StringWriter sw = new StringWriter();//&salt={4}
            // TODO: encrypt password.
            string requestString = String.Format("username={0}&password={1}&server={2}"
                , username, password, publicServerKey);

            var request = (HttpWebRequest)WebRequest.Create(LoginUrl);
            request.Method = "POST";
            request.Timeout = 15000; // 15s timeout
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

            byte[] formData = Encoding.ASCII.GetBytes(requestString);
            request.ContentLength = formData.Length;

            System.Net.ServicePointManager.Expect100Continue = false; // fixes lighthttpd 417 error

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Flush();
            }

            WebResponse response = request.GetResponse();

            string responseText = null;
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                responseText = sr.ReadToEnd();
            }

            request.Abort();
            LoginData data = new LoginData();
            //Problem with current login.php: when both password and serverKey are incorrect then
            // response will be just "Wrong server" and won't contain "Wrong username".
            //Workaround: For checking password correctness
            //provide a good publicServerKey - some server from xml.php.
            data.PasswordCorrect = !(responseText.Contains("Wrong username") || responseText.Contains("Incorrect username"));
            data.ServerCorrect = !responseText.Contains("server");
            StringReader sr2 = new StringReader(responseText);
            try
            {
                string authcode = sr2.ReadLine();
                string ip = sr2.ReadLine();
                string port = sr2.ReadLine();
                data.AuthCode = authcode;
                data.ServerAddress = ip;
                data.Port = int.Parse(port);
            }
            catch
            {
            }
            return data;
        }
    }
}
