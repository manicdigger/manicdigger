using System;

namespace ManicDigger.Mods
{
	/// <summary>
	/// This class contains all crafting recipes
	/// </summary>
	public class CoreCrafting : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("CoreBlocks");
		}
		public void Start(ModManager m)
		{
			/* Crafting recipes are given in the following style:
			 * 
			 * m.AddCraftingRecipe ("Result", 1, "Ingredient_1", 1);
			 * m.AddCraftingRecipe2("Result", 1, "Ingredient_1", 1, "Ingredient_2", 1);
			 * m.AddCraftingRecipe3("Result", 1, "Ingredient_1", 1, "Ingredient_2", 1, "Ingredient_3", 1);
			 */

			m.AddCraftingRecipe("Cobblestone", 1, "Stone", 2);
			m.AddCraftingRecipe("Stone", 2, "Cobblestone", 1);
			m.AddCraftingRecipe("OakWood", 2, "OakTreeTrunk", 1);
			m.AddCraftingRecipe("BirchWood", 2, "BirchTreeTrunk", 1);
			m.AddCraftingRecipe("SpruceWood", 2, "SpruceTreeTrunk", 1);
			m.AddCraftingRecipe("Brick", 1, "Stone", 4);
			m.AddCraftingRecipe("CraftingTable", 1, "OakWood", 3);
			m.AddCraftingRecipe("CraftingTable", 1, "BirchWood", 3);
			m.AddCraftingRecipe("CraftingTable", 1, "SpruceWood", 3);
			m.AddCraftingRecipe("Stair", 1, "Stone", 1);
			m.AddCraftingRecipe("Stair", 2, "DoubleStair", 1);
			m.AddCraftingRecipe("DoubleStair", 1, "Stone", 2);
			m.AddCraftingRecipe("DoubleStair", 1, "Stair", 2);
			m.AddCraftingRecipe("Glass", 1, "Sand", 2);
			m.AddCraftingRecipe("RedRoseDecorations", 1, "OakLeaves", 10);
			m.AddCraftingRecipe("RedRoseDecorations", 1, "BirchLeaves", 10);
			m.AddCraftingRecipe("RedRoseDecorations", 1, "SpruceLeaves", 10);
			m.AddCraftingRecipe("YellowFlowerDecorations", 1, "OakLeaves", 10);
			m.AddCraftingRecipe("YellowFlowerDecorations", 1, "BirchLeaves", 10);
			m.AddCraftingRecipe("YellowFlowerDecorations", 1, "SpruceLeaves", 10);
			m.AddCraftingRecipe("OakSapling", 1, "OakLeaves", 3);
			m.AddCraftingRecipe("BirchSapling", 1, "BirchLeaves", 3);
			m.AddCraftingRecipe("SpruceSapling", 1, "SpruceLeaves", 3);
			m.AddCraftingRecipe("RedMushroom", 1, "Dirt", 10);
			m.AddCraftingRecipe("BrownMushroom", 1, "Dirt", 10);
			m.AddCraftingRecipe("RedMushroom", 1, "Grass", 10);
			m.AddCraftingRecipe("BrownMushroom", 1, "Grass", 10);
			m.AddCraftingRecipe("Bookcase", 1, "OakWood", 2);
			m.AddCraftingRecipe("Bookcase", 1, "BirchWood", 2);
			m.AddCraftingRecipe("Bookcase", 1, "SpruceWood", 2);
			m.AddCraftingRecipe("MossyCobblestone", 1, "Cobblestone", 1);
			m.AddCraftingRecipe("Cobblestone", 1, "MossyCobblestone", 1);
			m.AddCraftingRecipe("Sponge", 1, "GoldBlock", 1);
			m.AddCraftingRecipe("RedCloth", 1, "GoldBlock", 1);
			m.AddCraftingRecipe("OrangeCloth", 1, "RedCloth", 1);
			m.AddCraftingRecipe("YellowCloth", 1, "OrangeCloth", 1);
			m.AddCraftingRecipe("LightGreenCloth", 1, "YellowCloth", 1);
			m.AddCraftingRecipe("GreenCloth", 1, "LightGreenCloth", 1);
			m.AddCraftingRecipe("AquaGreenCloth", 1, "GreenCloth", 1);
			m.AddCraftingRecipe("CyanCloth", 1, "AquaGreenCloth", 1);
			m.AddCraftingRecipe("BlueCloth", 1, "CyanCloth", 1);
			m.AddCraftingRecipe("PurpleCloth", 1, "BlueCloth", 1);
			m.AddCraftingRecipe("IndigoCloth", 1, "PurpleCloth", 1);
			m.AddCraftingRecipe("VioletCloth", 1, "IndigoCloth", 1);
			m.AddCraftingRecipe("MagentaCloth", 1, "VioletCloth", 1);
			m.AddCraftingRecipe("PinkCloth", 1, "MagentaCloth", 1);
			m.AddCraftingRecipe("BlackCloth", 1, "PinkCloth", 1);
			m.AddCraftingRecipe("GrayCloth", 1, "BlackCloth", 1);
			m.AddCraftingRecipe("WhiteCloth", 1, "GrayCloth", 1);
			m.AddCraftingRecipe("RedCloth", 1, "WhiteCloth", 1);
			m.AddCraftingRecipe("RedCloth", 1, "RedCarpet", 4);
			m.AddCraftingRecipe("OrangeCloth", 1, "OrangeCarpet", 4);
			m.AddCraftingRecipe("YellowCloth", 1, "YellowCarpet", 4);
			m.AddCraftingRecipe("LightGreenCloth", 1, "LightGreenCarpet", 4);
			m.AddCraftingRecipe("GreenCloth", 1, "GreenCarpet", 4);
			m.AddCraftingRecipe("AquaGreenCloth", 1, "AquaGreenCarpet", 4);
			m.AddCraftingRecipe("CyanCloth", 1, "CyanCarpet", 4);
			m.AddCraftingRecipe("BlueCloth", 1, "BlueCarpet", 4);
			m.AddCraftingRecipe("PurpleCloth", 1, "PurpleCarpet", 4);
			m.AddCraftingRecipe("IndigoCloth", 1, "IndigoCarpet", 4);
			m.AddCraftingRecipe("VioletCloth", 1, "VioletCarpet", 4);
			m.AddCraftingRecipe("MagentaCloth", 1, "MagentaCarpet", 4);
			m.AddCraftingRecipe("PinkCloth", 1, "PinkCarpet", 4);
			m.AddCraftingRecipe("BlackCloth", 1, "BlackCarpet", 4);
			m.AddCraftingRecipe("GrayCloth", 1, "GrayCarpet", 4);
			m.AddCraftingRecipe("WhiteCloth", 1, "WhiteCarpet", 4);
			m.AddCraftingRecipe("RedCarpet", 4, "RedCloth", 1);
			m.AddCraftingRecipe("OrangeCarpet", 4, "OrangeCloth", 1);
			m.AddCraftingRecipe("YellowCarpet", 4, "YellowCloth", 1);
			m.AddCraftingRecipe("LightGreenCarpet", 4, "LightGreenCloth", 1);
			m.AddCraftingRecipe("GreenCarpet", 4, "GreenCloth", 1);
			m.AddCraftingRecipe("AquaGreenCarpet", 4, "AquaGreenCloth", 1);
			m.AddCraftingRecipe("CyanCarpet", 4, "CyanCloth", 1);
			m.AddCraftingRecipe("BlueCarpet", 4, "BlueCloth", 1);
			m.AddCraftingRecipe("PurpleCarpet", 4, "PurpleCloth", 1);
			m.AddCraftingRecipe("IndigoCarpet", 4, "IndigoCloth", 1);
			m.AddCraftingRecipe("VioletCarpet", 4, "VioletCloth", 1);
			m.AddCraftingRecipe("MagentaCarpet", 4, "MagentaCloth", 1);
			m.AddCraftingRecipe("PinkCarpet", 4, "PinkCloth", 1);
			m.AddCraftingRecipe("BlackCarpet", 4, "BlackCloth", 1);
			m.AddCraftingRecipe("GrayCarpet", 4, "GrayCloth", 1);
			m.AddCraftingRecipe("WhiteCarpet", 4, "WhiteCloth", 1);
			m.AddCraftingRecipe("Roof", 1, "Brick", 2);
			m.AddCraftingRecipe("ChemicalGreen", 1, "GoldBlock", 1);
			m.AddCraftingRecipe("Camouflage", 1, "GoldBlock", 1);
			m.AddCraftingRecipe("DirtForFarming", 1, "Dirt", 2);
			m.AddCraftingRecipe("DirtForFarming", 1, "Grass", 2);
			m.AddCraftingRecipe("Crops1", 2, "Crops4", 1);
			m.AddCraftingRecipe("Minecart", 1, "BrushedMetal", 5);
			m.AddCraftingRecipe("Salt", 1, "Crops4", 2);
			m.AddCraftingRecipe("LuxuryRoof", 1, "Roof", 2);
			m.AddCraftingRecipe("Fence", 1, "OakTreeTrunk", 2);
			m.AddCraftingRecipe("Fence", 1, "BirchTreeTrunk", 2);
			m.AddCraftingRecipe("Fence", 1, "SpruceTreeTrunk", 2);
			m.AddCraftingRecipe("Hay", 1, "Crops4", 4);
			m.AddCraftingRecipe("SilverCoin", 1, "SilverOre", 1);
			m.AddCraftingRecipe("SilverCoin", 30, "GoldCoin", 1);
			m.AddCraftingRecipe("GoldCoin", 30, "GoldBar", 1);
			m.AddCraftingRecipe("Ladder", 1, "OakWood", 4);
			m.AddCraftingRecipe("Ladder", 1, "BirchWood", 4);
			m.AddCraftingRecipe("Ladder", 1, "SpruceWood", 4);
			m.AddCraftingRecipe("Sandstone", 1, "Sand", 4);
			m.AddCraftingRecipe("RedSandstone", 1, "RedSand", 4);
			m.AddCraftingRecipe("HalfCobblestone", 2, "Cobblestone", 1);
			m.AddCraftingRecipe("HalfMossyCobblestone", 2, "MossyCobblestone", 1);
			m.AddCraftingRecipe("HalfOakWood", 2, "OakWood", 1);
			m.AddCraftingRecipe("HalfBirchWood", 2, "BirchWood", 1);
			m.AddCraftingRecipe("HalfSpruceWood", 2, "SpruceWood", 1);
			m.AddCraftingRecipe("HalfBrick", 2, "Brick", 1);
			m.AddCraftingRecipe("HalfSandBrick", 2, "SandBrick", 1);
			m.AddCraftingRecipe("Cobblestone", 1, "HalfCobblestone", 2);
			m.AddCraftingRecipe("MossyCobblestone", 1, "HalfMossyCobblestone", 2);
			m.AddCraftingRecipe("OakWood", 1, "HalfOakWood", 2);
			m.AddCraftingRecipe("BirchWood", 1, "HalfBirchWood", 2);
			m.AddCraftingRecipe("SpruceWood", 1, "HalfSpruceWood", 2);
			m.AddCraftingRecipe("Brick", 1, "HalfBrick", 2);
			m.AddCraftingRecipe("SandBrick", 1, "HalfSandBrick", 2);

			m.AddCraftingRecipe2("GoldBlock", 1, "CoalOre", 1, "GoldOre", 1);
			m.AddCraftingRecipe2("IronBlock", 1, "CoalOre", 1, "IronOre", 1);
			m.AddCraftingRecipe2("Rail3", 4, "OakWood", 1, "IronBlock", 1);
			m.AddCraftingRecipe2("Rail3", 4, "BirchWood", 1, "IronBlock", 1);
			m.AddCraftingRecipe2("Rail3", 4, "SpruceWood", 1, "IronBlock", 1);
			m.AddCraftingRecipe2("Rail60", 2, "OakWood", 1, "IronBlock", 1);
			m.AddCraftingRecipe2("Rail60", 2, "BirchWood", 1, "IronBlock", 1);
			m.AddCraftingRecipe2("Rail60", 2, "SpruceWood", 1, "IronBlock", 1);
			m.AddCraftingRecipe2("Trampoline", 1, "BrushedMetal", 1, "OakWood", 1);
			m.AddCraftingRecipe2("Trampoline", 1, "BrushedMetal", 1, "BirchWood", 1);
			m.AddCraftingRecipe2("Trampoline", 1, "BrushedMetal", 1, "SpruceWood", 1);
			m.AddCraftingRecipe2("Torch", 1, "OakWood", 1, "CoalOre", 1);
			m.AddCraftingRecipe2("Torch", 1, "BirchWood", 1, "CoalOre", 1);
			m.AddCraftingRecipe2("Torch", 1, "SpruceWood", 1, "CoalOre", 1);
			m.AddCraftingRecipe2("GrassTrap", 1, "Dirt", 10, "Camouflage", 5);
			m.AddCraftingRecipe2("OakSapling", 10, "Apples", 5, "DirtForFarming", 1);
			m.AddCraftingRecipe2("BirchSapling", 10, "Apples", 5, "DirtForFarming", 1);
			m.AddCraftingRecipe2("SpruceSapling", 10, "Apples", 5, "DirtForFarming", 1);
			m.AddCraftingRecipe2("DirtBrick", 1, "Dirt", 2, "Stone", 1);
			m.AddCraftingRecipe2("BrushedMetal", 1, "IronBlock", 1, "CoalOre", 1);
			m.AddCraftingRecipe2("SandBrick", 1, "Sand", 1, "Stone", 2);
			m.AddCraftingRecipe2("FakeBookcase", 1, "Bookcase", 1, "Camouflage", 5);
			m.AddCraftingRecipe2("WoodDesk", 1, "OakWood", 2, "OakTreeTrunk", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "OakWood", 2, "BirchTreeTrunk", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "OakWood", 2, "SpruceTreeTrunk", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "BirchWood", 2, "OakTreeTrunk", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "BirchWood", 2, "BirchTreeTrunk", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "BirchWood", 2, "SpruceTreeTrunk", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "SpruceWood", 2, "OakWood", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "SpruceWood", 2, "BirchWood", 1);
			m.AddCraftingRecipe2("WoodDesk", 1, "SpruceWood", 2, "SpruceWood", 1);
			m.AddCraftingRecipe2("GlassDesk", 1, "Glass", 2, "OakTreeTrunk", 1);
			m.AddCraftingRecipe2("GlassDesk", 1, "Glass", 2, "BirchTreeTrunk", 1);
			m.AddCraftingRecipe2("GlassDesk", 1, "Glass", 2, "SpruceTreeTrunk", 1);
			m.AddCraftingRecipe2("Asphalt", 1, "CoalOre", 1, "Gravel", 2);
			m.AddCraftingRecipe2("Cake", 1, "Salt", 2, "Crops4", 4);
			m.AddCraftingRecipe2("Fire", 1, "OakTreeTrunk", 1, "Torch", 1);
			m.AddCraftingRecipe2("Fire", 1, "BirchTreeTrunk", 1, "Torch", 1);
			m.AddCraftingRecipe2("Fire", 1, "SpruceTreeTrunk", 1, "Torch", 1);
			m.AddCraftingRecipe2("GoldBar", 1, "GoldCoin", 25, "GoldBlock", 5);
			m.AddCraftingRecipe2("GoldCoin", 1, "SilverCoin", 25, "GoldOre", 5);

			m.AddCraftingRecipe3("Mosaik", 1, "Sand", 2, "Gravel", 1, "Stone", 1);
		}
	}
}
