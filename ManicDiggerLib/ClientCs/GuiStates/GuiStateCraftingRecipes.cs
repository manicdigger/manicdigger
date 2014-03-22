using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ProtoBuf;

namespace ManicDigger
{
    public partial class ManicDiggerGameWindow
    {
        #region IViewport3d Members
        public void CraftingRecipesStart(Packet_CraftingRecipe[] recipes, List<int> blocks, Action<IntRef> craftingRecipeSelected)
        {
            this.craftingrecipes2 = recipes;
            this.craftingblocks = blocks;
            this.craftingrecipeselected = craftingRecipeSelected;
            guistate = GuiState.CraftingRecipes;
            menustate = new MenuState();
            FreeMouse = true;
        }
        #endregion
        public Packet_CraftingRecipe[] craftingrecipes2;
        List<int> craftingblocks;
        ManicDigger.Action<IntRef> craftingrecipeselected;

        int craftingselectedrecipe = 0;
        List<int> okrecipes;
        private void DrawCraftingRecipes()
        {
            List<int> okrecipes = new List<int>();
            this.okrecipes = okrecipes;
            for (int i = 0; i < craftingrecipes2.Length; i++)
            {
                Packet_CraftingRecipe r = craftingrecipes2[i];
                if (r == null)
                {
                    continue;
                }
                //can apply recipe?
                foreach (Packet_Ingredient ingredient in r.Ingredients)
                {
                    if (ingredient == null)
                    {
                        continue;
                    }
                    if (craftingblocks.FindAll(v => v == ingredient.Type).Count < ingredient.Amount)
                    {
                        goto next;
                    }
                }
                okrecipes.Add(i);
            next:
                ;
            }
            int menustartx = xcenter(600);
            int menustarty = ycenter(okrecipes.Count * 80);
            if (okrecipes.Count == 0)
            {
                Draw2dText1(language.NoMaterialsForCrafting(), xcenter(200), ycenter(20), 12, null, false);
                return;
            }
            for (int i = 0; i < okrecipes.Count; i++)
            {
                Packet_CraftingRecipe r = craftingrecipes2[okrecipes[i]];
                for (int ii = 0; ii < r.IngredientsCount; ii++)
                {
                    int xx = menustartx + 20 + ii * 130;
                    int yy = menustarty + i * 80;
                    Draw2dTexture(game.d_TerrainTextures.terrainTexture(), xx, yy, 30, 30, IntRef.Create(game.TextureIdForInventory[r.Ingredients[ii].Type]), game.terrainTexture, Game.ColorFromArgb(255, 255, 255, 255), false);
                    Draw2dText1(string.Format("{0} {1}", r.Ingredients[ii].Amount, game.blocktypes[r.Ingredients[ii].Type].Name), xx + 50, yy, 12,
                       IntRef.Create(i == craftingselectedrecipe ? Game.ColorFromArgb(255, 255, 0, 0) : Game.ColorFromArgb(255, 255, 255, 255)), false);
                }
                {
                    int xx = menustartx + 20 + 400;
                    int yy = menustarty + i * 80;
                    Draw2dTexture(game.d_TerrainTextures.terrainTexture(), xx, yy, 40, 40, IntRef.Create(game.TextureIdForInventory[r.Output.Type]), game.terrainTexture, Game.ColorFromArgb(255, 255, 255, 255), false);
                    Draw2dText1(string.Format("{0} {1}", r.Output.Amount, game.blocktypes[r.Output.Type].Name), xx + 50, yy, 12,
                      IntRef.Create(i == craftingselectedrecipe ? Game.ColorFromArgb(255, 255, 0, 0) : Game.ColorFromArgb(255, 255, 255, 255)), false);
                }
            }
        }
    }
}
