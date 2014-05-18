using System;

namespace ManicDigger.Mods
{
    public class VegetationGrowth : IMod
    {
        public void PreStart(ModManager m)
        {
            m.RequireMod("Default");
        }
        public void Start(ModManager manager)
        {
            m = manager;
            DirtForFarming = m.GetBlockId("DirtForFarming");
            Sapling = m.GetBlockId("Sapling");
            BrownMushroom = m.GetBlockId("BrownMushroom");
            RedMushroom = m.GetBlockId("RedMushroom");
            YellowFlowerDecorations = m.GetBlockId("YellowFlowerDecorations");
            RedRoseDecorations = m.GetBlockId("RedRoseDecorations");
            Dirt = m.GetBlockId("Dirt");
            Grass = m.GetBlockId("Grass");
            Crops1 = m.GetBlockId("Crops1");
            Crops2 = m.GetBlockId("Crops2");
            Crops3 = m.GetBlockId("Crops3");
            Crops4 = m.GetBlockId("Crops4");
            Leaves = m.GetBlockId("Leaves");
            Apples = m.GetBlockId("Apples");
            TreeTrunk = m.GetBlockId("TreeTrunk");
            
            m.RegisterOnBlockUpdate(BlockTickGrowCropsCycle);
            m.RegisterOnBlockUpdate(BlockTickGrowSapling);
            m.RegisterOnBlockUpdate(BlockTickMushroomDeath);
            m.RegisterOnBlockUpdate(BlockTickFlowerDeath);
            m.RegisterOnBlockUpdate(BlockTickGrowGrassOrMushroomsOnDirt);
            m.RegisterOnBlockUpdate(BlockTickGrassDeathInDarkness);
        }
        ModManager m;
        int DirtForFarming;
        int Sapling;
        int BrownMushroom;
        int RedMushroom;
        int YellowFlowerDecorations;
        int RedRoseDecorations;
        int Dirt;
        int Grass;
        int Crops1;
        int Crops2;
        int Crops3;
        int Crops4;
        int Leaves;
        int Apples;
        int TreeTrunk;
        
        void BlockTickGrowCropsCycle(int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == DirtForFarming)
            {
                if (m.IsValidPos(x, y, z + 1))
                {
                    int blockabove = m.GetBlock(x, y, z + 1);
                    if (blockabove == Crops1) { blockabove = Crops2; }
                    else if (blockabove == Crops2) { blockabove = Crops3; }
                    else if (blockabove == Crops3) { blockabove = Crops4; }
                    else { return; }
                    m.SetBlock(x, y, z + 1, blockabove);
                }
            }
        }
        void BlockTickGrowSapling(int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == Sapling)
            {
                if (!IsShadow(x, y, z))
                {
                    if (!m.IsValidPos(x, y, z - 1))
                    {
                        return;
                    }
                    int under = m.GetBlock(x, y, z - 1);
                    if (!(under == Dirt
                        || under == Grass
                        || under == DirtForFarming))
                    {
                        return;
                    }
                    MakeAppleTree(x, y, z - 1);
                }
            }
        }
        void PlaceTree(int x, int y, int z)
        {
            Place(x, y, z + 1, TreeTrunk);
            Place(x, y, z + 2, TreeTrunk);
            Place(x, y, z + 3, TreeTrunk);
            
            Place(x + 1, y, z + 3, Leaves);
            Place(x - 1, y, z + 3, Leaves);
            Place(x, y + 1, z + 3, Leaves);
            Place(x, y - 1, z + 3, Leaves);
            
            Place(x + 1, y + 1, z + 3, Leaves);
            Place(x + 1, y - 1, z + 3, Leaves);
            Place(x - 1, y + 1, z + 3, Leaves);
            Place(x - 1, y - 1, z + 3, Leaves);
            
            Place(x + 1, y, z + 4, Leaves);
            Place(x - 1, y, z + 4, Leaves);
            Place(x, y + 1, z + 4, Leaves);
            Place(x, y - 1, z + 4, Leaves);
            
            Place(x, y, z + 4, Leaves);
        }
        void Place(int x, int y, int z, int blocktype)
        {
            if (m.IsValidPos(x, y, z))
            {
                m.SetBlock(x, y, z, blocktype);
            }
        }
        void BlockTickGrowGrassOrMushroomsOnDirt(int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == Dirt)
            {
                if (m.IsValidPos(x, y, z + 1))
                {
                    int roofBlock = m.GetBlock(x, y, z + 1);
                    if (m.IsTransparentForLight(roofBlock))
                    {
                        if (IsShadow(x, y, z) && !reflectedSunnyLight(x, y, z))
                        {
                            // if 1% chance happens then 1 mushroom will grow up );
                            if (rnd.NextDouble() < 0.01)
                            {
                                int tile = rnd.NextDouble() < 0.6 ? RedMushroom : BrownMushroom;
                                m.SetBlock(x, y, z + 1, tile);
                            }
                        }
                        else
                        {
                            m.SetBlock(x, y, z, Grass);
                        }
                    }
                }
            }
        }
        void BlockTickGrassDeathInDarkness(int x, int y, int z)
        {
            if (m.GetBlock(x, y, z) == Grass)
            {
                if (IsShadow(x, y, z)
                    && !(reflectedSunnyLight(x, y, z) && m.IsTransparentForLight(m.GetBlock(x, y, z + 1))))
                {
                    m.SetBlock(x, y, z, Dirt);
                }
            }
        }
        void MakeAppleTree(int cx, int cy, int cz)
        {
            int x = cx;
            int y = cy;
            int z = cz;
            int TileIdLeaves = Leaves;
            int TileIdApples = Apples;
            int TileIdTreeTrunk = TreeTrunk;
            int treeHeight = rnd.Next(4, 6);
            int xx = 0;
            int yy = 0;
            int dir = 0;
            
            for (int i = 0; i < treeHeight; i++)
            {
                Place(x, y, z + i, TileIdTreeTrunk);
                if (i == treeHeight - 1)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        dir += 45;
                        for (int k = 1; k < 2; k++)
                        {
                            int length = dir % 90 == 0 ? k : (int)(k / 2);
                            xx = length * (int)Math.Round(Math.Cos(dir * Math.PI / 180));
                            yy = length * (int)Math.Round(Math.Sin(dir * Math.PI / 180));
                            
                            Place(x + xx, y + yy, z + i, TileIdTreeTrunk);
                            float appleChance = 0.45f;
                            int tile;
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy, z + i + 1, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx + 1, y + yy, z + i, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx - 1, y + yy, z + i, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy + 1, z + i, tile);
                            tile = rnd.NextDouble() < appleChance ? TileIdApples : TileIdLeaves; PlaceIfEmpty(x + xx, y + yy - 1, z + i, tile);
                        }
                    }
                }
            }
        }
        void PlaceIfEmpty(int x, int y, int z, int blocktype)
        {
            if (m.IsValidPos(x, y, z) && m.GetBlock(x, y, z) == 0)
            {
                m.SetBlock(x, y, z, blocktype);
            }
        }
        Random rnd = new Random();
        // mushrooms will die when they have not shadow or dirt, or 20% chance happens
        void BlockTickMushroomDeath(int x, int y, int z)
        {
            int block = m.GetBlock(x, y, z);
            if (block == BrownMushroom || block == RedMushroom)
            {
                if (rnd.NextDouble() < 0.2) { m.SetBlock(x, y, z, 0); return; }
                if (!IsShadow(x, y, z - 1))
                {
                    m.SetBlock(x, y, z, 0);
                }
                else
                {
                    if (m.GetBlock(x, y, z - 1) == Dirt) return;
                    m.SetBlock(x, y, z, 0);
                }
            }
        }
        // floowers will die when they have not light, dirt or grass , or 2% chance happens
        void BlockTickFlowerDeath(int x, int y, int z)
        {
            int block = m.GetBlock(x, y, z);
            if (block == YellowFlowerDecorations || block == RedRoseDecorations)
            {
                if (rnd.NextDouble() < 0.02) { m.SetBlock(x, y, z, 0); return; }
                if (IsShadow(x, y, z - 1))
                {
                    m.SetBlock(x, y, z, 0);
                }
                else
                {
                    int under = m.GetBlock(x, y, z - 1);
                    if ((under == Dirt
                        || under == Grass)) return;
                    m.SetBlock(x, y, z, 0);
                }
            }
        }
        bool IsShadow(int x, int y, int z)
        {
            for (int i = 1; i < 10; i++)
            {
                if (m.IsValidPos(x, y, z + i) && !m.IsTransparentForLight(m.GetBlock(x, y, z + i)))
                {
                    return true;
                }
            }
            return false;
        }
        // The true if on a cube gets the sunlight reflected from another cubes
        bool reflectedSunnyLight(int x, int y, int z)
        {
            for (int i = x - 2; i <= x + 2; i++)
                for (int j = y - 2; j <= y + 2; j++)
                {
                    if (!IsShadow(i, j, z))
                    {
                        return true;
                    }
                }
            return false;
        }
    }
}
