using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using ManicDigger.ClientNative;

namespace ManicDigger
{
    public class ServerSystemModLoader : ServerSystem
    {
        public ServerSystemModLoader()
        {
            jintEngine.DisableSecurity();
            jintEngine.AllowClr = true;
        }

        bool started;
        public override void Update(Server server, float dt)
        {
            if (!started)
            {
                started = true;
                LoadMods(server, false);
            }
        }

        public override bool OnCommand(Server server, int sourceClientId, string command, string argument)
        {
            if (command == "mods")
            {
                RestartMods(server, sourceClientId);
                return true;
            }
            return false;
        }

        public bool RestartMods(Server server, int sourceClientId)
        {
            if (!server.PlayerHasPrivilege(sourceClientId, ServerClientMisc.Privilege.restart))
            {
                server.SendMessage(sourceClientId, string.Format(server.language.Get("Server_CommandInsufficientPrivileges"), server.colorError));
                return false;
            }
            server.SendMessageToAll(string.Format(server.language.Get("Server_CommandRestartModsSuccess"), server.colorImportant, server.GetClient(sourceClientId).ColoredPlayername(server.colorImportant)));
            server.ServerEventLog(string.Format("{0} restarts mods.", server.GetClient(sourceClientId).playername));

            server.modEventHandlers = new ModEventHandlers();
            for (int i = 0; i < server.systemsCount; i++)
            {
                if (server.systems[i] == null) { continue; }
                server.systems[i].OnRestart(server);
            }

            LoadMods(server, true);
            return true;
        }

        void LoadMods(Server server, bool restart)
        {
            server.modManager = new ModManager1();
            var m = server.modManager;
            m.Start(server);
            var scritps = GetScriptSources(server);
            CompileScripts(scritps, restart);
            Start(m, m.required);
        }

        Dictionary<string, string> GetScriptSources(Server server)
        {
            string[] modpaths = new[] { Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "ManicDiggerLib"), "Server"), "Mods"), "Mods" };

            for (int i = 0; i < modpaths.Length; i++)
            {
                if (File.Exists(Path.Combine(modpaths[i], "current.txt")))
                {
                    server.gameMode = File.ReadAllText(Path.Combine(modpaths[i], "current.txt")).Trim();
                }
                else if (Directory.Exists(modpaths[i]))
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(modpaths[i], "current.txt"), server.gameMode);
                    }
                    catch
                    {
                    }
                }
                modpaths[i] = Path.Combine(modpaths[i], server.gameMode);
            }
            Dictionary<string, string> scripts = new Dictionary<string, string>();
            foreach (string modpath in modpaths)
            {
                if (!Directory.Exists(modpath))
                {
                    continue;
                }
                server.ModPaths.Add(modpath);
                string[] files = Directory.GetFiles(modpath);
                foreach (string s in files)
                {
                    if (!GameStorePath.IsValidName(Path.GetFileNameWithoutExtension(s)))
                    {
                        continue;
                    }
                    if (!(Path.GetExtension(s).Equals(".cs", StringComparison.InvariantCultureIgnoreCase)
                        || Path.GetExtension(s).Equals(".js", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }
                    string scripttext = File.ReadAllText(s);
                    string filename = new FileInfo(s).Name;
                    scripts[filename] = scripttext;
                }
            }
            return scripts;
        }

        Jint.JintEngine jintEngine = new Jint.JintEngine();
        Dictionary<string, string> javascriptScripts = new Dictionary<string, string>();
        public void CompileScripts(Dictionary<string, string> scripts, bool restart)
        {
            CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<String, String> { { "CompilerVersion", "v3.5" } });
            var parms = new CompilerParameters();
            parms.GenerateExecutable = false;
            parms.CompilerOptions = "/unsafe";

#if !DEBUG
            parms.GenerateInMemory = true;
#else
            //Prepare for mod debugging
            //IMPORTANT: Visual Studio breakpoints will not jump into a generatet .cs file
            //Instead, call "System.Diagnostics.Debugger.Break()" to create a breakpoint in the mod-class

            //Generate files to debug
            parms.GenerateInMemory = false;
            parms.IncludeDebugInformation = true;

            //Use a local temp folder
            DirectoryInfo dirTemp = new DirectoryInfo(Path.Combine(new FileInfo(GetType().Assembly.Location).DirectoryName, "ModDebugInfos"));

            //Prepare temp directory
            if (!dirTemp.Exists)
            {
                Directory.CreateDirectory(dirTemp.FullName);
            }
            else
            {
                try
                {
                    //Clear temp files
                    foreach (FileInfo f in dirTemp.GetFiles())
                    {
                        f.Delete();
                    }
                }
                catch (Exception ex)
                {
                    //meh, maybe next time
                }
            }

            //created locally, this allows the debugger to find the .pdb
            parms.OutputAssembly = Path.Combine(new DirectoryInfo(new FileInfo(GetType().Assembly.Location).DirectoryName).FullName ,"Mods.dll");

            //generatet .cs files are stored here
            //they are rather important for this debug session, since the .pdb link to them
            parms.TempFiles = new TempFileCollection(dirTemp.FullName, true);
#endif

            parms.ReferencedAssemblies.Add("System.dll");
            parms.ReferencedAssemblies.Add("System.Drawing.dll");
            parms.ReferencedAssemblies.Add("ScriptingApi.dll");
            parms.ReferencedAssemblies.Add("LibNoise.dll");
            parms.ReferencedAssemblies.Add("protobuf-net.dll");
            parms.ReferencedAssemblies.Add("System.Xml.dll");

            Dictionary<string, string> csharpScripts = new Dictionary<string, string>();
            foreach (var k in scripts)
            {
                if (k.Key.EndsWith(".js"))
                {
                    javascriptScripts[k.Key] = k.Value;
                }
                else
                {
                    csharpScripts[k.Key] = k.Value;
                }
            }
            if (restart)
            {
                // javascript only
                return;
            }

            string[] csharpScriptsValues = new string[csharpScripts.Values.Count];
            int i = 0;
            foreach (var k in csharpScripts)
            {
                csharpScriptsValues[i++] = k.Value;
            }

            {
                CompilerResults results = compiler.CompileAssemblyFromSource(parms, csharpScriptsValues);

                if (results.Errors.Count == 0)
                {
                    Use(results);
                    return;
                }
            }

            //Error. Load scripts separately.

            foreach (var k in csharpScripts)
            {
                CompilerResults results = compiler.CompileAssemblyFromSource(parms, new string[] { k.Value });
                if (results.Errors.Count != 0)
                {
                    try
                    {
                        string errors = "";
                        foreach (CompilerError error in results.Errors)
                        {
                            //mono is treating warnings as errors.
                            //if (error.IsWarning)
                            {
                                //continue;
                            }
                            errors += string.Format("{0} Line:{1} {2}", error.ErrorNumber, error.Line, error.ErrorText);
                        }
                        string errormsg = "Can't load mod: " + k.Key + "\n" + errors;
                        try
                        {
                            System.Windows.Forms.MessageBox.Show(errormsg);
                        }
                        catch
                        {
                        }
                        Console.WriteLine(errormsg);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    continue;
                }
                Use(results);
            }
        }

        void Use(CompilerResults results)
        {
            foreach (Type t in results.CompiledAssembly.GetTypes())
            {
                if (typeof(IMod).IsAssignableFrom(t))
                {
                    mods[t.Name] = (IMod)results.CompiledAssembly.CreateInstance(t.FullName);
                    Console.WriteLine("Loaded mod: {0}", t.Name);
                }
            }
        }

        Dictionary<string, IMod> mods = new Dictionary<string, IMod>();
        Dictionary<string, string[]> modRequirements = new Dictionary<string, string[]>();
        Dictionary<string, bool> loaded = new Dictionary<string, bool>();

        public void Start(ModManager m, List<string> currentRequires)
        {
            /*
            foreach (var mod in mods)
            {
                mod.Start(m);
            }
            */

            modRequirements.Clear();
            loaded.Clear();

            foreach (var k in mods)
            {
                k.Value.PreStart(m);
                modRequirements[k.Key] = currentRequires.ToArray();
                currentRequires.Clear();
            }
            foreach (var k in mods)
            {
                StartMod(k.Key, k.Value, m);
            }

            StartJsMods(m);
        }

        void StartJsMods(ModManager m)
        {
            jintEngine.SetParameter("m", m);
            // todo: javascript mod requirements
            foreach (var k in javascriptScripts)
            {
                try
                {
                    jintEngine.Run(k.Value);
                    Console.WriteLine("Loaded mod: {0}", k.Key);
                }
                catch
                {
                    Console.WriteLine("Error in mod: {0}", k.Key);
                }
            }
        }

        void StartMod(string name, IMod mod, ModManager m)
        {
            if (loaded.ContainsKey(name))
            {
                return;
            }
            if (modRequirements.ContainsKey(name))
            {
                foreach (string required_name in modRequirements[name])
                {
                    if (!mods.ContainsKey(required_name))
                    {
                        try
                        {
                            System.Windows.Forms.MessageBox.Show(string.Format("Can't load mod {0} because its dependency {1} couldn't be loaded.", name, required_name));
                        }
                        catch
                        {
                            //This will be the case if the server is running on a headless linux server without X11 installed (previously crashed)
                            Console.WriteLine(string.Format("[Mod error] Can't load mod {0} because its dependency {1} couldn't be loaded.", name, required_name));
                        }
                    }
                    StartMod(required_name, mods[required_name], m);
                }
            }
            mod.Start(m);
            loaded[name] = true;
        }
    }
}
