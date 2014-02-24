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
		public List<UserEntry> BannedUsers { get; set; }
		[XmlArrayItem(ElementName = "IP")]
		public List<IPEntry> BannedIPs { get; set; }

		public bool IsIPBanned(string ipAddress)
		{
			foreach (IPEntry bannedip in this.BannedIPs)
			{
				if(bannedip.IPAdress == ipAddress)
					return true;
			}
			return false;
		}

		public bool IsUserBanned(string username)
		{
			foreach (UserEntry banneduser in this.BannedUsers)
			{
				if (username.Equals(banneduser.UserName, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}
			return false;
		}
		
		public bool BanPlayer(string username, string bannedby, string reason)
		{
			if (IsUserBanned(username))
			{
				return false;
			}
			UserEntry newBan = new UserEntry();
			newBan.UserName = username;
			newBan.BannedBy = bannedby;
			if (!string.IsNullOrEmpty(reason))
			{
				newBan.Reason = reason;
			}
			BannedUsers.Add(newBan);
			return true;
		}
		
		public bool BanIP(string ipadress, string bannedby, string reason)
		{
			if (IsIPBanned(ipadress))
			{
				return false;
			}
			IPEntry newBan = new IPEntry();
			newBan.IPAdress = ipadress;
			newBan.BannedBy = bannedby;
			if (!string.IsNullOrEmpty(reason))
			{
				newBan.Reason = reason;
			}
			BannedIPs.Add(newBan);
			return true;
		}

		public bool UnbanPlayer(string username)
		{
			bool exists = false;
			for (int i = this.BannedUsers.Count - 1; i >= 0; i--)
			{
				UserEntry banneduser = this.BannedUsers[i];
				if (banneduser.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))
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
				IPEntry bannedip = this.BannedIPs[i];
				if (bannedip.IPAdress.Equals(ip, StringComparison.InvariantCultureIgnoreCase))
				{
					exists = true;
					this.BannedIPs.RemoveAt(i);
					break;
				}
			}
			return exists;
		}
		
		public UserEntry GetUserEntry(string username)
		{
			foreach (UserEntry banneduser in this.BannedUsers)
			{
				if (username.Equals(banneduser.UserName, StringComparison.InvariantCultureIgnoreCase))
					return banneduser;
			}
			return null;
		}
		
		public IPEntry GetIPEntry(string ipadress)
		{
			foreach (IPEntry bannedip in this.BannedIPs)
			{
				if(bannedip.IPAdress == ipadress)
					return bannedip;
			}
			return null;
		}

		public ServerBanlist()
		{
			//Set Defaults
			this.BannedIPs = new List<IPEntry>();
			this.BannedUsers = new List<UserEntry>();
		}
	}
	
	public class UserEntry
	{
		public string UserName { get; set; }
		public string BannedBy  { get; set; }
		[XmlElement(IsNullable = true)]
		public string Reason  { get; set; }
	}
	
	public class IPEntry
	{
		public string IPAdress { get; set; }
		public string BannedBy  { get; set; }
		[XmlElement(IsNullable = true)]
		public string Reason  { get; set; }
	}
}
