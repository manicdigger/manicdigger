using System;
using System.Collections.Generic;
using System.Text;
using ManicDigger;

namespace GameModeFortress
{
    public class CraftingRecipes
    {
        public IGameData data;

        public CraftingRecipes()
        {
        }

        public void Load(string[] csvLines)
        {
            this.csv = new Csv();
            this.csv.LoadCsv(csvLines);
            for (int i = 1; i < csv.data.Length; i++)
            {
                CraftingRecipe r = new CraftingRecipe();

                int outputId = GetBlockId(csv.Get(i, "Output"));
                int outputAmount = csv.GetInt(i, "OutputAmount");
                int input0Id = GetBlockId(csv.Get(i, "Input0"));
                int input0Amount = csv.GetInt(i, "Input0Amount");
                int input1Id = GetBlockId(csv.Get(i, "Input1"));
                int input1Amount = csv.GetInt(i, "Input1Amount");
                int input2Id = GetBlockId(csv.Get(i, "Input2"));
                int input2Amount = csv.GetInt(i, "Input2Amount");
                                                                
                if (input0Id != -1)
                {
                    r.ingredients.Add(new Ingredient() { Type = input0Id, Amount = input0Amount });
                }
                if (input1Id != -1)
                {
                    r.ingredients.Add(new Ingredient() { Type = input1Id, Amount = input1Amount });
                }
                if (input2Id != -1)
                {
                    r.ingredients.Add(new Ingredient() { Type = input2Id, Amount = input2Amount });
                }

                if (outputId == -1) { throw new FormatException(); }
                if (r.ingredients.Count == 0) { throw new FormatException("Invalid ingredients in recipe " + i); }

                r.output = new Ingredient() { Type = outputId, Amount = outputAmount };
                craftingrecipes.Add(r);
            }
        }

        private int GetBlockId(string blockName)
        {
            int blockId = -1;
            for (int k = 0; k < data.Name.Length; k++)
            {
                if (data.Name[k] != null
                    && data.Name[k].Equals(blockName, StringComparison.InvariantCultureIgnoreCase))
                {
                    blockId = k;
                }
            }
            return blockId;
        }

        Csv csv;
        public List<CraftingRecipe> craftingrecipes = new List<CraftingRecipe>();
    }
}
