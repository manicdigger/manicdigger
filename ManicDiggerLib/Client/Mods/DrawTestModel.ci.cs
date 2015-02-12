public class ModDrawTestModel : ClientMod
{
    public override void OnNewFrameDraw3d(Game game, float deltaTime)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }

        DrawTestModel(game, deltaTime);
    }

    void DrawTestModel(Game game, float deltaTime)
    {
        if (!game.ENABLE_DRAW_TEST_CHARACTER)
        {
            return;
        }
        if (testmodel == null)
        {
            testmodel = new AnimatedModelRenderer();
            byte[] data = game.GetFile("player.txt");
            int dataLength = game.GetFileLength("player.txt");
            string dataString = game.platform.StringFromUtf8ByteArray(data, dataLength);
            AnimatedModel model = AnimatedModelSerializer.Deserialize(game.platform, dataString);
            testmodel.Start(game, model);
        }
        game.GLPushMatrix();
        game.GLTranslate(game.map.MapSizeX / 2, game.blockheight(game.map.MapSizeX / 2, game.map.MapSizeY / 2 - 2, 128), game.map.MapSizeY / 2 - 2);
        game.platform.BindTexture2d(game.GetTexture("mineplayer.png"));
        testmodel.Render(deltaTime, 0, true, true, 1);
        game.GLPopMatrix();
    }
    AnimatedModelRenderer testmodel;

    public override bool OnClientCommand(Game game, ClientCommandArgs args)
    {
        if (args.command == "testmodel")
        {
            game.ENABLE_DRAW_TEST_CHARACTER = game.BoolCommandArgument(args.arguments);
            return true;
        }
        return false;
    }
}
