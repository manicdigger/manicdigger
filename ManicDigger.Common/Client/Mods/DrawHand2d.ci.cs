public class ModDrawHand2d : ClientMod
{
	internal string lasthandimage;
	public override void OnNewFrameDraw3d(Game game, float deltaTime)
	{
		if (ShouldDrawHand(game))
		{
			string img = HandImage2d(game);
			if (img != null)
			{
				game.rend.OrthoMode(game.Width(), game.Height());
				if (lasthandimage != img)
				{
					lasthandimage = img;
					byte[] file = game.GetFile(img);
					BitmapCi bmp = game.platform.BitmapCreateFromPng(file, game.platform.ByteArrayLength(file));
					if (bmp != null)
					{
						game.handTexture = game.platform.LoadTextureFromBitmap(bmp);
						game.platform.BitmapDelete(bmp);
					}
				}
				game.rend.Draw2dTexture(game.handTexture, game.Width() / 2, game.Height() - 512, 512, 512, null, 0, ColorCi.FromArgb(255, 255, 255, 255), false);
                game.rend.PerspectiveMode();
			}
		}
	}

	public static bool ShouldDrawHand(Game game)
	{
		return (!game.ENABLE_TPP_VIEW) && game.ENABLE_DRAW2D;
	}

	public static string HandImage2d(Game game)
	{
		Packet_Item item = game.d_Inventory.RightHand[game.ActiveHudIndex];
		string img = null;
		if (item != null)
		{
			img = game.blocktypes[item.BlockId].Handimage;
			if (game.IronSights)
			{
				img = game.blocktypes[item.BlockId].IronSightsImage;
			}
		}
		return img;
	}
}
