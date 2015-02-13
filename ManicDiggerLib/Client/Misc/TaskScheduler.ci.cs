public class TaskScheduler
{
    public TaskScheduler()
    {
        actions = null;
    }

    BackgroundAction[] actions;

    public void Update(Game game, float dt)
    {
        if (actions == null)
        {
            actions = new BackgroundAction[game.clientmodsCount];
            for (int i = 0; i < game.clientmodsCount; i++)
            {
                actions[i] = new BackgroundAction();
            }
        }

        if (game.platform.MultithreadingAvailable())
        {
            for (int i = 0; i < game.clientmodsCount; i++)
            {
                game.clientmods[i].OnReadOnlyMainThread(game, dt);
            }

            bool allDone = true;
            for (int i = 0; i < game.clientmodsCount; i++)
            {
                if (actions[i] != null && actions[i].active && (!actions[i].finished))
                {
                    allDone = false;
                }
            }

            if (allDone)
            {
                for (int i = 0; i < game.clientmodsCount; i++)
                {
                    game.clientmods[i].OnReadWriteMainThread(game, dt);
                }
                for (int i = 0; i < game.commitActions.count; i++)
                {
                    game.commitActions.items[i].Run();
                }
                game.commitActions.Clear();
                for (int i = 0; i < game.clientmodsCount; i++)
                {
                    BackgroundAction a = actions[i];
                    a.game = game;
                    a.dt = dt;
                    a.i = i;
                    a.active = true;
                    a.finished = false;
                    game.platform.QueueUserWorkItem(a);
                }
            }
        }
        else
        {
            for (int i = 0; i < game.clientmodsCount; i++)
            {
                game.clientmods[i].OnReadOnlyMainThread(game, dt);
            }

            for (int i = 0; i < game.clientmodsCount; i++)
            {
                game.clientmods[i].OnReadOnlyBackgroundThread(game, dt);
            }

            for (int i = 0; i < game.clientmodsCount; i++)
            {
                game.clientmods[i].OnReadWriteMainThread(game, dt);
            }

            for (int i = 0; i < game.commitActions.count; i++)
            {
                game.commitActions.items[i].Run();
            }
            game.commitActions.Clear();
        }
    }
}

public class BackgroundAction : Action_
{
    public BackgroundAction()
    {
        game = null;
        i = -1;
        dt = 1;
        active = false;
        finished = false;
    }
    internal Game game;
    internal int i;
    internal float dt;
    internal bool active;
    internal bool finished;

    public override void Run()
    {
        game.clientmods[i].OnReadOnlyBackgroundThread(game, dt);
        finished = true;
    }
}
