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
            if (e.player == null) { continue; }
            if (e.player.CurrentTexture != -1)
            {
                continue;
            }
            // a) download skin
            if (!game.issingleplayer && e.player.Type == PlayerType.Player && e.player.Texture == null)
            {
                if (e.player.SkinDownloadResponse == null)
                {
                    e.player.SkinDownloadResponse = new HttpResponseCi();
                    string url = StringTools.StringAppend(game.platform, skinserver, StringTools.StringSubstringToEnd(game.platform, e.player.Name, 2));
                    url = StringTools.StringAppend(game.platform, url, ".png");
                    game.platform.WebClientDownloadDataAsync(url, e.player.SkinDownloadResponse);
                    continue;
                }
                if (!e.player.SkinDownloadResponse.error)
                {
                    if (!e.player.SkinDownloadResponse.done)
                    {
                        continue;
                    }
                    BitmapCi bmp_ = game.platform.BitmapCreateFromPng(e.player.SkinDownloadResponse.value, e.player.SkinDownloadResponse.valueLength);
                    if (bmp_ != null)
                    {
                        e.player.CurrentTexture = game.GetTextureOrLoad(e.player.Name, bmp_);
                        game.platform.BitmapDelete(bmp_);
                        continue;
                    }
                }
            }
            // b) file skin
            if (e.player.Texture == null)
            {
                e.player.CurrentTexture = game.GetTexture("mineplayer.png");
                continue;
            }

            byte[] file = game.GetFile(e.player.Texture);
            if (file == null)
            {
                e.player.CurrentTexture = 0;
                continue;
            }
            BitmapCi bmp = game.platform.BitmapCreateFromPng(file, game.platform.ByteArrayLength(file));
            if (bmp == null)
            {
                e.player.CurrentTexture = 0;
                continue;
            }
            e.player.CurrentTexture = game.GetTextureOrLoad(e.player.Texture, bmp);
            game.platform.BitmapDelete(bmp);
        }
    }
}
