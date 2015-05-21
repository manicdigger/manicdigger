public class ModBlockDamageToPlayer : ClientMod
{
    public ModBlockDamageToPlayer()
    {
        one = 1;
        BlockDamageToPlayerTimer = TimerCi.Create(BlockDamageToPlayerEvery, BlockDamageToPlayerEvery * 2);
    }
    public override void OnNewFrameFixed(Game game, NewFrameEventArgs args)
    {
        if (game.guistate == GuiState.MapLoading)
        {
            return;
        }
        if (game.FollowId() == null)
        {
            UpdateBlockDamageToPlayer(game, args.GetDt());
        }
    }
    public const int BlockDamageToPlayerEvery = 1;
    TimerCi BlockDamageToPlayerTimer;

    float one;

    //TODO server side?
    internal void UpdateBlockDamageToPlayer(Game game, float dt)
    {
        float pX = game.player.position.x;
        float pY = game.player.position.y;
        float pZ = game.player.position.z;
        pY += game.entities[game.LocalPlayerId].drawModel.eyeHeight;
        int block1 = 0;
        int block2 = 0;
        if (game.map.IsValidPos(game.MathFloor(pX), game.MathFloor(pZ), game.MathFloor(pY)))
        {
            block1 = game.map.GetBlock(game.platform.FloatToInt(pX), game.platform.FloatToInt(pZ), game.platform.FloatToInt(pY));
        }
        if (game.map.IsValidPos(game.MathFloor(pX), game.MathFloor(pZ), game.MathFloor(pY) - 1))
        {
            block2 = game.map.GetBlock(game.platform.FloatToInt(pX), game.platform.FloatToInt(pZ), game.platform.FloatToInt(pY) - 1);
        }

        int damage = game.d_Data.DamageToPlayer()[block1] + game.d_Data.DamageToPlayer()[block2];
        if (damage > 0)
        {
            int hurtingBlock = block1;	//Use block at eyeheight as source block
            if (hurtingBlock == 0 || game.d_Data.DamageToPlayer()[hurtingBlock] == 0) { hurtingBlock = block2; }	//Fallback to block at feet if eyeheight block is air or doesn't deal damage
            int times = BlockDamageToPlayerTimer.Update(dt);
            for (int i = 0; i < times; i++)
            {
                game.ApplyDamageToPlayer(damage, Packet_DeathReasonEnum.BlockDamage, hurtingBlock);
            }
        }

        //Player drowning
        int deltaTime = game.platform.FloatToInt(one * (game.platform.TimeMillisecondsFromStart() - game.lastOxygenTickMilliseconds)); //Time in milliseconds
        if (deltaTime >= 1000)
        {
            if (game.WaterSwimmingEyes())
            {
                game.PlayerStats.CurrentOxygen -= 1;
                if (game.PlayerStats.CurrentOxygen <= 0)
                {
                    game.PlayerStats.CurrentOxygen = 0;
                    int dmg = game.platform.FloatToInt(one * game.PlayerStats.MaxHealth / 10);
                    if (dmg < 1)
                    {
                        dmg = 1;
                    }
                    game.ApplyDamageToPlayer(dmg, Packet_DeathReasonEnum.Drowning, block1);
                }
            }
            else
            {
                game.PlayerStats.CurrentOxygen = game.PlayerStats.MaxOxygen;
            }
            if (GameVersionHelper.ServerVersionAtLeast(game.platform, game.serverGameVersion, 2014, 3, 31))
            {
                game.SendPacketClient(ClientPackets.Oxygen(game.PlayerStats.CurrentOxygen));
            }
            game.lastOxygenTickMilliseconds = game.platform.TimeMillisecondsFromStart();
        }
    }
}
