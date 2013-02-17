using System;
using System.Collections.Generic;
using System.Text;

namespace ManicDigger.Mods
{
    public class Doors : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }
        public void Start(ModManager m)
        {
            this.m = m;

            m.RegisterOnBlockBuild(OnBuild);
            m.RegisterOnBlockDelete(OnDelete);
            m.RegisterOnBlockUse(OnUse);

            m.SetString("en", "DoorBottomClosed", "Closed Door");
            m.SetString("en", "DoorTopClosed", "Closed Door");
            m.SetString("en", "DoorBottomOpen", "Open Door");
            m.SetString("en", "DoorTopOpen", "Open Door");

            SoundSet sounds = new SoundSet()
            {
                Walk = new string[] { "walk1", "walk2", "walk3", "walk4" },
                Break = new string[] { "destruct" },
                Build = new string[] { "build" },
                Clone = new string[] { "clone" },
            };

            m.SetBlockType(126, "DoorBottomClosed", new BlockType()
            {
                AllTextures = "DoorBottom",
                DrawType = DrawType.ClosedDoor,
                WalkableType = WalkableType.Solid,
                Sounds = sounds,
                IsUsable = true,
            });
            m.SetBlockType(127, "DoorTopClosed", new BlockType()
            {
                AllTextures = "DoorTop",
                DrawType = DrawType.ClosedDoor,
                WalkableType = WalkableType.Solid,
                Sounds = sounds,
                IsUsable = true,
            });
            m.SetBlockType(128, "DoorBottomOpen", new BlockType()
            {
                AllTextures = "DoorBottom",
                DrawType = DrawType.OpenDoor,
                WalkableType = WalkableType.Empty,
                Sounds = sounds,
                IsUsable = true,
            });
            m.SetBlockType(129, "DoorTopOpen", new BlockType()
            {
                AllTextures = "DoorTop",
                DrawType = DrawType.OpenDoor,
                WalkableType = WalkableType.Empty,
                Sounds = sounds,
                IsUsable = true,
            });

            m.AddToCreativeInventory("DoorBottomClosed");
            m.AddCraftingRecipe("DoorBottomClosed", 1, "Wood", 2);

            DoorBottomClosed = m.GetBlockId("DoorBottomClosed");
            DoorTopClosed = m.GetBlockId("DoorTopClosed");
            DoorBottomOpen = m.GetBlockId("DoorBottomOpen");
            DoorTopOpen = m.GetBlockId("DoorTopOpen");
        }

        ModManager m;
        int DoorBottomClosed;
        int DoorTopClosed;
        int DoorBottomOpen;
        int DoorTopOpen;

        void OnBuild(int player, int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == DoorBottomClosed)
            {
                if (m.IsValidPos(x, y, z + 1) && m.GetBlock(x, y, z + 1) == 0)
                {
                    m.SetBlock(x, y, z + 1, DoorTopClosed);
                }
                else if (m.IsValidPos(x, y, z - 1) && m.GetBlock(x, y, z - 1) == 0)
                {
                    m.SetBlock(x, y, z, DoorTopClosed);
                    m.SetBlock(x, y, z - 1, DoorBottomClosed);
                }
                else
                {
                    m.SetBlock(x, y, z, 0);
                }
            }
        }

        void OnDelete(int player, int x, int y, int z, int block)
        {
            if (m.IsValidPos(x, y, z + 1)
                && (m.GetBlock(x, y, z + 1) == DoorTopClosed || m.GetBlock(x, y, z + 1) == DoorTopOpen))
            {
                m.SetBlock(x, y, z + 1, 0);
            }
            if (m.IsValidPos(x, y, z - 1)
                && (m.GetBlock(x, y, z - 1) == DoorBottomOpen || m.GetBlock(x, y, z - 1) == DoorBottomClosed))
            {
                m.SetBlock(x, y, z - 1, 0);
            }
        }

        void OnUse(int player, int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == DoorBottomClosed)
            {
                m.SetBlock(x, y, z, DoorBottomOpen);
                m.SetBlock(x, y, z + 1, DoorTopOpen);
            }
            else if (m.GetBlock(x, y, z) == DoorBottomOpen)
            {
                m.SetBlock(x, y, z, DoorBottomClosed);
                m.SetBlock(x, y, z + 1, DoorTopClosed);
            }
            else if (m.GetBlock(x, y, z) == DoorTopClosed)
            {
                m.SetBlock(x, y, z, DoorTopOpen);
                m.SetBlock(x, y, z - 1, DoorBottomOpen);
            }
            else if (m.GetBlock(x, y, z) == DoorTopOpen)
            {
                m.SetBlock(x, y, z, DoorTopClosed);
                m.SetBlock(x, y, z - 1, DoorBottomClosed);
            }
        }
    }
}
