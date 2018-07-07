using ManicDigger.Common;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace ManicDigger.ClientNative
{
	public class GamePlatformNative : GamePlatform
	{
		#region Primitive
		public override int FloatToInt(float value)
		{
			return (int)value;
		}

		public override float MathSin(float a)
		{
			return (float)Math.Sin(a);
		}

		public override float MathCos(float a)
		{
			return (float)Math.Cos(a);
		}

		public override float MathSqrt(float value)
		{
			return (float)System.Math.Sqrt(value);
		}

		public override float MathAcos(float p)
		{
			return (float)Math.Acos(p);
		}

		public override float MathTan(float p)
		{
			return (float)Math.Tan(p);
		}

		public override float FloatModulo(float a, int b)
		{
			return a % b;
		}




		public override int IntParse(string value)
		{
			return System.Int32.Parse(value);
		}

		public override bool IntTryParse(string s, IntRef ret)
		{
			int i;
			if (int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
			{
				ret.SetValue(i);
				return true;
			}
			else
			{
				return false;
			}
		}

		public override float FloatParse(string value)
		{
			return System.Single.Parse(value);
		}

		public override bool FloatTryParse(string s, FloatRef ret)
		{
			float f;
			if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
			{
				ret.SetValue(f);
				return true;
			}
			else
			{
				return false;
			}
		}

		public override string IntToString(int value)
		{
			return value.ToString();
		}

		public override string FloatToString(float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}


		public override string StringToLower(string p)
		{
			return p.ToLowerInvariant();
		}

		public override int[] StringToCharArray(string s, IntRef length)
		{
			if (s == null)
			{
				length.SetValue(0);
				return new int[0];
			}
			length.SetValue(s.Length);
			int[] charArray = new int[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				charArray[i] = s[i];
			}
			return charArray;
		}

		public override string CharArrayToString(int[] charArray, int length)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				s.Append((char)charArray[i]);
			}
			return s.ToString();
		}

		public override string[] StringSplit(string value, string separator, IntRef returnLength)
		{
			string[] ret = value.Split(new char[] { separator[0] });
			returnLength.SetValue(ret.Length);
			return ret;
		}

		public override string StringJoin(string[] value, string separator)
		{
			return string.Join(separator, value);
		}

		public override bool StringEmpty(string data)
		{
			return string.IsNullOrWhiteSpace(data);
		}

		public override string StringTrim(string value)
		{
			return value.Trim();
		}

		public override string StringFormat(string format, string arg0)
		{
			return string.Format(format, arg0);
		}

		public override string StringFormat2(string format, string arg0, string arg1)
		{
			return string.Format(format, arg0, arg1);
		}

		public override string StringFormat3(string format, string arg0, string arg1, string arg2)
		{
			return string.Format(format, arg0, arg1, arg2);
		}

		public override string StringFormat4(string format, string arg0, string arg1, string arg2, string arg3)
		{
			return string.Format(format, arg0, arg1, arg2, arg3);
		}

		public override byte[] StringToUtf8ByteArray(string s, IntRef retLength)
		{
			byte[] data = Encoding.UTF8.GetBytes(s);
			retLength.SetValue(data.Length);
			return data;
		}

		public override string StringFromUtf8ByteArray(byte[] value, int valueLength)
		{
			string s = Encoding.UTF8.GetString(value, 0, valueLength);
			return s;
		}

		public override bool StringContains(string a, string b)
		{
			return a.Contains(b);
		}

		public override string StringReplace(string s, string from, string to)
		{
			return s.Replace(from, to);
		}

		public override bool StringStartsWithIgnoreCase(string a, string b)
		{
			return a.StartsWith(b, StringComparison.InvariantCultureIgnoreCase);
		}

		public override int StringIndexOf(string s, string p)
		{
			return s.IndexOf(p);
		}

		#endregion

		#region Misc

		public GamePlatformNative()
		{
			System.Threading.ThreadPool.SetMinThreads(32, 32);
			System.Threading.ThreadPool.SetMaxThreads(128, 128);
			datapaths = new[] {
				Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "data"),
				"data"
			};
			start.Start();
		}

		public bool TouchTest = false;
		string[] datapaths;

		public override string Timestamp()
		{
			string time = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", System.DateTime.Now);
			return time;
		}

		public override void ClipboardSetText(string s)
		{
			System.Windows.Forms.Clipboard.SetText(s);
		}
		ManicDigger.Renderers.TextRenderer r = new ManicDigger.Renderers.TextRenderer();
		Dictionary<TextAndFont, SizeF> textsizes = new Dictionary<TextAndFont, SizeF>();
		public SizeF TextSize(string text, FontCi font)
		{
			SizeF size;
			if (textsizes.TryGetValue(new TextAndFont()
			{
				text = text,
				size = font.GetFontSize(),
				family = font.GetFontFamily(),
				style = font.GetFontStyle()
			}, out size))
			{
				return size;
			}
			size = textrenderer.MeasureTextSize(text, font);
			textsizes[new TextAndFont() { text = text, size = font.GetFontSize(), family = font.GetFontFamily(), style = font.GetFontStyle() }] = size;
			return size;
		}

		public override void TextSize(string text, FontCi font, IntRef outWidth, IntRef outHeight)
		{
			SizeF size = TextSize(text, font);
			outWidth.SetValue((int)size.Width);
			outHeight.SetValue((int)size.Height);
		}

		public override void Exit()
		{
			Environment.Exit(0);
		}

		public override bool ExitAvailable()
		{
			return true;
		}

		public override string PathSavegames()
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		}

		public override string PathCombine(string part1, string part2)
		{
			return Path.Combine(part1, part2);
		}

		public override string[] DirectoryGetFiles(string path, IntRef length)
		{
			if (!Directory.Exists(path))
			{
				length.SetValue(0);
				return new string[0];
			}
			string[] files = Directory.GetFiles(path);
			length.SetValue(files.Length);
			return files;
		}

		public override string[] FileReadAllLines(string path, IntRef length)
		{
			string[] lines = File.ReadAllLines(path);
			length.SetValue(lines.Length);
			return lines;
		}

		public override void WebClientDownloadDataAsync(string url, HttpResponseCi response)
		{
			DownloadDataArgs args = new DownloadDataArgs();
			args.url = url;
			args.response = response;
			ThreadPool.QueueUserWorkItem(DownloadData, args);
		}

		class DownloadDataArgs
		{
			public string url;
			public HttpResponseCi response;
		}

		void DownloadData(object o)
		{
			DownloadDataArgs args = (DownloadDataArgs)o;
			WebClient c = new WebClient();
			try
			{
				byte[] data = c.DownloadData(args.url);
				args.response.SetValue(data);
				args.response.SetValueLength(data.Length);
				args.response.SetDone(true);
			}
			catch
			{
				args.response.SetError(true);
			}
		}

		public override void ThumbnailDownloadAsync(string ip, int port, ThumbnailResponseCi response)
		{
			ThumbnailDownloadArgs args = new ThumbnailDownloadArgs();
			args.ip = ip;
			args.port = port;
			args.response = response;
			ThreadPool.QueueUserWorkItem(DownloadServerThumbnail, args);
		}

		void DownloadServerThumbnail(object o)
		{
			ThumbnailDownloadArgs args = (ThumbnailDownloadArgs)o;
			//Fetch server info from given adress
			QueryClient qClient = new QueryClient();
			qClient.SetPlatform(this);
			qClient.PerformQuery(args.ip, args.port);
			if (qClient.GetQuerySuccess())
			{
				//Received a result
				QueryResult r = qClient.GetResult();
				args.response.SetData(r.ServerThumbnail);
				args.response.SetDataLength(r.ServerThumbnail.Length);
				args.response.SetServerMessage(qClient.GetServerMessage());
				args.response.SetDone(true);
			}
			else
			{
				//Did not receive a response
				args.response.SetError(true);
			}
		}

		class ThumbnailDownloadArgs
		{
			public string ip;
			public int port;
			public ThumbnailResponseCi response;
		}

		public override string FileName(string fullpath)
		{
			FileInfo info = new FileInfo(fullpath);
			return info.Name.Replace(info.Extension, "");
		}

		public override string GetLanguageIso6391()
		{
			return CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
		}

		Stopwatch start = new Stopwatch();

		public override int TimeMillisecondsFromStart()
		{
			return (int)start.ElapsedMilliseconds;
		}

		public override void ThrowException(string message)
		{
			throw new Exception(message);
		}

		public override BitmapCi BitmapCreate(int width, int height)
		{
			BitmapCiCs bmp = new BitmapCiCs();
			bmp.bmp = new Bitmap(width, height);
			return bmp;
		}

		public override void BitmapSetPixelsArgb(BitmapCi bmp, int[] pixels)
		{
			BitmapCiCs bmp_ = (BitmapCiCs)bmp;
			int width = bmp_.bmp.Width;
			int height = bmp_.bmp.Height;
			if (IsMono)
			{
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						int color = pixels[x + y * width];
						bmp_.bmp.SetPixel(x, y, Color.FromArgb(color));
					}
				}
			}
			else
			{
				FastBitmap fastbmp = new FastBitmap();
				fastbmp.bmp = bmp_.bmp;
				fastbmp.Lock();
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						fastbmp.SetPixel(x, y, pixels[x + y * width]);
					}
				}
				fastbmp.Unlock();
			}
		}

		public override BitmapCi BitmapCreateFromPng(byte[] data, int dataLength)
		{
			BitmapCiCs bmp = new BitmapCiCs();
			try
			{
				bmp.bmp = new Bitmap(new MemoryStream(data, 0, dataLength));
			}
			catch
			{
				bmp.bmp = new Bitmap(1, 1);
				bmp.bmp.SetPixel(0, 0, Color.Orange);
			}
			return bmp;
		}

		public bool IsMono = Type.GetType("Mono.Runtime") != null;

		public override void BitmapGetPixelsArgb(BitmapCi bitmap, int[] bmpPixels)
		{
			BitmapCiCs bmp = (BitmapCiCs)bitmap;
			int width = bmp.bmp.Width;
			int height = bmp.bmp.Height;
			if (IsMono)
			{
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						bmpPixels[x + y * width] = bmp.bmp.GetPixel(x, y).ToArgb();
					}
				}
			}
			else
			{
				FastBitmap fastbmp = new FastBitmap();
				fastbmp.bmp = bmp.bmp;
				fastbmp.Lock();
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						bmpPixels[x + y * width] = fastbmp.GetPixel(x, y);
					}
				}
				fastbmp.Unlock();
			}
		}

		public override int LoadTextureFromBitmap(BitmapCi bmp)
		{
			BitmapCiCs bmp_ = (BitmapCiCs)bmp;
			return LoadTexture(bmp_.bmp, false);
		}

		ManicDigger.Renderers.TextRenderer textrenderer = new ManicDigger.Renderers.TextRenderer();

		public override BitmapCi CreateTextTexture(Text_ t)
		{
			Bitmap bmp = textrenderer.MakeTextTexture(t);
			return new BitmapCiCs() { bmp = bmp };
		}

		public override void SetTextRendererFont(int fontID)
		{
			textrenderer.SetFont(fontID);
		}

		public override float BitmapGetWidth(BitmapCi bmp)
		{
			BitmapCiCs bmp_ = (BitmapCiCs)bmp;
			return bmp_.bmp.Width;
		}

		public override float BitmapGetHeight(BitmapCi bmp)
		{
			BitmapCiCs bmp_ = (BitmapCiCs)bmp;
			return bmp_.bmp.Height;
		}

		public override void BitmapDelete(BitmapCi bmp)
		{
			BitmapCiCs bmp_ = (BitmapCiCs)bmp;
			bmp_.bmp.Dispose();
		}

		public override void ConsoleWriteLine(string s)
		{
			Console.WriteLine(s);
		}

		public override MonitorObject MonitorCreate()
		{
			return new MonitorObject();
		}

		public override void MonitorEnter(MonitorObject monitorObject)
		{
			System.Threading.Monitor.Enter(monitorObject);
		}

		public override void MonitorExit(MonitorObject monitorObject)
		{
			System.Threading.Monitor.Exit(monitorObject);
		}

		public override AviWriterCi AviWriterCreate()
		{
			AviWriterCiCs avi = new AviWriterCiCs();
			return avi;
		}

		public override UriCi ParseUri(string uri)
		{
			MyUri myuri = new MyUri(uri);

			UriCi ret = new UriCi();
			ret.SetUrl(myuri.Url);
			ret.SetIp(myuri.Ip);
			ret.SetPort(myuri.Port);
			ret.SetGet(new DictionaryStringString());
			foreach (var k in myuri.Get)
			{
				ret.GetGet().Set(k.Key, k.Value);
			}
			return ret;
		}

		public override RandomCi RandomCreate()
		{
			return new RandomNative();
		}

		public override string PathStorage()
		{
			return GameStorePath.GetStorePath();
		}

		public override string GetGameVersion()
		{
			return GameVersion.Version;
		}

		ICompression compression = new CompressionGzip();
		public override void GzipDecompress(byte[] compressed, int compressedLength, byte[] ret)
		{
			byte[] data = new byte[compressedLength];
			for (int i = 0; i < compressedLength; i++)
			{
				data[i] = compressed[i];
			}
			byte[] decompressed = compression.Decompress(data);
			for (int i = 0; i < decompressed.Length; i++)
			{
				ret[i] = decompressed[i];
			}
		}
		public override byte[] GzipCompress(byte[] data, int dataLength, IntRef retLength)
		{
			byte[] data_ = new byte[dataLength];
			for (int i = 0; i < dataLength; i++)
			{
				data_[i] = data[i];
			}
			byte[] compressed = compression.Compress(data_);
			retLength.SetValue(compressed.Length);
			return compressed;
		}
		public bool ENABLE_CHATLOG = true;
		public string gamepathlogs()
		{
			return Path.Combine(PathStorage(), "Logs");
		}
		private static string MakeValidFileName(string name)
		{
			string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
			string invalidReStr = string.Format(@"[{0}]", invalidChars);
			return Regex.Replace(name, invalidReStr, "_");
		}
		public override bool ChatLog(string servername, string p)
		{
			if (!ENABLE_CHATLOG)
			{
				return true;
			}
			if (!Directory.Exists(gamepathlogs()))
			{
				Directory.CreateDirectory(gamepathlogs());
			}
			string filename = Path.Combine(gamepathlogs(), MakeValidFileName(servername) + ".txt");
			try
			{
				File.AppendAllText(filename, string.Format("{0} {1}\n", DateTime.Now, p));
				return true;
			}
			catch
			{
				return false;
			}
		}

		public override bool IsValidTypingChar(int c_)
		{
			char c = (char)c_;
			return (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)
			|| char.IsPunctuation(c) || char.IsSeparator(c) || char.IsSymbol(c))
			&& c != '\r' && c != '\t';
		}

		public override void MessageBoxShowError(string text, string caption)
		{
			System.Windows.Forms.MessageBox.Show(text, caption, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
		}

		public override int ByteArrayLength(byte[] arr)
		{
			return arr.Length;
		}

		public override string[] ReadAllLines(string p, IntRef retCount)
		{
			List<string> lines = new List<string>();
			StringReader reader = new StringReader(p);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				lines.Add(line);
			}
			retCount.SetValue(lines.Count);
			return lines.ToArray();
		}

		public override bool ClipboardContainsText()
		{
			return Clipboard.ContainsText();
		}

		public override string ClipboardGetText()
		{
			return Clipboard.GetText();
		}

		public void SetExit(GameExit exit)
		{
			gameexit = exit;
		}

		class UploadData
		{
			public string url;
			public byte[] data;
			public int dataLength;
			public HttpResponseCi response;
		}

		public override void WebClientUploadDataAsync(string url, byte[] data, int dataLength, HttpResponseCi response)
		{
			UploadData d = new UploadData();
			d.url = url;
			d.data = data;
			d.dataLength = dataLength;
			d.response = response;
			System.Threading.ThreadPool.QueueUserWorkItem(DoUploadData, d);
		}

		void DoUploadData(object o)
		{
			UploadData d = (UploadData)o;
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(d.url);
				request.Method = "POST";
				request.Timeout = 15000; // 15s timeout
				request.ContentType = "application/x-www-form-urlencoded";
				request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

				request.ContentLength = d.dataLength;

				System.Net.ServicePointManager.Expect100Continue = false; // fixes lighthttpd 417 error

				using (Stream requestStream = request.GetRequestStream())
				{
					requestStream.Write(d.data, 0, d.dataLength);
					requestStream.Flush();
				}
				WebResponse response_ = request.GetResponse();

				MemoryStream m = new MemoryStream();
				using (Stream s = response_.GetResponseStream())
				{
					CopyTo(s, m);
				}
				d.response.SetValue(m.ToArray());
				d.response.SetValueLength(d.response.GetValue().Length);
				d.response.SetDone(true);

				request.Abort();

			}
			catch
			{
				d.response.SetError(true);
			}
		}

		public static void CopyTo(Stream source, Stream destination)
		{
			// TODO: Argument validation
			byte[] buffer = new byte[16384]; // For example...
			int bytesRead;
			while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
			{
				destination.Write(buffer, 0, bytesRead);
			}
		}

		public override string FileOpenDialog(string extension, string extensionName, string initialDirectory)
		{
			OpenFileDialog d = new OpenFileDialog();
			d.InitialDirectory = initialDirectory;
			d.FileName = "Default." + extension;
			d.Filter = string.Format("{1}|*.{0}|All files|*.*", extension, extensionName);
			d.CheckFileExists = false;
			d.CheckPathExists = true;
			string dir = System.Environment.CurrentDirectory;
			DialogResult result = d.ShowDialog();
			System.Environment.CurrentDirectory = dir;
			if (result == DialogResult.OK)
			{
				return d.FileName;
			}
			return null;
		}

		public override void ApplicationDoEvents()
		{
			if (IsMono)
			{
				Application.DoEvents();
				Thread.Sleep(0);
			}
		}

		public override void ThreadSpinWait(int iterations)
		{
			Thread.SpinWait(iterations);
		}

		public override void ShowKeyboard(bool show)
		{
		}

		public override bool IsFastSystem()
		{
			return true;
		}

		static string GetPreferencesFilePath()
		{
			string path = GameStorePath.GetStorePath();
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return Path.Combine(path, "Preferences.txt");
		}

		public override Preferences GetPreferences()
		{
			if (File.Exists(GetPreferencesFilePath()))
			{
				try
				{
					Preferences p = new Preferences();
					p.SetPlatform(this);
					string[] lines = File.ReadAllLines(GetPreferencesFilePath());
					foreach (string l in lines)
					{
						int a = l.IndexOf("=", StringComparison.InvariantCultureIgnoreCase);
						string name = l.Substring(0, a);
						string value = l.Substring(a + 1);
						p.SetString(name, value);
					}
					return p;
				}
				catch
				{
					File.Delete(GetPreferencesFilePath());
					Preferences p = new Preferences();
					p.SetPlatform(this);
					return p;
				}
			}
			else
			{
				Preferences p = new Preferences();
				p.SetPlatform(this);
				return p;
			}
		}

		public override void SetPreferences(Preferences preferences)
		{
			DictionaryStringString items = preferences.GetItems();
			List<string> lines = new List<string>();
			for (int i = 0; i < items.GetSize(); i++)
			{
				if (items.items[i] == null)
				{
					continue;
				}
				string key = items.items[i].key;
				string value = items.items[i].value;
				lines.Add(key + "=" + value);
			}
			try
			{
				File.WriteAllLines(GetPreferencesFilePath(), lines.ToArray());
			}
			catch
			{
			}
		}

		public bool IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;

		public override bool MultithreadingAvailable()
		{
			return true;
		}

		public override void QueueUserWorkItem(Action_ action)
		{
			ThreadPool.QueueUserWorkItem((a) =>
			{
				action.Run();
			});
		}

		AssetLoader assetloader;
		public override void LoadAssetsAsyc(AssetList list, FloatRef progress)
		{
			if (assetloader == null)
			{
				assetloader = new AssetLoader(datapaths);
			}
			assetloader.LoadAssetsAsync(list, progress);
		}

		public override bool IsSmallScreen()
		{
			return TouchTest;
		}

		public override void OpenLinkInBrowser(string url)
		{
			if (!(url.StartsWith("http://") || url.StartsWith("https://")))
			{
				//Check if string is an URL - if not, abort
				return;
			}
			Process.Start(url);
		}

		public string cachepath()
		{
			return Path.Combine(PathStorage(), "Cache");
		}
		public void checkcachedir()
		{
			if (!Directory.Exists(cachepath()))
			{
				Directory.CreateDirectory(cachepath());
			}
		}

		public override void SaveAssetToCache(Asset tosave)
		{
			//Check if cache directory exists
			checkcachedir();
			BinaryWriter bw = new BinaryWriter(File.Create(Path.Combine(cachepath(), tosave.md5)));
			bw.Write(tosave.name);
			bw.Write(tosave.dataLength);
			bw.Write(tosave.data);
			bw.Close();
		}

		public override Asset LoadAssetFromCache(string md5)
		{
			//Check if cache directory exists
			checkcachedir();
			BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(cachepath(), md5)));
			string contentName = br.ReadString();
			int contentLength = br.ReadInt32();
			byte[] content = br.ReadBytes(contentLength);
			br.Close();
			Asset a = new Asset();
			a.data = content;
			a.dataLength = contentLength;
			a.md5 = md5;
			a.name = contentName;
			return a;
		}

		public override bool IsCached(string md5)
		{
			if (!Directory.Exists(cachepath()))
				return false;
			return File.Exists(Path.Combine(cachepath(), md5));
		}

		public override bool IsChecksum(string checksum)
		{
			//Check if checksum string has correct length
			if (checksum.Length != 32)
			{
				return false;
			}
			//Convert checksum string to lowercase letters
			checksum = checksum.ToLower();
			char[] chars = checksum.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if ((chars[i] < '0' || chars[i] > '9') && (chars[i] < 'a' || chars[i] > 'f'))
				{
					//Return false if any character inside the checksum is not hexadecimal
					return false;
				}
			}
			//Return true if all checks have been passed
			return true;
		}

		public override string DecodeHTMLEntities(string htmlencodedstring)
		{
			return System.Web.HttpUtility.HtmlDecode(htmlencodedstring);
		}

		public override bool IsDebuggerAttached()
		{
			return System.Diagnostics.Debugger.IsAttached;
		}

		public override string QueryStringValue(string key)
		{
			return null;
		}

		#endregion

		#region Audio

		AudioOpenAl audio;
		public GameExit gameexit;
		void StartAudio()
		{
			if (audio == null)
			{
				audio = new AudioOpenAl();
				audio.d_GameExit = gameexit;
			}
		}

		public override AudioData AudioDataCreate(byte[] data, int dataLength)
		{
			StartAudio();
			return audio.GetSampleFromArray(data);
		}

		public override bool AudioDataLoaded(AudioData data)
		{
			return true;
		}

		public override AudioCi AudioCreate(AudioData data)
		{
			return audio.CreateAudio((AudioDataCs)data);
		}

		public override void AudioPlay(AudioCi audio_)
		{
			StartAudio();
			((AudioOpenAl.AudioTask)audio_).Play();
		}

		public override void AudioPause(AudioCi audio_)
		{
			((AudioOpenAl.AudioTask)audio_).Pause();
		}

		public override void AudioDelete(AudioCi audio_)
		{
			((AudioOpenAl.AudioTask)audio_).Stop();
		}

		public override bool AudioFinished(AudioCi audio_)
		{
			return ((AudioOpenAl.AudioTask)audio_).Finished;
		}

		public override void AudioSetPosition(AudioCi audio_, float x, float y, float z)
		{
			((AudioOpenAl.AudioTask)audio_).position = new Vector3(x, y, z);
		}

		public override void AudioUpdateListener(float posX, float posY, float posZ, float orientX, float orientY, float orientZ)
		{
			StartAudio();
			audio.UpdateListener(new Vector3(posX, posY, posZ), new Vector3(orientX, orientY, orientZ));
		}

		#endregion

		#region Tcp
		public override bool TcpAvailable()
		{
			return true;
		}

		public override void TcpConnect(string ip, int port, BoolRef connected)
		{
			// FIXME: This code causes a SocketException when called multiple times.
			// This effectively crashes the game in multiplayer server selection.
			// Reason for this is that only a single socket exists to handle connections.

			this.connected = connected;
			sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			sock.NoDelay = true;
			sock.BeginConnect(ip, port, OnConnect, sock);
		}
		Socket sock;
		BoolRef connected;
		TcpConnectionRaw c;
		void OnConnect(IAsyncResult result)
		{
			Socket sock = (Socket)result.AsyncState;
			c = new TcpConnectionRaw(sock);
			c.ReceivedData += new EventHandler<MessageEventArgs>(c_ReceivedData);
			if (tosend.Count > 0)
			{
				c.Send(tosend.ToArray());
				tosend.Clear();
			}
			connected.SetValue(true);
		}

		void c_ReceivedData(object sender, MessageEventArgs e)
		{
			lock (received)
			{
				for (int i = 0; i < e.data.Length; i++)
				{
					received.Enqueue(e.data[i]);
				}
			}
		}
		Queue<byte> tosend = new Queue<byte>();
		public override void TcpSend(byte[] data, int length)
		{
			if (c == null)
			{
				for (int i = 0; i < length; i++)
				{
					tosend.Enqueue(data[i]);
				}
			}
			else
			{
				byte[] data1 = new byte[length];
				for (int i = 0; i < length; i++)
				{
					data1[i] = data[i];
				}
				c.Send(data1);
			}
		}
		Queue<byte> received = new Queue<byte>();
		public override int TcpReceive(byte[] data, int dataLength)
		{
			if (c == null)
			{
				return 0;
			}
			int total = 0;
			lock (received)
			{
				for (int i = 0; i < dataLength; i++)
				{
					if (received.Count == 0)
					{
						break;
					}
					data[i] = received.Dequeue();
					total++;
				}
			}
			return total;
		}

		#endregion

		#region Enet
		public override bool EnetAvailable()
		{
			return true;
		}

		public override EnetHost EnetCreateHost()
		{
			return new EnetHostNative() { host = new ENet.Host() };
		}

		public override bool EnetHostService(EnetHost host, int timeout, EnetEventRef enetEvent)
		{
			EnetHostNative host_ = (EnetHostNative)host;
			ENet.Event e;
			bool ret = host_.host.Service(timeout, out e);
			EnetEventNative ee = new EnetEventNative(e);
			enetEvent.SetEvent(ee);
			return ret;
		}

		public override bool EnetHostCheckEvents(EnetHost host, EnetEventRef event_)
		{
			EnetHostNative host_ = (EnetHostNative)host;
			ENet.Event e;
			bool ret = host_.host.CheckEvents(out e);
			EnetEventNative ee = new EnetEventNative(e);
			event_.SetEvent(ee);
			return ret;
		}

		public override EnetPeer EnetHostConnect(EnetHost host, string hostName, int port, int data, int channelLimit)
		{
			EnetHostNative host_ = (EnetHostNative)host;
			ENet.Peer peer = host_.host.Connect(hostName, port, data, channelLimit);
			EnetPeerNative peer_ = new EnetPeerNative();
			peer_.peer = peer;
			return peer_;
		}

		public override void EnetPeerSend(EnetPeer peer, byte channelID, byte[] data, int dataLength, int flags)
		{
			try
			{
				EnetPeerNative peer_ = (EnetPeerNative)peer;
				peer_.peer.Send(channelID, data, (ENet.PacketFlags)flags);
			}
			catch
			{
			}
		}

		public override void EnetHostInitialize(EnetHost host, IPEndPointCi address, int peerLimit, int channelLimit, int incomingBandwidth, int outgoingBandwidth)
		{
			if (address != null)
			{
				throw new Exception();
			}
			EnetHostNative host_ = (EnetHostNative)host;
			host_.host.Initialize(null, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);
		}
		#endregion

		#region WebSocket

		public override bool WebSocketAvailable()
		{
			return false;
		}

		public override void WebSocketConnect(string ip, int port)
		{
		}

		public override void WebSocketSend(byte[] data, int dataLength)
		{
		}

		public override int WebSocketReceive(byte[] data, int dataLength)
		{
			return -1;
		}

		#endregion

		#region OpenGlImpl

		public GameWindow window;

		public override int GetCanvasWidth()
		{
			return window.Width;
		}

		public override int GetCanvasHeight()
		{
			return window.Height;
		}

		public void Start()
		{
			window.KeyDown += new EventHandler<KeyboardKeyEventArgs>(game_KeyDown);
			window.KeyUp += new EventHandler<KeyboardKeyEventArgs>(game_KeyUp);
			window.KeyPress += new EventHandler<OpenTK.KeyPressEventArgs>(game_KeyPress);
			window.MouseDown += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonDown);
			window.MouseUp += new EventHandler<MouseButtonEventArgs>(Mouse_ButtonUp);
			window.MouseMove += new EventHandler<MouseMoveEventArgs>(Mouse_Move);
			window.MouseWheel += new EventHandler<OpenTK.Input.MouseWheelEventArgs>(Mouse_WheelChanged);
			window.RenderFrame += new EventHandler<OpenTK.FrameEventArgs>(window_RenderFrame);
			window.Closed += new EventHandler<EventArgs>(window_Closed);
			window.Resize += new EventHandler<EventArgs>(window_Resized);
			window.TargetRenderFrequency = 0;
			window.Title = "Manic Digger";
		}

		void window_Closed(object sender, EventArgs e)
		{
			gameexit.SetExit(true);
		}

		void window_Resized(object sender, EventArgs e)
		{
			Size sizeLimit = new Size(1280, 720);
			if (window.Width < sizeLimit.Width)
			{
				window.Width = sizeLimit.Width;
			}
			if (window.Height < sizeLimit.Height)
			{
				window.Height = sizeLimit.Height;
			}
		}

		public override void SetVSync(bool enabled)
		{
			window.VSync = enabled ? VSyncMode.On : VSyncMode.Off;
		}

		Screenshot screenshot = new Screenshot();

		public override void SaveScreenshot()
		{
			screenshot.d_GameWindow = window;
			screenshot.SaveScreenshot();
		}

		public override BitmapCi GrabScreenshot()
		{
			screenshot.d_GameWindow = window;
			Bitmap bmp = screenshot.GrabScreenshot();
			BitmapCiCs bmp_ = new BitmapCiCs();
			bmp_.bmp = bmp;
			return bmp_;
		}

		public override void WindowExit()
		{
			if (gameexit != null)
			{
				gameexit.SetExit(true);
			}
			window.Exit();
		}

		public override void SetTitle(string applicationname)
		{
			window.Title = applicationname;
		}

		public override string KeyName(int key)
		{
			if (Enum.IsDefined(typeof(OpenTK.Input.Key), key))
			{
				string s = Enum.GetName(typeof(OpenTK.Input.Key), key);
				return s;
			}
			//if (Enum.IsDefined(typeof(SpecialKey), key))
			//{
			//    string s = Enum.GetName(typeof(SpecialKey), key);
			//    return s;
			//}
			return key.ToString();
		}
		DisplayResolutionCi[] resolutions;
		int resolutionsCount;
		public override DisplayResolutionCi[] GetDisplayResolutions(IntRef retResolutionsCount)
		{
			if (resolutions == null)
			{
				resolutions = new DisplayResolutionCi[1024];
				foreach (var r in DisplayDevice.Default.AvailableResolutions)
				{
					if (r.Width < 800 || r.Height < 600 || r.BitsPerPixel < 16)
					{
						continue;
					}
					DisplayResolutionCi r2 = new DisplayResolutionCi();
					r2.Width = r.Width;
					r2.Height = r.Height;
					r2.BitsPerPixel = r.BitsPerPixel;
					r2.RefreshRate = r.RefreshRate;
					resolutions[resolutionsCount++] = r2;
				}
			}
			retResolutionsCount.SetValue(resolutionsCount);
			return resolutions;
		}

		public override WindowState GetWindowState()
		{
			return (WindowState)window.WindowState;
		}

		public override void SetWindowState(WindowState value)
		{
			window.WindowState = (OpenTK.WindowState)value;
		}

		public override void ChangeResolution(int width, int height, int bitsPerPixel, float refreshRate)
		{
			DisplayDevice.Default.ChangeResolution(width, height, bitsPerPixel, refreshRate);
		}

		public override DisplayResolutionCi GetDisplayResolutionDefault()
		{
			DisplayDevice d = DisplayDevice.Default;
			DisplayResolutionCi r = new DisplayResolutionCi();
			r.Width = d.Width;
			r.Height = d.Height;
			r.BitsPerPixel = d.BitsPerPixel;
			r.RefreshRate = d.RefreshRate;
			return r;
		}

		#endregion

		#region OpenGl
		public override void GlViewport(int x, int y, int width, int height)
		{
			GL.Viewport(x, y, width, height);
		}

		public override void GlClearColorBufferAndDepthBuffer()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public override void GlDisableDepthTest()
		{
			GL.Disable(EnableCap.DepthTest);
		}

		public override void BindTexture2d(int texture)
		{
			GL.BindTexture(TextureTarget.Texture2D, texture);
		}

		float[] xyz = new float[65536 * 3];
		float[] uv = new float[65536 * 2];
		byte[] rgba = new byte[65536 * 4];
		ushort[] indices = new ushort[65536];

		public override Model CreateModel(ModelData data)
		{
			int id = GL.GenLists(1);

			GL.NewList(id, ListMode.Compile);

			DrawModelData(data);

			GL.EndList();
			DisplayListModel m = new DisplayListModel();
			m.listId = id;
			return m;
		}

		public override void DrawModelData(ModelData data)
		{
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.ColorArray);
			GL.EnableClientState(ArrayCap.TextureCoordArray);

			float[] dataXyz = data.getXyz();
			float[] dataUv = data.getUv();
			byte[] dataRgba = data.getRgba();

			for (int i = 0; i < data.GetXyzCount(); i++)
			{
				xyz[i] = dataXyz[i];
			}
			for (int i = 0; i < data.GetUvCount(); i++)
			{
				uv[i] = dataUv[i];
			}
			if (dataRgba == null)
			{
				for (int i = 0; i < data.GetRgbaCount(); i++)
				{
					rgba[i] = 255;
				}
			}
			else
			{
				for (int i = 0; i < data.GetRgbaCount(); i++)
				{
					rgba[i] = dataRgba[i];
				}
			}
			GL.VertexPointer(3, VertexPointerType.Float, 3 * 4, xyz);
			GL.ColorPointer(4, ColorPointerType.UnsignedByte, 4 * 1, rgba);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 2 * 4, uv);

			BeginMode beginmode = BeginMode.Triangles;
			if (data.getMode() == DrawModeEnum.Triangles)
			{
				beginmode = BeginMode.Triangles;
				GL.Enable(EnableCap.Texture2D);
			}
			else if (data.getMode() == DrawModeEnum.Lines)
			{
				beginmode = BeginMode.Lines;
				GL.Disable(EnableCap.Texture2D);
			}
			else
			{
				throw new Exception();
			}

			int[] dataIndices = data.getIndices();
			for (int i = 0; i < data.GetIndicesCount(); i++)
			{
				indices[i] = (ushort)dataIndices[i];
			}

			GL.DrawElements(beginmode, data.GetIndicesCount(), DrawElementsType.UnsignedShort, indices);

			GL.DisableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.ColorArray);
			GL.DisableClientState(ArrayCap.TextureCoordArray);
			GL.Disable(EnableCap.Texture2D);
		}

		class DisplayListModel : Model
		{
			public int listId;
		}

		public override void DrawModel(Model model)
		{
			GL.CallList(((DisplayListModel)model).listId);
		}

		int[] lists = new int[1024];

		public override void DrawModels(Model[] model, int count)
		{
			if (lists.Length < count)
			{
				lists = new int[count * 2];
			}
			for (int i = 0; i < count; i++)
			{
				lists[i] = ((DisplayListModel)model[i]).listId;
			}
			GL.CallLists(count, ListNameType.Int, lists);
		}

		public override void InitShaders()
		{
		}

		public override void SetMatrixUniformProjection(float[] pMatrix)
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(pMatrix);
		}

		public override void SetMatrixUniformModelView(float[] mvMatrix)
		{
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(mvMatrix);
		}

		public override void GlClearColorRgbaf(float r, float g, float b, float a)
		{
			GL.ClearColor(r, g, b, a);
		}

		public override void GlEnableDepthTest()
		{
			GL.Enable(EnableCap.DepthTest);
		}

		public bool ALLOW_NON_POWER_OF_TWO = false;
		public bool ENABLE_MIPMAPS = true;
		public bool ENABLE_TRANSPARENCY = true;

		//http://www.opentk.com/doc/graphics/textures/loading
		public int LoadTexture(Bitmap bmpArg, bool linearMag)
		{
			Bitmap bmp = bmpArg;
			bool convertedbitmap = false;
			if ((!ALLOW_NON_POWER_OF_TWO) &&
				(!(BitTools.IsPowerOfTwo(bmp.Width) && BitTools.IsPowerOfTwo(bmp.Height))))
			{
				Bitmap bmp2 = new Bitmap(BitTools.NextPowerOfTwo(bmp.Width),
								  BitTools.NextPowerOfTwo(bmp.Height));
				using (Graphics g = Graphics.FromImage(bmp2))
				{
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					g.DrawImage(bmp, 0, 0, bmp2.Width, bmp2.Height);
				}
				convertedbitmap = true;
				bmp = bmp2;
			}
			GL.Enable(EnableCap.Texture2D);
			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, id);
			if (!ENABLE_MIPMAPS)
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			}
			else
			{
				//GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); //DOES NOT WORK ON ATI GRAPHIC CARDS
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1); //DOES NOT WORK ON ???
				int[] MipMapCount = new int[1];
				GL.GetTexParameter(TextureTarget.Texture2D, GetTextureParameter.TextureMaxLevel, out MipMapCount[0]);
				if (MipMapCount[0] == 0)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
				}
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, linearMag ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 4);
			}
			BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

			bmp.UnlockBits(bmp_data);

			GL.Enable(EnableCap.DepthTest);

			if (ENABLE_TRANSPARENCY)
			{
				GL.Enable(EnableCap.AlphaTest);
				GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
			}


			if (ENABLE_TRANSPARENCY)
			{
				GL.Enable(EnableCap.Blend);
				GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
				//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
				//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new Color4(0, 0, 0, byte.MaxValue));
			}

			if (convertedbitmap)
			{
				bmp.Dispose();
			}
			return id;
		}

		public override void GlDisableCullFace()
		{
			GL.Disable(EnableCap.CullFace);
		}

		public override void GlEnableCullFace()
		{
			GL.Enable(EnableCap.CullFace);
		}

		public override void DeleteModel(Model model)
		{
			DisplayListModel m = (DisplayListModel)model;
			GL.DeleteLists(m.listId, 1);
		}

		public override void GlEnableTexture2d()
		{
			GL.Enable(EnableCap.Texture2D);
		}

		public override void GlDisableTexture2d()
		{
			GL.Disable(EnableCap.Texture2D);
		}

		public override void GLLineWidth(int width)
		{
			GL.LineWidth(width);
		}

		public override void GLDisableAlphaTest()
		{
			GL.Disable(EnableCap.AlphaTest);
		}

		public override void GLEnableAlphaTest()
		{
			GL.Enable(EnableCap.AlphaTest);
		}

		public override void GLDeleteTexture(int id)
		{
			GL.DeleteTexture(id);
		}

		public override void GlClearDepthBuffer()
		{
			GL.Clear(ClearBufferMask.DepthBufferBit);
		}

		public override void GlLightModelAmbient(int r, int g, int b)
		{
			float mult = 1f;
			float[] global_ambient = new float[] {
				(float)r / 255f * mult,
				(float)g / 255f * mult,
				(float)b / 255f * mult,
				1f
			};
			GL.LightModel(LightModelParameter.LightModelAmbient, global_ambient);
		}

		public override void GlEnableFog()
		{
			GL.Enable(EnableCap.Fog);
		}

		public override void GlHintFogHintNicest()
		{
			GL.Hint(HintTarget.FogHint, HintMode.Nicest);
		}

		public override void GlFogFogModeExp2()
		{
			GL.Fog(FogParameter.FogMode, (int)FogMode.Exp2);
		}

		public override void GlFogFogColor(int r, int g, int b, int a)
		{
			float[] fogColor = new[] {
				(float)r / 255,
				(float)g / 255,
				(float)b / 255,
				(float)a / 255
			};
			GL.Fog(FogParameter.FogColor, fogColor);
		}

		public override void GlFogFogDensity(float density)
		{
			GL.Fog(FogParameter.FogDensity, density);
		}

		public override int GlGetMaxTextureSize()
		{
			int size = 1024;
			try
			{
				GL.GetInteger(GetPName.MaxTextureSize, out size);
			}
			catch
			{
			}
			return size;
		}

		public override void GlDepthMask(bool flag)
		{
			GL.DepthMask(flag);
		}

		public override void GlCullFaceBack()
		{
			GL.CullFace(CullFaceMode.Back);
		}

		public override void GlEnableLighting()
		{
			GL.Enable(EnableCap.Lighting);
		}

		public override void GlEnableColorMaterial()
		{
			GL.Enable(EnableCap.ColorMaterial);
		}

		public override void GlColorMaterialFrontAndBackAmbientAndDiffuse()
		{
			GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
		}

		public override void GlShadeModelSmooth()
		{
			GL.ShadeModel(ShadingModel.Smooth);
		}

		public override void GlDisableFog()
		{
			GL.Disable(EnableCap.Fog);
		}

		public override void GlActiveTexture(int textureUnit)
		{
			switch (textureUnit)
			{
				case 0:
					GL.ActiveTexture(TextureUnit.Texture0);
					break;
				case 1:
					GL.ActiveTexture(TextureUnit.Texture1);
					break;
				case 2:
					GL.ActiveTexture(TextureUnit.Texture2);
					break;
				case 3:
					GL.ActiveTexture(TextureUnit.Texture3);
					break;
				default:
					throw new NotImplementedException("Only four texture units are supported currently.");
			}
		}

		public override GlProgram GlCreateProgram()
		{
			return new GlProgramNative() { id = GL.CreateProgram() };
		}

		public override void GlDeleteProgram(GlProgram program)
		{
			GlProgramNative p = (GlProgramNative)program;
			GL.DeleteProgram(p.id);
		}

		public override GlShader GlCreateShader(ShaderType shaderType)
		{
			OpenTK.Graphics.OpenGL.ShaderType glShaderType;
			switch (shaderType)
			{
				case ShaderType.VertexShader:
					glShaderType = OpenTK.Graphics.OpenGL.ShaderType.VertexShader;
					break;
				case ShaderType.FragmentShader:
				default:
					glShaderType = OpenTK.Graphics.OpenGL.ShaderType.FragmentShader;
					break;
			}
			return new GLShaderNative() { id = GL.CreateShader(glShaderType) };
		}

		public override void GlShaderSource(GlShader shader, string source)
		{
			GLShaderNative s = (GLShaderNative)shader;
			GL.ShaderSource(s.id, source);
		}

		public override void GlCompileShader(GlShader shader)
		{
			GLShaderNative s = (GLShaderNative)shader;
			GL.CompileShader(s.id);
		}

		public override bool GlGetShaderCompileStatus(GlShader shader)
		{
			GLShaderNative s = (GLShaderNative)shader;
			int status_code;
			GL.GetShader(s.id, ShaderParameter.CompileStatus, out status_code);
			return (1 == status_code);
		}

		public override string GlGetShaderInfoLog(GlShader shader)
		{
			GLShaderNative s = (GLShaderNative)shader;
			return GL.GetShaderInfoLog(s.id);
		}

		public override void GlAttachShader(GlProgram program, GlShader shader)
		{
			GlProgramNative p = (GlProgramNative)program;
			GLShaderNative s = (GLShaderNative)shader;
			GL.AttachShader(p.id, s.id);
		}

		public override void GlUseProgram(GlProgram program)
		{
			if (program == null)
			{
				GL.UseProgram(0);
				return;
			}
			GlProgramNative p = (GlProgramNative)program;
			GL.UseProgram(p.id);
		}

		public override int GlGetUniformLocation(GlProgram program, string name)
		{
			GlProgramNative p = (GlProgramNative)program;
			return GL.GetUniformLocation(p.id, name);
		}

		public override void GlLinkProgram(GlProgram program)
		{
			GlProgramNative p = (GlProgramNative)program;
			GL.LinkProgram(p.id);
		}

		public override bool GlGetProgramLinkStatus(GlProgram program)
		{
			GlProgramNative p = (GlProgramNative)program;
			int status_code;
			GL.GetProgram(p.id, GetProgramParameterName.LinkStatus, out status_code);
			return (1 == status_code);
		}

		public override string GlGetProgramInfoLog(GlProgram program)
		{
			GlProgramNative p = (GlProgramNative)program;
			return GL.GetProgramInfoLog(p.id);
		}

		public override string GlGetStringSupportedShadingLanguage()
		{
			return GL.GetString(StringName.ShadingLanguageVersion);
		}

		public override void GlUniform1i(int location, int v0)
		{
			GL.Uniform1(location, v0);
		}

		public override void GlUniform1f(int location, float v0)
		{
			GL.Uniform1(location, v0);
		}

		public override void GlUniform2f(int location, float v0, float v1)
		{
			GL.Uniform2(location, v0, v1);
		}

		public override void GlUniform3f(int location, float v0, float v1, float v2)
		{
			GL.Uniform3(location, v0, v1, v2);
		}

		public override void GlUniform4f(int location, float v0, float v1, float v2, float v3)
		{
			GL.Uniform4(location, v0, v1, v2, v3);
		}

		public override void GlUniformArray1f(int location, int count, float[] values)
		{
			GL.Uniform2(location, count, values);
		}

		#endregion

		#region Game

		bool singlePlayerServerAvailable = true;
		public override bool SinglePlayerServerAvailable()
		{
			return singlePlayerServerAvailable;
		}

		public override void SinglePlayerServerStart(string saveFilename)
		{
			singlepLayerServerExit = false;
			StartSinglePlayerServer(saveFilename);
		}

		public bool singlepLayerServerExit;
		public override void SinglePlayerServerExit()
		{
			singlepLayerServerExit = true;
		}

		public System.Action<string> StartSinglePlayerServer;
		public bool singlePlayerServerLoaded;

		public override bool SinglePlayerServerLoaded()
		{
			return singlePlayerServerLoaded;
		}
		public DummyNetwork singlePlayerServerDummyNetwork;
		public override DummyNetwork SinglePlayerServerGetNetwork()
		{
			return singlePlayerServerDummyNetwork;
		}

		public override void SinglePlayerServerDisable()
		{
			singlePlayerServerAvailable = false;
		}

		public override PlayerInterpolationState CastToPlayerInterpolationState(InterpolatedObject a)
		{
			return (PlayerInterpolationState)a;
		}

		#endregion

		#region Translation

		public override bool LanguageNativeAvailable()
		{
			return true;
		}

		public override Language GetLanguageHandler()
		{
			return new LanguageNative();
		}

		#endregion

		#region Event handlers

		public List<NewFrameHandler> newFrameHandlers = new List<NewFrameHandler>();
		public override void AddOnNewFrame(NewFrameHandler handler)
		{
			newFrameHandlers.Add(handler);
		}

		public List<KeyEventHandler> keyEventHandlers = new List<KeyEventHandler>();
		public override void AddOnKeyEvent(KeyEventHandler handler)
		{
			keyEventHandlers.Add(handler);
		}

		public List<MouseEventHandler> mouseEventHandlers = new List<MouseEventHandler>();
		public override void AddOnMouseEvent(MouseEventHandler handler)
		{
			mouseEventHandlers.Add(handler);
		}

		public List<TouchEventHandler> touchEventHandlers = new List<TouchEventHandler>();
		public override void AddOnTouchEvent(TouchEventHandler handler)
		{
			touchEventHandlers.Add(handler);
		}

		public CrashReporter crashreporter;
		OnCrashHandler onCrashHandler;
		public override void AddOnCrash(OnCrashHandler handler)
		{
#if !DEBUG
			crashreporter.OnCrash = OnCrash;
			onCrashHandler = handler;
#endif
		}
		void OnCrash()
		{
			if (onCrashHandler != null)
			{
				onCrashHandler.OnCrash();
			}
		}

		#endregion

		#region Input

		bool mousePointerLocked;
		bool mouseCursorVisible = true;
		MouseState current, previous;
		int lastX, lastY;

		public override bool IsMousePointerLocked()
		{
			return mousePointerLocked;
		}

		public override bool MouseCursorIsVisible()
		{
			return mouseCursorVisible;
		}

		public override void SetWindowCursor(int hotx, int hoty, int sizex, int sizey, byte[] imgdata, int imgdataLength)
		{
			try
			{
				Bitmap bmp = new Bitmap(new MemoryStream(imgdata, 0, imgdataLength)); //new Bitmap("data/local/gui/mousecursor.png");
				if (bmp.Width > 32 || bmp.Height > 32)
				{
					// Limit cursor size to 32x32
					return;
				}
				// Convert to required 0xBBGGRRAA format - see https://github.com/opentk/opentk/pull/107#issuecomment-41771702
				int i = 0;
				byte[] data = new byte[4 * bmp.Width * bmp.Height];
				for (int y = 0; y < bmp.Width; y++)
				{
					for (int x = 0; x < bmp.Height; x++)
					{
						Color color = bmp.GetPixel(x, y);
						data[i] = color.B;
						data[i + 1] = color.G;
						data[i + 2] = color.R;
						data[i + 3] = color.A;
						i += 4;
					}
				}
				bmp.Dispose();
				window.Cursor = new MouseCursor(hotx, hoty, sizex, sizey, data);
			}
			catch
			{
				RestoreWindowCursor();
			}
		}

		public override void RestoreWindowCursor()
		{
			window.Cursor = MouseCursor.Default;
		}

		public static int ToGlKey(OpenTK.Input.Key key)
		{
			return (int)key;
		}

		public override void MouseCursorSetVisible(bool value)
		{
			if (!value)
			{
				if (TouchTest)
				{
					return;
				}
				if (!mouseCursorVisible)
				{
					//Cursor already hidden. Do nothing.
					return;
				}
				window.CursorVisible = false;
				mouseCursorVisible = false;
			}
			else
			{
				if (mouseCursorVisible)
				{
					//Cursor already visible. Do nothing.
					return;
				}
				window.CursorVisible = true;
				mouseCursorVisible = true;
			}
		}

		public override void RequestMousePointerLock()
		{
			MouseCursorSetVisible(false);
			mousePointerLocked = true;
		}

		public override void ExitMousePointerLock()
		{
			MouseCursorSetVisible(true);
			mousePointerLocked = false;
		}

		public override bool Focused()
		{
			return window.Focused;
		}

		void window_RenderFrame(object sender, OpenTK.FrameEventArgs e)
		{
			UpdateMousePosition();
			foreach (NewFrameHandler h in newFrameHandlers)
			{
				NewFrameEventArgs args = new NewFrameEventArgs();
				args.SetDt((float)e.Time);
				h.OnNewFrame(args);
			}
			window.SwapBuffers();
		}

		void UpdateMousePosition()
		{
			current = Mouse.GetState();
			if (!window.Focused)
			{
				return;
			}
			if (current != previous)
			{
				// Mouse state has changed
				int xdelta = current.X - previous.X;
				int ydelta = current.Y - previous.Y;
				foreach (MouseEventHandler h in mouseEventHandlers)
				{
					MouseEventArgs args = new MouseEventArgs();
					args.SetX(lastX);
					args.SetY(lastY);
					args.SetMovementX(xdelta);
					args.SetMovementY(ydelta);
					args.SetEmulated(true);
					h.OnMouseMove(args);
				}
			}
			previous = current;
			if (mousePointerLocked)
			{
				/*
				* Windows: OK
				* Cursor hides properly
				* Cursor is trapped inside window
				* Centering works
				*
				* Linux: Needs workaround
				* Cursor hides properly
				* Cursor is trapped inside window
				* Centering broken
				*
				* Mac OS X: OK
				* Cursor hides properly (although visible when doing Skype screencast)
				* Centering works
				* Opening "mission control" by gesture does not free cursor
				*/

				int centerx = window.Bounds.Left + (window.Bounds.Width / 2);
				int centery = window.Bounds.Top + (window.Bounds.Height / 2);

				// Setting cursor position this way works on Windows and Mac
				Mouse.SetPosition(centerx, centery);
			}
		}

		void Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
		{
			foreach (MouseEventHandler h in mouseEventHandlers)
			{
				MouseWheelEventArgs args = new MouseWheelEventArgs();
				args.SetDelta(e.Delta);
				args.SetDeltaPrecise(e.DeltaPrecise);
				h.OnMouseWheel(args);
			}
		}

		void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (TouchTest)
			{
				foreach (TouchEventHandler h in touchEventHandlers)
				{
					TouchEventArgs args = new TouchEventArgs();
					args.SetX(e.X);
					args.SetY(e.Y);
					args.SetId(0);
					h.OnTouchStart(args);
				}
			}
			else
			{
				foreach (MouseEventHandler h in mouseEventHandlers)
				{
					MouseEventArgs args = new MouseEventArgs();
					args.SetX(e.X);
					args.SetY(e.Y);
					args.SetButton((int)e.Button);
					h.OnMouseDown(args);
				}
			}
		}

		void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (TouchTest)
			{
				foreach (TouchEventHandler h in touchEventHandlers)
				{
					TouchEventArgs args = new TouchEventArgs();
					args.SetX(e.X);
					args.SetY(e.Y);
					args.SetId(0);
					h.OnTouchEnd(args);
				}
			}
			else
			{
				foreach (MouseEventHandler h in mouseEventHandlers)
				{
					MouseEventArgs args = new MouseEventArgs();
					args.SetX(e.X);
					args.SetY(e.Y);
					args.SetButton((int)e.Button);
					h.OnMouseUp(args);
				}
			}
		}

		void Mouse_Move(object sender, MouseMoveEventArgs e)
		{
			lastX = e.X;
			lastY = e.Y;
			if (TouchTest)
			{
				foreach (TouchEventHandler h in touchEventHandlers)
				{
					TouchEventArgs args = new TouchEventArgs();
					args.SetX(e.X);
					args.SetY(e.Y);
					args.SetId(0);
					h.OnTouchMove(args);
				}
			}
			else
			{
				foreach (MouseEventHandler h in mouseEventHandlers)
				{
					MouseEventArgs args = new MouseEventArgs();
					args.SetX(e.X);
					args.SetY(e.Y);
					args.SetMovementX(e.XDelta);
					args.SetMovementY(e.YDelta);
					args.SetEmulated(false);
					h.OnMouseMove(args);
				}
			}
		}

		void game_KeyPress(object sender, OpenTK.KeyPressEventArgs e)
		{
			foreach (KeyEventHandler h in keyEventHandlers)
			{
				KeyPressEventArgs args = new KeyPressEventArgs();
				args.SetKeyChar((int)e.KeyChar);
				h.OnKeyPress(args);
			}
		}

		void game_KeyDown(object sender, KeyboardKeyEventArgs e)
		{
			foreach (KeyEventHandler h in keyEventHandlers)
			{
				KeyEventArgs args = new KeyEventArgs();
				args.SetKeyCode(ToGlKey(e.Key));
				args.SetCtrlPressed(e.Modifiers == KeyModifiers.Control);
				args.SetShiftPressed(e.Modifiers == KeyModifiers.Shift);
				args.SetAltPressed(e.Modifiers == KeyModifiers.Alt);
				h.OnKeyDown(args);
			}
		}

		void game_KeyUp(object sender, KeyboardKeyEventArgs e)
		{
			foreach (KeyEventHandler h in keyEventHandlers)
			{
				KeyEventArgs args = new KeyEventArgs();
				args.SetKeyCode(ToGlKey(e.Key));
				h.OnKeyUp(args);
			}
		}

		#endregion
	}

	public class RandomNative : RandomCi
	{
		public Random rnd = new Random();
		public override float NextFloat()
		{
			return (float)rnd.NextDouble();
		}

		public override int Next()
		{
			return rnd.Next();
		}

		public override int MaxNext(int range)
		{
			return rnd.Next(range);
		}
	}

	public class MyUri
	{
		public MyUri(string uri)
		{
			//string url = "md://publichash:123/?user=a&auth=123";
			var a = new Uri(uri);
			Ip = a.Host;
			Port = a.Port;
			Get = ParseGet(uri);
		}
		internal string Url { get; private set; }
		internal string Ip { get; private set; }
		internal int Port { get; private set; }
		internal Dictionary<string, string> Get { get; private set; }
		private static Dictionary<string, string> ParseGet(string url)
		{
			try
			{
				Dictionary<string, string> d;
				d = new Dictionary<string, string>();
				if (url.Contains("?"))
				{
					string url2 = url.Substring(url.IndexOf("?") + 1);
					var ss = url2.Split(new char[] { '&' });
					for (int i = 0; i < ss.Length; i++)
					{
						var ss2 = ss[i].Split(new char[] { '=' });
						d[ss2[0]] = ss2[1];
					}
				}
				return d;
			}
			catch
			{
				//throw new FormatException("Invalid address: " + url);
				return null;
			}
		}
	}

	public class AviWriterCiCs : AviWriterCi
	{
		public AviWriterCiCs()
		{
			avi = new AviWriter();
		}

		public AviWriter avi;
		public Bitmap openbmp;

		public override void Open(string filename, int framerate, int width, int height)
		{
			openbmp = avi.Open(filename, (uint)framerate, width, height);
		}

		public override void AddFrame(BitmapCi bitmap)
		{
			var bmp_ = (BitmapCiCs)bitmap;

			using (Graphics g = Graphics.FromImage(openbmp))
			{
				g.DrawImage(bmp_.bmp, 0, 0);
			}
			openbmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

			avi.AddFrame();
		}

		public override void Close()
		{
			avi.Close();
		}
	}


	public class BitmapCiCs : BitmapCi
	{
		public Bitmap bmp;
	}
	public class GlProgramNative : GlProgram
	{
		public int id;
	}
	public class GLShaderNative : GlShader
	{
		public int id;
	}


	public class GameWindowNative : OpenTK.GameWindow
	{
		public GamePlatformNative platform;
		public GameWindowNative(OpenTK.Graphics.GraphicsMode mode)
			: base(1280, 720, mode)
		{
			VSync = OpenTK.VSyncMode.On;
			WindowState = OpenTK.WindowState.Normal;
		}
	}
}
