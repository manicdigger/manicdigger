using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Utilities
{
    public class CrashReporter
    {
        // Where we are saving this crash report
        public static string _filepath; //GameStorePath.GetStorePath();
        public static string _filename;

        // The function we are going to wrap
        public delegate void Main(string[] args);

        // Constructor
        public CrashReporter(string filepath, string filename)
        {
            _filepath = filepath;
            _filename = filename;
        }

        // This function wraps the passed function with our reporter
        public void Start(Main main, string[] args)
        {
            // If we are not debugging in VS, go ahead and wrap the function
            if (!Debugger.IsAttached)
            {
                try
                {
                    main(args);
                }
                catch (Exception e)
                {
                    Crash(e);
                }
            }
            // We are debugging in VS, let the debugger handle the crash
            else
            {
                main(args);
            }
        }
        
        // This function writes our crash report to disk
        public static void Crash(Exception e)
        {
            // Create the full path to file
            string crashfile = Path.Combine(_filepath, _filename);

            // Write the file to disk
            File.WriteAllText(crashfile, e.ToString());

            // Show the user that a booboo has ocurred
            System.Windows.Forms.MessageBox.Show(e.ToString());

            // We have nothing left to do, as our application crashed...
            Environment.Exit(1);
        }
    }

}
