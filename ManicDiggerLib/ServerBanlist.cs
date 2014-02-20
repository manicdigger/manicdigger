using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GameModeFortress
{
	[XmlRoot(ElementName = "ManicDiggerServerBanlist")]
	public class ServerBanlist
	{
		[XmlArrayItem(ElementName = "User")]
		public List<string> BannedUsers { get; set; }
		[XmlArrayItem(ElementName = "IP")]
		public List<string> BannedIPs { get; set; }

		public bool IsIPBanned(string ipAddress)
		{
			foreach (string bannedip in this.BannedIPs)
			{
				if(bannedip == ipAddress)
					return true;
			}
			return false;
		}

		public bool IsUserBanned(string username)
		{
			foreach (string banneduser in this.BannedUsers)
			{
				if (username.Equals(banneduser, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}
			return false;
		}

		public bool UnbanPlayer(string username)
		{
			bool exists = false;
			for (int i = this.BannedUsers.Count - 1; i >= 0; i--)
			{
				string banneduser = this.BannedUsers[i];
				if (banneduser.Equals(username, StringComparison.InvariantCultureIgnoreCase))
				{
					exists = true;
					this.BannedUsers.RemoveAt(i);
					break;
				}
			}
			return exists;
		}
		
		public bool UnbanIP(string ip)
		{
			bool exists = false;
			for (int i = this.BannedIPs.Count - 1; i >= 0; i--)
			{
				string bannedip = this.BannedIPs[i];
				if (bannedip.Equals(ip, StringComparison.InvariantCultureIgnoreCase))
				{
					exists = true;
					this.BannedIPs.RemoveAt(i);
					break;
				}
			}
			return exists;
		}

		public ServerBanlist()
		{
			//Set Defaults
			this.BannedIPs = new List<string>();
			this.BannedUsers = new List<string>();
		}
	}
}
