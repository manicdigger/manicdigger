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
        public void CraftingRecipesStart(CraftingRecipe[] recipes, List<int> blocks, Action<int?> craftingRecipeSelected)
        {
            this.craftingrecipes2 = recipes;
            this.craftingblocks = blocks;
            this.craftingrecipeselected = craftingRecipeSelected;
            guistate = GuiState.CraftingRecipes;
            menustate = new MenuState();
            FreeMouse = true;
        }
        #endregion
        public CraftingRecipe[] craftingrecipes2;
        List<int> craftingblocks;
        ManicDigger.Action<int?> craftingrecipeselected;

        int craftingselectedrecipe = 0;
        List<int> okrecipes;
        private void DrawCraftingRecipes()
        {
            List<int> okrecipes = new List<int>();
            this.okrecipes = okrecipes;
            for (int i = 0; i < craftingrecipes2.Length; i++)
            {
                CraftingRecipe r = craftingrecipes2[i];
                //can apply recipe?
                foreach (Ingredient ingredient in r.ingredients)
                {
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
                Draw2dText(Language.NoMaterialsForCrafting, xcenter(200), ycenter(20), 12, Color.White);
                return;
            }
            for (int i = 0; i < okrecipes.Count; i++)
            {
                CraftingRecipe r = craftingrecipes2[okrecipes[i]];
                for (int ii = 0; ii < r.ingredients.Length; ii++)
                {
                    int xx = menustartx + 20 + ii * 130;
                    int yy = menustarty + i * 80;
                    Draw2dTexture(d_TerrainTextures.terrainTexture, xx, yy, 30, 30, d_Data.TextureIdForInventory[r.ingredients[ii].Type]);
                    Draw2dText(string.Format("{0} {1}", r.ingredients[ii].Amount, blocktypes[r.ingredients[ii].Type].Name), xx + 50, yy, 12,
                        i == craftingselectedrecipe ? Color.Red : Color.White);
                }
                {
                    int xx = menustartx + 20 + 400;
                    int yy = menustarty + i * 80;
                    Draw2dTexture(d_TerrainTextures.terrainTexture, xx, yy, 40, 40, d_Data.TextureIdForInventory[r.output.Type]);
                    Draw2dText(string.Format("{0} {1}", r.output.Amount, blocktypes[r.output.Type].Name), xx + 50, yy, 12,
                        i == craftingselectedrecipe ? Color.Red : Color.White);
                }
            }
        }
    }

    [ProtoContract()]
    public class Ingredient
    {
        [ProtoMember(1)]
        public int Type;
        [ProtoMember(2)]
        public int Amount;
    }

    [ProtoContract()]
    public class CraftingRecipe
    {
        [ProtoMember(1)]
        public Ingredient[] ingredients;
        [ProtoMember(2)]
        public Ingredient output;
    }
}
