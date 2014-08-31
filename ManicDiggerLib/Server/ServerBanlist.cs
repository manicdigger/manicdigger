using System;
using System.Collections.Generic;
using System.Xml.Serialization;


[XmlRoot(ElementName = "ManicDiggerServerBanlist")]
public class ServerBanlist
{
    [XmlArrayItem(ElementName = "User")]
    public List<UserEntry> BannedUsers { get; set; }
    [XmlArrayItem(ElementName = "IP")]
    public List<IPEntry> BannedIPs { get; set; }

    public bool IsIPBanned(string ipAddress)
    {
        foreach (IPEntry bannedip in BannedIPs)
        {
            if (bannedip.IPAdress == ipAddress)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsUserBanned(string username)
    {
        foreach (UserEntry banneduser in BannedUsers)
        {
            if (username.Equals(banneduser.UserName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public int ClearTimeBans()
    {
        int counter = 0;
        List<string> unbanUsers = new List<string>();
        List<string> unbanIPs = new List<string>();
        //Check banned usernames
        foreach (UserEntry banneduser in BannedUsers)
        {
            if (banneduser.BannedUntil < DateTime.UtcNow)
            {
                unbanUsers.Add(banneduser.UserName);
            }
        }
        //Check banned IPs
        foreach (IPEntry bannedip in BannedIPs)
        {
            if (bannedip.BannedUntil < DateTime.UtcNow)
            {
                unbanIPs.Add(bannedip.IPAdress);
            }
        }
        //Unban all players/IPs whose timeban has elapsed
        foreach (string username in unbanUsers)
        {
            if (UnbanPlayer(username))
            {
                counter++;
            }
        }
        foreach (string ipadress in unbanIPs)
        {
            if (UnbanIP(ipadress))
            {
                counter++;
            }
        }
        if (counter > 0)
        {
            Console.WriteLine("Removed {0} expired timebans.", counter);
        }
        return counter;
    }

    public bool BanPlayer(string username, string bannedby, string reason)
    {
        return TimeBanPlayer(username, bannedby, reason, 0);
    }

    public bool TimeBanPlayer(string username, string bannedby, string reason, int intervalMinutes)
    {
        if (IsUserBanned(username))
        {
            return false;
        }
        UserEntry newBan = new UserEntry();
        newBan.UserName = username;
        newBan.BannedBy = bannedby;
        if (intervalMinutes > 0)
        {
            newBan.BannedUntil = DateTime.UtcNow + TimeSpan.FromMinutes(intervalMinutes);
        }
        if (!string.IsNullOrEmpty(reason))
        {
            newBan.Reason = reason;
        }
        BannedUsers.Add(newBan);
        return true;
    }

    public bool BanIP(string ipadress, string bannedby, string reason)
    {
        return TimeBanIP(ipadress, bannedby, reason, 0);
    }

    public bool TimeBanIP(string ipadress, string bannedby, string reason, int intervalMinutes)
    {
        if (IsIPBanned(ipadress))
        {
            return false;
        }
        IPEntry newBan = new IPEntry();
        newBan.IPAdress = ipadress;
        newBan.BannedBy = bannedby;
        if (intervalMinutes > 0)
        {
            newBan.BannedUntil = DateTime.UtcNow + TimeSpan.FromMinutes(intervalMinutes);
        }
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
        for (int i = BannedUsers.Count - 1; i >= 0; i--)
        {
            UserEntry banneduser = BannedUsers[i];
            if (banneduser.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))
            {
                exists = true;
                BannedUsers.RemoveAt(i);
                break;
            }
        }
        return exists;
    }

    public bool UnbanIP(string ip)
    {
        bool exists = false;
        for (int i = BannedIPs.Count - 1; i >= 0; i--)
        {
            IPEntry bannedip = BannedIPs[i];
            if (bannedip.IPAdress.Equals(ip, StringComparison.InvariantCultureIgnoreCase))
            {
                exists = true;
                BannedIPs.RemoveAt(i);
                break;
            }
        }
        return exists;
    }

    public UserEntry GetUserEntry(string username)
    {
        foreach (UserEntry banneduser in BannedUsers)
        {
            if (username.Equals(banneduser.UserName, StringComparison.InvariantCultureIgnoreCase))
                return banneduser;
        }
        return null;
    }

    public IPEntry GetIPEntry(string ipadress)
    {
        foreach (IPEntry bannedip in BannedIPs)
        {
            if (bannedip.IPAdress == ipadress)
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
    public string BannedBy { get; set; }
    [XmlElement(IsNullable = true)]
    public DateTime? BannedUntil { get; set; }
    [XmlElement(IsNullable = true)]
    public string Reason { get; set; }
}

public class IPEntry
{
    public string IPAdress { get; set; }
    public string BannedBy { get; set; }
    [XmlElement(IsNullable = true)]
    public DateTime? BannedUntil { get; set; }
    [XmlElement(IsNullable = true)]
    public string Reason { get; set; }
}

