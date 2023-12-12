using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ManicDigger.Common
{
    public class ModManagerUtil
    {
        public ModManagerUtil()
        {
        }
        public static List<ModInfo> GetAllMods()
        {
            List<ModInfo> modinfos = new List<ModInfo>();
            string modpack = "";
            string[] modpaths = new[] { Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.Combine("..", ".."), ".."), "ManicDigger.Common"), "Server"), "Mods"), "Mods" };

            for (int i = 0; i < modpaths.Length; i++)
            {
                if (File.Exists(Path.Combine(modpaths[i], "current.txt")))
                {
                    modpack = File.ReadAllText(Path.Combine(modpaths[i], "current.txt")).Trim();
                }
                else if (Directory.Exists(modpaths[i]))
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(modpaths[i], "current.txt"), modpack);
                    }
                    catch
                    {

                    }
                }
                modpaths[i] = Path.Combine(modpaths[i], modpack);
            }

            foreach (string modpath in modpaths)
            {
                if (!Directory.Exists(modpath))
                {
                    continue;
                }
                string[] directories = Directory.GetDirectories(modpath);

                foreach (string d in directories)
                {
                    if (File.Exists(Path.Combine(d, "Modinfo.json")))
                    {
                        modinfos.Add(JsonConvert.DeserializeObject<ModInfo>(File.ReadAllText(Path.Combine(d, "Modinfo.json"))));
                    }
                }
            }

            return modinfos;
        }
    }
}
