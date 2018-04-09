using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ManicDigger.Common
{
	public delegate void Action();

	/// <summary>
	/// Description of CrashReporter.
	/// </summary>
	public class CrashReporter
	{
		private static string gamepathcrash = GameStorePath.GetStorePath();
		private static string s_strDefaultFileName = "ManicDiggerCrash.txt";

		private string m_strFileName = "";

		public Action OnCrash;

		/// <summary>
		/// If set to true, the crash will be written to the console.
		/// If set to false, the crash will be displayed in a MessageBox.
		/// </summary>
		private static bool s_blnIsConsole = false;

		/// <summary>
		/// Default filename for crash reports
		/// </summary>
		public static string DefaultFileName
		{
			get { return s_strDefaultFileName; }
			set { s_strDefaultFileName = value; }
		}

		/// <summary>
		/// Writes crashreport to the file specified in DefaultFileName
		/// </summary>
		public CrashReporter()
			: this(s_strDefaultFileName)
		{
		}

		/// <summary>
		/// Writes crashreport to the given filename
		/// </summary>
		/// <param name="strFileName">Filename for the CrashReport</param>
		public CrashReporter(string strFileName)
		{
			m_strFileName = strFileName;
		}

		/// <summary>
		/// Enable global Exception handlin
		/// </summary>
		/// <param name="blnIsConsole">If set to true, the crash will be written to the console. If set to false, the crash will be displayed in a MessageBox.</param>
		public static void EnableGlobalExceptionHandling(bool blnIsConsole)
		{
			s_blnIsConsole = blnIsConsole;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		}

		/// <summary>
		/// Called after a unhandled exception occured
		/// </summary>
		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (s_blnIsConsole)
			{
				//Critical!
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Out.WriteLine("Unhandled Exception occurred");
			}

			Exception ex = e.ExceptionObject as Exception;

			//Create CrashReport for exception
			CrashReporter reporter = new CrashReporter();
			reporter.Crash(ex);
		}

		/// <summary>
		/// Creates a CrashReport if the Action failed
		/// </summary>
		/// <param name="start">Action to execute</param>
		public void Start(ThreadStart start)
		{
			if (!Debugger.IsAttached)
			{
				try
				{
					start();
				}
				catch (Exception e)
				{
					Crash(e);
				}
			}
			else
			{
				start();
			}
		}


		/// <summary>
		/// Log the exception and exit the application
		/// </summary>
		public void Crash(Exception exCrash)
		{
			StringBuilder strGuiMessage = new StringBuilder();

			strGuiMessage.AppendLine(DateTime.Now.ToString() + "> Critical Error: " + exCrash.Message);

			//Write to Crash.txt
			try
			{
				if (!Directory.Exists(gamepathcrash))
				{
					Directory.CreateDirectory(gamepathcrash);
				}

				string crashfile = Path.Combine(gamepathcrash, m_strFileName);

				//Open/Create Crash file
				using (FileStream fs = File.Open(crashfile, FileMode.Append))
				{
					using (StreamWriter logger = new StreamWriter(fs))
					{
						Log(DateTime.Now.ToString() + ": Critical error occurred", logger);

						//Call OnCrash logic
						CallOnCrash(logger);

						Exception exToLog = exCrash;

						//Log the exception and its inner exceptions
						while (exToLog != null)
						{
							Log(exToLog.ToString(), logger);
							Log("-------------------------------", logger);
							exToLog = exToLog.InnerException;
						}
					}
				}

				//Output Crashreport Location
				strGuiMessage.AppendLine("Crash report created: \"" + crashfile + "\"");
			}
			catch (Exception ex)
			{
				strGuiMessage.AppendLine("Crashreport failed: " + ex.ToString());
			}
			finally
			{
				if (s_blnIsConsole)
				{
					//Give user time to read output
					Console.WriteLine("Ending after critical error!");
					Console.WriteLine("Press any key to shut down...");
					Console.ReadLine();
				}
				else
				{
					DisplayInGui(strGuiMessage.ToString());
				}

				//Shutdown
				Environment.Exit(1);
			}
		}

		/// <summary>
		/// Tries to execute the OnCrash delegate
		/// </summary>
		private void CallOnCrash(TextWriter logger)
		{
			if (OnCrash != null)
			{
				try
				{
					OnCrash();
				}
				catch (Exception ex)
				{
					logger.WriteLine("OnCrash() failed: " + ex.ToString());
				}
			}
		}

		/// <summary>
		/// Write to logger and Console
		/// </summary>
		private void Log(string str, TextWriter logger)
		{
			logger.WriteLine(str);
			Console.WriteLine(str);
		}

		/// <summary>
		/// Displays a text in the gui
		/// </summary>
		private void DisplayInGui(string strTxt)
		{
			try
			{
				//Display error
				for (int i = 0; i < 5; i++)
				{
					Cursor.Show();
					Thread.Sleep(100);
					Application.DoEvents();
				}

				MessageBox.Show(strTxt, "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch
			{
				//If this fails, something is really screwed... let's hope the crash report was created
				//Just swallow this exception, to prevent a exception endless loop (UnhandledException -> CrashReport -> UnhandledException)
			}
		}

	}
}
