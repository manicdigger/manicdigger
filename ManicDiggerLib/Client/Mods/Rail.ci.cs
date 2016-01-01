public class ModRail : ClientMod
{
    public ModRail()
    {
        one = 1;
        railheight = one * 3 / 10;
    }
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (d_RailMapUtil == null)
        {
            this.d_RailMapUtil = new RailMapUtil();
            this.d_RailMapUtil.game = game;
        }
        RailOnNewFrame(game, args.GetDt());
    }

    float one;

    internal Entity localMinecart;
    internal float minecartheight() { return one / 2; }
    internal bool wasqpressed;
    internal bool wasepressed;

    internal void RailOnNewFrame(Game game, float dt)
    {
        if (localMinecart == null)
        {
            localMinecart = new Entity();
            localMinecart.minecart = new Minecart();
            game.EntityAddLocal(localMinecart);
        }
        localMinecart.minecart.enabled = railriding;
        if (railriding)
        {
            Minecart m = localMinecart.minecart;
            m.positionX = game.player.position.x;
            m.positionY = game.player.position.y;
            m.positionZ = game.player.position.z;
            m.direction = currentdirection;
            m.lastdirection = lastdirection;
            m.progress = currentrailblockprogress;
        }

        game.localplayeranimationhint.InVehicle = railriding;
        game.localplayeranimationhint.DrawFixX = 0;
        game.localplayeranimationhint.DrawFixY = railriding ? (-one * 7 / 10) : 0;
        game.localplayeranimationhint.DrawFixZ = 0;

        bool turnright = game.keyboardState[game.GetKey(GlKeys.D)];
        bool turnleft = game.keyboardState[game.GetKey(GlKeys.A)];
        RailSound(game);
        if (railriding)
        {
            game.controls.SetFreemove(FreemoveLevelEnum.Freemove);
            game.enable_move = false;
            Vector3Ref railPos = CurrentRailPos(game);
            game.player.position.x = railPos.X;
            game.player.position.y = railPos.Y;
            game.player.position.z = railPos.Z;
            currentrailblockprogress += currentvehiclespeed * dt;
            if (currentrailblockprogress >= 1)
            {
                lastdirection = currentdirection;
                currentrailblockprogress = 0;
                TileEnterData newenter = new TileEnterData();
                Vector3IntRef nexttile = NextTile(currentdirection, currentrailblockX, currentrailblockY, currentrailblockZ);
                newenter.BlockPositionX = nexttile.X;
                newenter.BlockPositionY = nexttile.Y;
                newenter.BlockPositionZ = nexttile.Z;
                //slope
                if (GetUpDownMove(game, currentrailblockX, currentrailblockY, currentrailblockZ,
                    DirectionUtils.ResultEnter(DirectionUtils.ResultExit(currentdirection))) == UpDown.Up)
                {
                    newenter.BlockPositionZ++;
                }
                if (GetUpDownMove(game, newenter.BlockPositionX,
                    newenter.BlockPositionY,
                    newenter.BlockPositionZ - 1,
                    DirectionUtils.ResultEnter(DirectionUtils.ResultExit(currentdirection))) == UpDown.Down)
                {
                    newenter.BlockPositionZ--;
                }

                newenter.EnterDirection = DirectionUtils.ResultEnter(DirectionUtils.ResultExit(currentdirection));
                BoolRef newdirFound = new BoolRef();
                VehicleDirection12 newdir = BestNewDirection(PossibleRails(game, newenter), turnleft, turnright, newdirFound);
                if (!newdirFound.value)
                {
                    //end of rail
                    currentdirection = DirectionUtils.Reverse(currentdirection);
                }
                else
                {
                    currentdirection = newdir;
                    currentrailblockX = game.platform.FloatToInt(newenter.BlockPositionX);
                    currentrailblockY = game.platform.FloatToInt(newenter.BlockPositionY);
                    currentrailblockZ = game.platform.FloatToInt(newenter.BlockPositionZ);
                }
            }
        }
        if (game.keyboardState[game.GetKey(GlKeys.W)] && game.GuiTyping != TypingState.Typing)
        {
            currentvehiclespeed += 1 * dt;
        }
        if (game.keyboardState[game.GetKey(GlKeys.S)] && game.GuiTyping != TypingState.Typing)
        {
            currentvehiclespeed -= 5 * dt;
        }
        if (currentvehiclespeed < 0)
        {
            currentvehiclespeed = 0;
        }
        //todo fix
        //if (viewport.keypressed != null && viewport.keypressed.Key == GlKeys.Q)            
        if (!wasqpressed && game.keyboardState[game.GetKey(GlKeys.Q)] && game.GuiTyping != TypingState.Typing)
        {
            Reverse();
        }
        if (!wasepressed && game.keyboardState[game.GetKey(GlKeys.E)] && !railriding && (game.controls.GetFreemove() == FreemoveLevelEnum.None) && game.GuiTyping != TypingState.Typing)
        {
            currentrailblockX = game.platform.FloatToInt(game.player.position.x);
            currentrailblockY = game.platform.FloatToInt(game.player.position.z);
            currentrailblockZ = game.platform.FloatToInt(game.player.position.y) - 1;
            if (!game.map.IsValidPos(currentrailblockX, currentrailblockY, currentrailblockZ))
            {
                ExitVehicle(game);
            }
            else
            {
                int railunderplayer = game.d_Data.Rail()[game.map.GetBlock(currentrailblockX, currentrailblockY, currentrailblockZ)];
                railriding = true;
                originalmodelheight = game.GetCharacterEyesHeight();
                game.SetCharacterEyesHeight(minecartheight());
                currentvehiclespeed = 0;
                if ((railunderplayer & RailDirectionFlags.Horizontal) != 0)
                {
                    currentdirection = VehicleDirection12.HorizontalRight;
                }
                else if ((railunderplayer & RailDirectionFlags.Vertical) != 0)
                {
                    currentdirection = VehicleDirection12.VerticalUp;
                }
                else if ((railunderplayer & RailDirectionFlags.UpLeft) != 0)
                {
                    currentdirection = VehicleDirection12.UpLeftUp;
                }
                else if ((railunderplayer & RailDirectionFlags.UpRight) != 0)
                {
                    currentdirection = VehicleDirection12.UpRightUp;
                }
                else if ((railunderplayer & RailDirectionFlags.DownLeft) != 0)
                {
                    currentdirection = VehicleDirection12.DownLeftLeft;
                }
                else if ((railunderplayer & RailDirectionFlags.DownRight) != 0)
                {
                    currentdirection = VehicleDirection12.DownRightRight;
                }
                else
                {
                    ExitVehicle(game);
                }
                lastdirection = currentdirection;
            }
        }
        else if (!wasepressed && game.keyboardState[game.GetKey(GlKeys.E)] && railriding && game.GuiTyping != TypingState.Typing)
        {
            ExitVehicle(game);
            game.player.position.y += one * 7 / 10;
        }
        wasqpressed = game.keyboardState[game.GetKey(GlKeys.Q)] && game.GuiTyping != TypingState.Typing;
        wasepressed = game.keyboardState[game.GetKey(GlKeys.E)] && game.GuiTyping != TypingState.Typing;
    }

    internal VehicleDirection12 BestNewDirection(int dirVehicleDirection12Flags, bool turnleft, bool turnright, BoolRef retFound)
    {
        // 0-- x
        // |
        // y

        // y is down, x is right
        // Naming: first the 2 connected directions followed by the preferred exit direction

        retFound.value = true;
        if (turnright)
        {
            // steering right
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownRightRight) != 0)
            {
                return VehicleDirection12.DownRightRight;
            }
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpRightUp) != 0)
            {
                return VehicleDirection12.UpRightUp;
            }
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpLeftLeft) != 0)
            {
                return VehicleDirection12.UpLeftLeft;
            }
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownLeftDown) != 0)
            {
                return VehicleDirection12.DownLeftDown;
            }
        }
        if (turnleft)
        {
            // steering left
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownRightDown) != 0)
            {
                return VehicleDirection12.DownRightDown;
            }
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpRightRight) != 0)
            {
                return VehicleDirection12.UpRightRight;
            }
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpLeftUp) != 0)
            {
                return VehicleDirection12.UpLeftUp;
            }
            if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownLeftLeft) != 0)
            {
                return VehicleDirection12.DownLeftLeft;
            }
        }

        // Handle driving straight first
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.VerticalDown) != 0) { return VehicleDirection12.VerticalDown; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.VerticalUp) != 0) { return VehicleDirection12.VerticalUp; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.HorizontalLeft) != 0) { return VehicleDirection12.HorizontalLeft; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.HorizontalRight) != 0) { return VehicleDirection12.HorizontalRight; }

        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownLeftDown) != 0){ return VehicleDirection12.DownLeftDown; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownLeftLeft) != 0) { return VehicleDirection12.DownLeftLeft; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownRightDown) != 0) { return VehicleDirection12.DownRightDown; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.DownRightRight) != 0) { return VehicleDirection12.DownRightRight; }

        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpLeftLeft) != 0) { return VehicleDirection12.UpLeftLeft; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpLeftUp) != 0) { return VehicleDirection12.UpLeftUp; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpRightRight) != 0) { return VehicleDirection12.UpRightRight; }
        if ((dirVehicleDirection12Flags & VehicleDirection12Flags.UpRightUp) != 0) { return VehicleDirection12.UpRightUp; }

        retFound.value = false;
        return VehicleDirection12.DownLeftDown; // return null
    }

    internal Vector3Ref CurrentRailPos(Game game)
    {
        RailSlope slope = d_RailMapUtil.GetRailSlope(currentrailblockX,
            currentrailblockY, currentrailblockZ);
        float aX = currentrailblockX;
        float aY = currentrailblockY;
        float aZ = currentrailblockZ;
        float x_correction = 0;
        float y_correction = 0;
        float z_correction = 0;
        float half = one / 2;
        switch (currentdirection)
        {
            case VehicleDirection12.HorizontalRight:
                x_correction += currentrailblockprogress;
                y_correction += half;
                if (slope == RailSlope.TwoRightRaised)
                    z_correction += currentrailblockprogress;
                if (slope == RailSlope.TwoLeftRaised)
                    z_correction += 1 - currentrailblockprogress;
                break;
            case VehicleDirection12.HorizontalLeft:
                x_correction += 1 - currentrailblockprogress;
                y_correction += half;
                if (slope == RailSlope.TwoRightRaised)
                    z_correction += 1 - currentrailblockprogress;
                if (slope == RailSlope.TwoLeftRaised)
                    z_correction += currentrailblockprogress;
                break;
            case VehicleDirection12.VerticalDown:
                x_correction += half;
                y_correction += currentrailblockprogress;
                if (slope == RailSlope.TwoDownRaised)
                    z_correction += currentrailblockprogress;
                if (slope == RailSlope.TwoUpRaised)
                    z_correction += 1 - currentrailblockprogress;
                break;
            case VehicleDirection12.VerticalUp:
                x_correction += half;
                y_correction += 1 - currentrailblockprogress;
                if (slope == RailSlope.TwoDownRaised)
                    z_correction += 1 - currentrailblockprogress;
                if (slope == RailSlope.TwoUpRaised)
                    z_correction += currentrailblockprogress;
                break;
            case VehicleDirection12.UpLeftLeft:
                x_correction += half * (1 - currentrailblockprogress);
                y_correction += half * currentrailblockprogress;
                break;
            case VehicleDirection12.UpLeftUp:
                x_correction += half * currentrailblockprogress;
                y_correction += half - half * currentrailblockprogress;
                break;
            case VehicleDirection12.UpRightRight:
                x_correction += half + half * currentrailblockprogress;
                y_correction += half * currentrailblockprogress;
                break;
            case VehicleDirection12.UpRightUp:
                x_correction += 1 - half * currentrailblockprogress;
                y_correction += half - half * currentrailblockprogress;
                break;
            case VehicleDirection12.DownLeftLeft:
                x_correction += half * (1 - currentrailblockprogress);
                y_correction += 1 - half * currentrailblockprogress;
                break;
            case VehicleDirection12.DownLeftDown:
                x_correction += half * currentrailblockprogress;
                y_correction += half + half * currentrailblockprogress;
                break;
            case VehicleDirection12.DownRightRight:
                x_correction += half + half * currentrailblockprogress;
                y_correction += 1 - half * currentrailblockprogress;
                break;
            case VehicleDirection12.DownRightDown:
                x_correction += 1 - half * currentrailblockprogress;
                y_correction += half + half * currentrailblockprogress;
                break;
        }
        //+1 because player can't be inside rail block (picking wouldn't work)
        return Vector3Ref.Create(aX + x_correction, aZ + railheight + 1 + z_correction, aY + y_correction);
    }

    internal void Reverse()
    {
        currentdirection = DirectionUtils.Reverse(currentdirection);
        currentrailblockprogress = 1 - currentrailblockprogress;
        lastdirection = currentdirection;
        //currentvehiclespeed = 0;
    }

    internal void ExitVehicle(Game game)
    {
        game.SetCharacterEyesHeight(originalmodelheight);
        railriding = false;
        game.controls.SetFreemove(FreemoveLevelEnum.None);
        game.enable_move = true;
    }

    internal float currentvehiclespeed;
    internal int currentrailblockX;
    internal int currentrailblockY;
    internal int currentrailblockZ;
    internal float currentrailblockprogress;
    internal VehicleDirection12 currentdirection;
    internal VehicleDirection12 lastdirection;
    internal float railheight;

    internal bool railriding;
    int lastrailsoundtimeMilliseconds;
    int lastrailsound;
    internal void RailSound(Game game)
    {
        float railsoundpersecond = currentvehiclespeed;
        if (railsoundpersecond > 10)
        {
            railsoundpersecond = 10;
        }
        game.AudioPlayLoop("railnoise.wav", railriding && railsoundpersecond > (one * 1 / 10), false);
        if (!railriding)
        {
            return;
        }
        if ((game.platform.TimeMillisecondsFromStart() - lastrailsoundtimeMilliseconds) > 1000 / railsoundpersecond)
        {
            game.AudioPlay(game.platform.StringFormat("rail{0}.wav", game.platform.IntToString(lastrailsound + 1)));
            lastrailsoundtimeMilliseconds = game.platform.TimeMillisecondsFromStart();
            lastrailsound++;
            if (lastrailsound >= 4)
            {
                lastrailsound = 0;
            }
        }
    }
    internal float originalmodelheight;

    internal RailMapUtil d_RailMapUtil;
    internal int GetUpDownMove(Game game, int railblockX, int railblockY, int railblockZ, TileEnterDirection dir)
    {
        if (!game.map.IsValidPos(railblockX, railblockY, railblockZ))
        {
            return UpDown.None;
        }
        //going up
        RailSlope slope = d_RailMapUtil.GetRailSlope(railblockX, railblockY, railblockZ);
        if (slope == RailSlope.TwoDownRaised && dir == TileEnterDirection.Up)
        {
            return UpDown.Up;
        }
        if (slope == RailSlope.TwoUpRaised && dir == TileEnterDirection.Down)
        {
            return UpDown.Up;
        }
        if (slope == RailSlope.TwoLeftRaised && dir == TileEnterDirection.Right)
        {
            return UpDown.Up;
        }
        if (slope == RailSlope.TwoRightRaised && dir == TileEnterDirection.Left)
        {
            return UpDown.Up;
        }
        //going down
        if (slope == RailSlope.TwoDownRaised && dir == TileEnterDirection.Down)
        {
            return UpDown.Down;
        }
        if (slope == RailSlope.TwoUpRaised && dir == TileEnterDirection.Up)
        {
            return UpDown.Down;
        }
        if (slope == RailSlope.TwoLeftRaised && dir == TileEnterDirection.Left)
        {
            return UpDown.Down;
        }
        if (slope == RailSlope.TwoRightRaised && dir == TileEnterDirection.Right)
        {
            return UpDown.Down;
        }
        return UpDown.None;
    }

    public static Vector3IntRef NextTile(VehicleDirection12 direction, int currentTileX, int currentTileY, int currentTileZ)
    {
        return NextTile_(DirectionUtils.ResultExit(direction), currentTileX, currentTileY, currentTileZ);
    }

    public static Vector3IntRef NextTile_(TileExitDirection direction, int currentTileX, int currentTileY, int currentTileZ)
    {
        switch (direction)
        {
            case TileExitDirection.Left:
                return Vector3IntRef.Create(currentTileX - 1, currentTileY, currentTileZ);
            case TileExitDirection.Right:
                return Vector3IntRef.Create(currentTileX + 1, currentTileY, currentTileZ);
            case TileExitDirection.Up:
                return Vector3IntRef.Create(currentTileX, currentTileY - 1, currentTileZ);
            case TileExitDirection.Down:
                return Vector3IntRef.Create(currentTileX, currentTileY + 1, currentTileZ);
            default:
                return null;
        }
    }

    internal int PossibleRails(Game game, TileEnterData enter)
    {
        int possible_railsVehicleDirection12Flags = 0;
        if (game.map.IsValidPos(enter.BlockPositionX, enter.BlockPositionY, enter.BlockPositionZ))
        {
            int newpositionrail = game.d_Data.Rail()[
                game.map.GetBlock(enter.BlockPositionX, enter.BlockPositionY, enter.BlockPositionZ)];
            VehicleDirection12[] all_possible_rails = new VehicleDirection12[3];
            int all_possible_railsCount = 0;
            VehicleDirection12[] possibleRails3 = DirectionUtils.PossibleNewRails3(enter.EnterDirection);
            for (int i = 0; i < 3; i++)
            {
                VehicleDirection12 z = possibleRails3[i];
                if ((newpositionrail & DirectionUtils.ToRailDirectionFlags(DirectionUtils.ToRailDirection(z)))
                    != 0)
                {
                    all_possible_rails[all_possible_railsCount++] = z;
                }
            }
            possible_railsVehicleDirection12Flags = DirectionUtils.ToVehicleDirection12Flags_(all_possible_rails, all_possible_railsCount);
        }
        return possible_railsVehicleDirection12Flags;
    }
}
