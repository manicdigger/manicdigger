public class CoreComands : ClientMod
{
    public CoreComands()
    {

    }
    public override bool OnClientCommand(Game game, ClientCommandArgs args) {
        string cmd = args.command;
        string arguments = args.arguments;
        // Command requiring no arguments
        string strFreemoveNotAllowed = game.language.FreemoveNotAllowed();

        switch (cmd)
        {
            case "clients":
                game.Log("Clients:");
                for (int i = 0; i < game.entitiesCount; i++)
                {
                    Entity entity = game.entities[i];
                    if (entity == null) { continue; }
                    if (entity.drawName == null) { continue; }
                    if (!entity.drawName.ClientAutoComplete) { continue; }
                    game.Log(game.platform.StringFormat2("{0} {1}", game.platform.IntToString(i), game.entities[i].drawName.Name));
                }

                break;
            case "reconnect":
                game.Reconnect();
                break;
            case "m":
                game.mouseSmoothing = !game.mouseSmoothing;
                if (game.mouseSmoothing) { game.Log("Mouse smoothing enabled."); }
                else { game.Log("Mouse smoothing disabled."); }

                break;
            case "noclip":
            case "freemove":
                if (game.AllowFreemove)
                {
                    game.stopPlayerMove = true;

                    if (game.BoolCommandArgument(arguments))
                    {
                        
                        game.controls.SetFreemove((cmd == "noclip")? FreemoveLevelEnum.Noclip :FreemoveLevelEnum.Freemove);
                    }
                    else
                    {
                        game.controls.SetFreemove(FreemoveLevelEnum.None);
                    }
                }
                else
                {
                    game.Log(strFreemoveNotAllowed);
                    return true;
                }

                break;
            case "gui":
                game.ENABLE_DRAW2D = game.BoolCommandArgument(arguments);
                break;
        }
        // Commands requiring numeric arguments
        if (arguments != "")
        {
            if (cmd == "fog")
            {
                int foglevel;
                foglevel = game.platform.IntParse(arguments);
                {
                    int foglevel2 = foglevel;
                    if (foglevel2 > 1024)
                    {
                        foglevel2 = 1024;
                    }
                    if (foglevel2 % 2 == 0)
                    {
                        foglevel2--;
                    }
                    game.d_Config3d.viewdistance = foglevel2;
                }
                game.OnResize();
            }
            else if (cmd == "fov")
            {
                int arg = game.platform.IntParse(arguments);
                int minfov = 1;
                int maxfov = 179;
                if (!game.issingleplayer)
                {
                    minfov = 60;
                }
                if (arg < minfov || arg > maxfov)
                {
                    game.Log(game.platform.StringFormat2("Valid field of view: {0}-{1}", game.platform.IntToString(minfov), game.platform.IntToString(maxfov)));
                }
                else
                {
                    float fov_ = (2 * Game.GetPi() * (game.one * arg / 360));
                    game.fov = fov_;
                    game.OnResize();
                }
            }
            else if (cmd == "movespeed")
            {
                if (game.AllowFreemove)
                {
                    if (game.platform.FloatParse(arguments) <= 500)
                    {
                        game.movespeed = game.basemovespeed * game.platform.FloatParse(arguments);
                        game.AddChatline(game.platform.StringFormat("Movespeed: {0}x", arguments));
                    }
                    else
                    {
                        game.AddChatline("Entered movespeed to high! max. 500x");
                    }
                }
                else
                {
                    game.Log(strFreemoveNotAllowed);
                    return true;
                }
            }
            else if (cmd == "serverinfo")
            {
                //Fetches server info from given adress
                IntRef splitCount = new IntRef();
                string[] split = game.platform.StringSplit(arguments, ":", splitCount);
                if (splitCount.value == 2)
                {
                    QueryClient qClient = new QueryClient();
                    qClient.SetPlatform(game.platform);
                    qClient.PerformQuery(split[0], game.platform.IntParse(split[1]));
                    if (qClient.querySuccess)
                    {
                        //Received result
                        QueryResult r = qClient.GetResult();
                        game.AddChatline(r.GameMode);
                        game.AddChatline(game.platform.IntToString(r.MapSizeX));
                        game.AddChatline(game.platform.IntToString(r.MapSizeY));
                        game.AddChatline(game.platform.IntToString(r.MapSizeZ));
                        game.AddChatline(game.platform.IntToString(r.MaxPlayers));
                        game.AddChatline(r.MOTD);
                        game.AddChatline(r.Name);
                        game.AddChatline(game.platform.IntToString(r.PlayerCount));
                        game.AddChatline(r.PlayerList);
                        game.AddChatline(game.platform.IntToString(r.Port));
                        game.AddChatline(r.PublicHash);
                        game.AddChatline(r.ServerVersion);
                    }
                    game.AddChatline(qClient.GetServerMessage());
                }
            }
        }

        return false;
    
      }

}