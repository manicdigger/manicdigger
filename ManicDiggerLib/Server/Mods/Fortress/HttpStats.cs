using System;
using FragLabs.HTTP;
using System.Diagnostics;

namespace ManicDigger.Mods.Fortress
{
	public class HttpStats : IMod
	{
		public void PreStart(ModManager m) { }
		
		public void Start(ModManager m)
		{
			var module = new HttpInfoModule();
			module.m = m;
			m.InstallHttpModule("Stats", () => "Basic server stats", module);
		}
	}

	public class HttpInfoModule : IHttpModule
	{
		public void Installed(HttpServer server)
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
		}

		Stopwatch stopwatch;
		public ModManager m;
		int pageViews;

		public void Uninstalled(HttpServer server)
		{
		}

		public bool ResponsibleForRequest(HttpRequest request)
		{
			if (request.Uri.AbsolutePath.ToLower() == "/stats")
				return true;
			return false;
		}

		public bool ProcessAsync(ProcessRequestEventArgs args)
		{
			pageViews++;
			double cpu = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds / stopwatch.Elapsed.TotalSeconds;
			string html = "<html>";
			html += "<h1>System Statistics</h1>";
			html += "<ul>";
			html += string.Format("<li>Uptime: {0} <br/></li>", ToReadableString(stopwatch.Elapsed));
			html += string.Format("<li>CPU usage: {0:P2} <br/></li>", cpu);
			html += string.Format("<li>Total processor time: {0}<br/></li>", ToReadableString(Process.GetCurrentProcess().TotalProcessorTime));
			html += string.Format("<li>Working set: {0}<br/></li>", BytesToString(Process.GetCurrentProcess().WorkingSet64));
			html += string.Format("<li>Total bytes downloaded: {0}<br/></li>", BytesToString(m.TotalReceivedBytes()));
			html += string.Format("<li>Total bytes uploaded: {0}<br/></li>", BytesToString(m.TotalSentBytes()));
			html += "</ul>";
			html += string.Format("Page accessed <b>{0}</b> times.<br/>", pageViews);
			html += "</html>";
			args.Response.Producer = new BufferedProducer(html);
			return false;
		}

		//http://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
		static String BytesToString(long byteCount)
		{
			string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
			if (byteCount == 0)
				return "0 " + suf[0];
			long bytes = Math.Abs(byteCount);
			int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			double num = Math.Round(bytes / Math.Pow(1024, place), 1);
			return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
		}

		public static string ToReadableString(TimeSpan span)
		{
			string formatted = string.Format("{0}{1}{2}{3}",
			                                 span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
			                                 span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
			                                 span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
			                                 span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);
			
			if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

			if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

			return formatted;
		}
	}
}
