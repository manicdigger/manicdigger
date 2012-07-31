using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace ManicDigger
{
    public class ModLoader
    {
        public void CompileScripts(Dictionary<string, string> scripts)
        {
            foreach (var k in scripts)
            {
                CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<String, String>{{ "CompilerVersion", "v3.5" }});
                var parms = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                };
                parms.ReferencedAssemblies.Add("System.dll");
                parms.ReferencedAssemblies.Add("ScriptingApi.dll");
                parms.ReferencedAssemblies.Add("LibNoise.dll");
                CompilerResults results = compiler.CompileAssemblyFromSource(parms, new string[] { k.Value });
                if (results.Errors.Count != 0)
                {
                    try
                    {
                        string errors = "";
                        foreach (CompilerError error in results.Errors)
                        {
                            if (error.IsWarning)
                            {
                                continue;
                            }
                            errors += string.Format("{0} Line:{1} {2}",error.ErrorNumber, error.Line, error.ErrorText);
                        }
                        System.Windows.Forms.MessageBox.Show("Can't load mod: " + k.Key + "\n"+errors);
                    }
                    catch
                    {
                    }
                    continue;
                }
                foreach (Type t in results.CompiledAssembly.GetTypes())
                {
                    if (typeof(IMod).IsAssignableFrom(t))
                    {
                        mods[t.Name] = (IMod)results.CompiledAssembly.CreateInstance(t.FullName);
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
                        System.Windows.Forms.MessageBox.Show(string.Format("Can't load mod {0} because its dependency {1} couldn't be loaded.", name, required_name));
                    }
                    StartMod(required_name, mods[required_name], m);
                }
            }
            mod.Start(m);
            loaded[name] = true;
        }
    }
}
