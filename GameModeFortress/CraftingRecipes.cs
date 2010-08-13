using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace GameModeFortress
{
    public class CraftingRecipes
    {
        public CraftingRecipes()
        {
            MakeRecipes();
        }
        public List<CraftingRecipe> craftingrecipes = new List<CraftingRecipe>();
        void MakeRecipes()
        {
            craftingrecipes = new List<CraftingRecipe>();
            MakeRecipe(TileTypeMinecraft.Stone, 2, TileTypeMinecraft.Cobblestone, 1);
            MakeRecipe(TileTypeMinecraft.Cobblestone, 2, TileTypeMinecraft.Stone, 1);
            MakeRecipe(TileTypeMinecraft.TreeTrunk, 1, TileTypeMinecraft.Wood, 2);
            MakeRecipe(TileTypeMinecraft.Stone, 4, TileTypeMinecraft.Brick, 1);
            MakeRecipe(TileTypeMinecraft.GoldOre, 1, TileTypeMinecraft.CoalOre, 1, TileTypeMinecraft.GoldBlock, 1);
            MakeRecipe(TileTypeMinecraft.IronOre, 1, TileTypeMinecraft.CoalOre, 1, TileTypeMinecraft.IronBlock, 1);
            MakeRecipe(TileTypeMinecraft.Wood, 1, TileTypeMinecraft.IronBlock, 1, GameDataTilesManicDigger.railstart + (int)RailDirectionFlags.Corners, 2);
            MakeRecipe(TileTypeMinecraft.Wood, 3, TileTypeManicDigger.CraftingTable, 1);
            MakeRecipe(TileTypeMinecraft.Stone, 2, TileTypeMinecraft.Stair, 1);
            MakeRecipe(TileTypeMinecraft.Stone, 2, TileTypeMinecraft.DoubleStair, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.TNT, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.Adminium, 1);
            MakeRecipe(TileTypeMinecraft.Sand, 2, TileTypeMinecraft.Glass, 1);
            MakeRecipe(TileTypeMinecraft.Leaves, 10, TileTypeMinecraft.RedRoseDecorations, 1);
            MakeRecipe(TileTypeMinecraft.Leaves, 10, TileTypeMinecraft.YellowFlowerDecorations, 1);
            MakeRecipe(TileTypeMinecraft.Leaves, 3, TileTypeMinecraft.Sapling, 1);
            MakeRecipe(TileTypeMinecraft.Dirt, 10, TileTypeMinecraft.RedMushroom, 1);
            MakeRecipe(TileTypeMinecraft.Dirt, 10, TileTypeMinecraft.BrownMushroom, 1);
            MakeRecipe(TileTypeMinecraft.Grass, 10, TileTypeMinecraft.RedMushroom, 1);
            MakeRecipe(TileTypeMinecraft.Grass, 10, TileTypeMinecraft.BrownMushroom, 1);
            MakeRecipe(TileTypeMinecraft.Wood, 2, TileTypeMinecraft.Bookcase, 1);
            MakeRecipe(TileTypeMinecraft.Cobblestone, 1, TileTypeMinecraft.MossyCobblestone, 1);
            MakeRecipe(TileTypeMinecraft.MossyCobblestone, 1, TileTypeMinecraft.Cobblestone, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.Sponge, 1);

            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeMinecraft.RedCloth, 1);
            MakeRecipe(TileTypeMinecraft.RedCloth, 1, TileTypeMinecraft.OrangeCloth, 1);
            MakeRecipe(TileTypeMinecraft.OrangeCloth, 1, TileTypeMinecraft.YellowCloth, 1);
            MakeRecipe(TileTypeMinecraft.YellowCloth, 1, TileTypeMinecraft.LightGreenCloth, 1);
            MakeRecipe(TileTypeMinecraft.LightGreenCloth, 1, TileTypeMinecraft.GreenCloth, 1);
            MakeRecipe(TileTypeMinecraft.GreenCloth, 1, TileTypeMinecraft.AquaGreenCloth, 1);
            MakeRecipe(TileTypeMinecraft.AquaGreenCloth, 1, TileTypeMinecraft.CyanCloth, 1);
            MakeRecipe(TileTypeMinecraft.CyanCloth, 1, TileTypeMinecraft.BlueCloth, 1);
            MakeRecipe(TileTypeMinecraft.BlueCloth, 1, TileTypeMinecraft.PurpleCloth, 1);
            MakeRecipe(TileTypeMinecraft.PurpleCloth, 1, TileTypeMinecraft.IndigoCloth, 1);
            MakeRecipe(TileTypeMinecraft.IndigoCloth, 1, TileTypeMinecraft.VioletCloth, 1);
            MakeRecipe(TileTypeMinecraft.VioletCloth, 1, TileTypeMinecraft.MagentaCloth, 1);
            MakeRecipe(TileTypeMinecraft.MagentaCloth, 1, TileTypeMinecraft.PinkCloth, 1);
            MakeRecipe(TileTypeMinecraft.PinkCloth, 1, TileTypeMinecraft.BlackCloth, 1);
            MakeRecipe(TileTypeMinecraft.BlackCloth, 1, TileTypeMinecraft.GrayCloth, 1);
            MakeRecipe(TileTypeMinecraft.GrayCloth, 1, TileTypeMinecraft.WhiteCloth, 1);
            MakeRecipe(TileTypeMinecraft.WhiteCloth, 1, TileTypeMinecraft.RedCloth, 1);

            MakeRecipe(TileTypeMinecraft.Brick, 2, TileTypeManicDigger.Roof, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeManicDigger.ChemicalGreen, 1);
            MakeRecipe(TileTypeMinecraft.GoldBlock, 1, TileTypeManicDigger.Camouflage, 1);
            MakeRecipe(TileTypeMinecraft.Dirt, 2, TileTypeManicDigger.DirtForFarming, 1);
            MakeRecipe(TileTypeMinecraft.Grass, 2, TileTypeManicDigger.DirtForFarming, 1);
            MakeRecipe(TileTypeManicDigger.Crops4, 1, TileTypeManicDigger.Crops1, 2);
            MakeRecipe(TileTypeMinecraft.IronBlock, 1, TileTypeMinecraft.CoalOre, 1, TileTypeManicDigger.BrushedMetal, 1);
            MakeRecipe(TileTypeManicDigger.BrushedMetal, 5, TileTypeManicDigger.Minecart, 1);
            MakeRecipe(TileTypeManicDigger.BrushedMetal, 1, TileTypeMinecraft.Wood, 1, TileTypeManicDigger.Trampoline, 1);
            MakeRecipe(TileTypeMinecraft.Wood, 1, TileTypeMinecraft.CoalOre, 1, TileTypeMinecraft.Torch, 1);
        }
        void MakeRecipe(params object[] r)
        {
            var recipe = new CraftingRecipe();
            for (int i = 0; i < r.Length - 2; i += 2)
            {
                recipe.ingredients.Add(new Ingredient() { Type = Convert.ToInt32(r[i]), Amount = Convert.ToInt32(r[i + 1]) });
            }
            recipe.output = new Ingredient() { Type = Convert.ToInt32(r[r.Length - 2]), Amount = Convert.ToInt32(r[r.Length - 1]) };
            craftingrecipes.Add(recipe);
        }
    }
}
