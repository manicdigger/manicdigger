using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;

namespace ManicDigger
{
    public class ModLoader
    {
        public ModLoader()
        {
            jintEngine.DisableSecurity();
            jintEngine.AllowClr = true;
        }
        Jint.JintEngine jintEngine = new Jint.JintEngine();
        Dictionary<string, string> javascriptScripts = new Dictionary<string, string>();
        public void CompileScripts(Dictionary<string, string> scripts, bool restart)
        {
            foreach (var k in scripts)
            {
                if (k.Key.EndsWith(".js"))
                {
                    javascriptScripts[k.Key] = k.Value;
                    continue;
                }
                if (restart)
                {
                    continue;
                }
                CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<String, String>{{ "CompilerVersion", "v3.5" }});
                var parms = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                    CompilerOptions = "/unsafe"
                };
                parms.ReferencedAssemblies.Add("System.dll");
                parms.ReferencedAssemblies.Add("System.Drawing.dll");
                parms.ReferencedAssemblies.Add("ScriptingApi.dll");
                parms.ReferencedAssemblies.Add("LibNoise.dll");
                parms.ReferencedAssemblies.Add("protobuf-net.dll");
                parms.ReferencedAssemblies.Add("System.Xml.dll");
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
                            errors += string.Format("{0} Line:{1} {2}",error.ErrorNumber, error.Line, error.ErrorText);
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
                foreach (Type t in results.CompiledAssembly.GetTypes())
                {
                    if (typeof(IMod).IsAssignableFrom(t))
                    {
                        mods[t.Name] = (IMod)results.CompiledAssembly.CreateInstance(t.FullName);
                        Console.WriteLine("Loaded mod: {0}", t.Name);
                    }
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
