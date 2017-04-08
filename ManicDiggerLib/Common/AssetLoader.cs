using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ManicDigger.Common
{
	/// <summary>
	/// Provides functions for loading and reading asset files from given folders
	/// </summary>
	public class AssetLoader
	{
		public AssetLoader(string[] datapaths_)
		{
			this.datapaths = datapaths_;
		}
		string[] datapaths;
		public void LoadAssetsAsync(AssetList list, FloatRef progress)
		{
			List<Asset> assets = new List<Asset>();
			foreach (string path in datapaths)
			{
				try
				{
					if (!Directory.Exists(path))
					{
						continue;
					}
					foreach (string s in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
					{
						try
						{
							FileInfo f = new FileInfo(s);
							if (f.Name.Equals("thumbs.db", StringComparison.InvariantCultureIgnoreCase))
							{
								continue;
							}
							Asset a = new Asset();
							a.data = File.ReadAllBytes(s);
							a.dataLength = a.data.Length;
							a.name = f.Name.ToLowerInvariant();
							a.md5 = Md5(a.data);
							assets.Add(a);
						}
						catch
						{
						}
					}
				}
				catch
				{
				}
			}
			progress.value = 1;
			list.count = assets.Count;
			list.items = new Asset[2048];
			for (int i = 0; i < assets.Count; i++)
			{
				list.items[i] = assets[i];
			}
		}

		MD5CryptoServiceProvider sha1 = new MD5CryptoServiceProvider();
		string Md5(byte[] data)
		{
			string hash = ToHex(sha1.ComputeHash(data), false);
			return hash;
		}

		public static string ToHex(byte[] bytes, bool upperCase)
		{
			StringBuilder result = new StringBuilder(bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
			{
				result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
			}

			return result.ToString();
		}
	}
}
