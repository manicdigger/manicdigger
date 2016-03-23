public class ModCameraKeys : ClientMod
{
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        float one = 1;
        float dt = args.GetDt();

        if (game.guistate == GuiState.MapLoading) { return; }

        bool angleup = false;
        bool angledown = false;
        float overheadcameraanglemovearea = one * 5 / 100;
        float overheadcameraspeed = 3;
        game.controls.wantsjump = game.guistate == GuiState.Normal && game.GuiTyping == TypingState.None && game.keyboardState[game.GetKey(GlKeys.Space)];
        game.controls.wantsjumphalf = false;
        game.controls.shiftkeydown = game.guistate == GuiState.Normal && game.GuiTyping == TypingState.None && game.keyboardState[game.GetKey(GlKeys.ShiftLeft)];
        game.controls.movedx = 0;
        game.controls.movedy = 0;
        game.controls.moveup = false;
        game.controls.movedown = false;
        if (game.guistate == GuiState.Normal)
        {
            if (game.GuiTyping == TypingState.None)
            {
                if (game.reachedwall_1blockhigh && (game.AutoJumpEnabled || (!game.platform.IsMousePointerLocked())))
                {
                    game.controls.wantsjump = true;
                }
                if (game.reachedHalfBlock)
                {
                    game.controls.wantsjumphalf = true;
                }
                if (game.overheadcamera)
                {
                    CameraMove m = new CameraMove();
                    if (game.keyboardState[game.GetKey(GlKeys.A)]) { game.overheadcameraK.TurnRight(dt * overheadcameraspeed); }
                    if (game.keyboardState[game.GetKey(GlKeys.D)]) { game.overheadcameraK.TurnLeft(dt * overheadcameraspeed); }
                    if (game.keyboardState[game.GetKey(GlKeys.W)]) { angleup = true; }
                    if (game.keyboardState[game.GetKey(GlKeys.S)]) { angledown = true; }
                    game.overheadcameraK.Center.X = game.player.position.x;
                    game.overheadcameraK.Center.Y = game.player.position.y;
                    game.overheadcameraK.Center.Z = game.player.position.z;
                    m.Distance = game.overheadcameradistance;
                    m.AngleUp = angleup;
                    m.AngleDown = angledown;
                    game.overheadcameraK.Move(m, dt);
                    float toDest = game.Dist(game.player.position.x, game.player.position.y, game.player.position.z,
                    game.playerdestination.X + one / 2, game.playerdestination.Y - one / 2, game.playerdestination.Z + one / 2);
                    if (toDest >= 1)
                    {
                        game.controls.movedy += 1;
                        if (game.reachedwall)
                        {
                            game.controls.wantsjump = true;
                        }
                        //player orientation
                        float qX = game.playerdestination.X - game.player.position.x;
                        float qY = game.playerdestination.Y - game.player.position.y;
                        float qZ = game.playerdestination.Z - game.player.position.z;
                        float angle = game.VectorAngleGet(qX, qY, qZ);
                        game.player.position.roty = Game.GetPi() / 2 + angle;
                        game.player.position.rotx = Game.GetPi();
                    }
                }
                else if (game.enable_move)
                {
                    if (game.keyboardState[game.GetKey(GlKeys.W)]) { game.controls.movedy += 1; }
                    if (game.keyboardState[game.GetKey(GlKeys.S)]) { game.controls.movedy += -1; }
                    if (game.keyboardState[game.GetKey(GlKeys.A)]) { game.controls.movedx += -1; game.localplayeranimationhint.leanleft = true; game.localstance = 1; }
                    else { game.localplayeranimationhint.leanleft = false; }
                    if (game.keyboardState[game.GetKey(GlKeys.D)]) { game.controls.movedx += 1; game.localplayeranimationhint.leanright = true; game.localstance = 2; }
                    else { game.localplayeranimationhint.leanright = false; }
                    if (!game.localplayeranimationhint.leanleft && !game.localplayeranimationhint.leanright) { game.localstance = 0; }

                    game.controls.movedx += game.touchMoveDx;
                    game.controls.movedy += game.touchMoveDy;
                }
            }
            if ((game.controls.GetFreemove() != FreemoveLevelEnum.None) || game.SwimmingEyes())
            {
                if (game.GuiTyping == TypingState.None && game.keyboardState[game.GetKey(GlKeys.Space)])
                {
                    game.controls.moveup = true;
                }
                if (game.GuiTyping == TypingState.None && game.keyboardState[game.GetKey(GlKeys.ControlLeft)])
                {
                    game.controls.movedown = true;
                }
            }
        }
    }
}
