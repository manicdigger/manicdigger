public class ModLoadPlayerTextures : ClientMod
{
    public override void OnNewFrame(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        if (!started)
        {
            started = true;
            if (!game.issingleplayer)
            {
                skinserverResponse = new HttpResponseCi();
                game.platform.WebClientDownloadDataAsync("http://manicdigger.sourceforge.net/skinserver.txt", skinserverResponse);
            }
        }
        LoadPlayerTextures(game);
    }
    bool started;
    internal string skinserver;
    internal HttpResponseCi skinserverResponse;
    internal void LoadPlayerTextures(Game game)
    {
        if (!game.issingleplayer)
        {
            if (skinserverResponse.done)
            {
                skinserver = game.platform.StringFromUtf8ByteArray(skinserverResponse.value, skinserverResponse.valueLength);
            }
            else if (skinserverResponse.error)
            {
                skinserver = null;
            }
            else
            {
                return;
            }
        }
        for (int i = 0; i < game.entitiesCount; i++)
        {
            Entity e = game.entities[i];
            if (e == null) { continue; }
            if (e.drawModel == null) { continue; }
            if (e.drawModel.CurrentTexture != -1)
            {
                continue;
            }
            // a) download skin
            if (!game.issingleplayer && e.drawModel.DownloadSkin && skinserver != null && e.drawModel.Texture_ == null)
            {
                if (e.drawModel.SkinDownloadResponse == null)
                {
                    e.drawModel.SkinDownloadResponse = new HttpResponseCi();
                    string url = StringTools.StringAppend(game.platform, skinserver, StringTools.StringSubstringToEnd(game.platform, e.drawName.Name, 2));
                    url = StringTools.StringAppend(game.platform, url, ".png");
                    game.platform.WebClientDownloadDataAsync(url, e.drawModel.SkinDownloadResponse);
                    continue;
                }
                if (!e.drawModel.SkinDownloadResponse.error)
                {
                    if (!e.drawModel.SkinDownloadResponse.done)
                    {
                        continue;
                    }
                    BitmapCi bmp_ = game.platform.BitmapCreateFromPng(e.drawModel.SkinDownloadResponse.value, e.drawModel.SkinDownloadResponse.valueLength);
                    if (bmp_ != null)
                    {
                        e.drawModel.CurrentTexture = game.GetTextureOrLoad(e.drawName.Name, bmp_);
                        game.platform.BitmapDelete(bmp_);
                        continue;
                    }
                }
            }
            // b) file skin
            if (e.drawModel.Texture_ == null)
            {
                e.drawModel.CurrentTexture = game.GetTexture("mineplayer.png");
                continue;
            }

            byte[] file = game.GetFile(e.drawModel.Texture_);
            if (file == null)
            {
                e.drawModel.CurrentTexture = 0;
                continue;
            }
            BitmapCi bmp = game.platform.BitmapCreateFromPng(file, game.platform.ByteArrayLength(file));
            if (bmp == null)
            {
                e.drawModel.CurrentTexture = 0;
                continue;
            }
            e.drawModel.CurrentTexture = game.GetTextureOrLoad(e.drawModel.Texture_, bmp);
            game.platform.BitmapDelete(bmp);
        }
    }
}
