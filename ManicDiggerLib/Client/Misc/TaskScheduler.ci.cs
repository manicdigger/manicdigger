public class TaskScheduler_
{
    public TaskScheduler_()
    {
        mainTasks = QueueTask.Create(128);
        backgroundPerFrameTasks = ListTask.Create(128);
        commitTasks = QueueTask.Create(16 * 1024);
        tasks = QueueTask.Create(16 * 1024);
        newPerFrameTasks = QueueTask.Create(128);
    }

    public void Start(GamePlatform platform_)
    {
        platform = platform_;
        lockObject = platform.MonitorCreate();
    }

    GamePlatform platform;
    MonitorObject lockObject;

    public void QueueTaskReadOnlyMainThread(Task task)
    {
        platform.MonitorEnter(lockObject);
        mainTasks.Enqueue(task);
        platform.MonitorExit(lockObject);
    }

    QueueTask newPerFrameTasks;

    public void QueueTaskReadOnlyBackgroundPerFrame(Task task)
    {
        platform.MonitorEnter(lockObject);
        newPerFrameTasks.Enqueue(task);
        platform.MonitorExit(lockObject);
    }

    public void QueueTaskCommit(Task task)
    {
        platform.MonitorEnter(lockObject);
        commitTasks.Enqueue(task);
        platform.MonitorExit(lockObject);
    }

    QueueTask mainTasks;
    ListTask backgroundPerFrameTasks;
    QueueTask commitTasks;

    QueueTask tasks;

    public void Update(float dt)
    {
        Move(mainTasks, tasks);
        while (tasks.Count() > 0)
        {
            tasks.Dequeue().Run(dt);
        }

        if (platform.MultithreadingAvailable())
        {
            for (int i = backgroundPerFrameTasks.count - 1; i >= 0; i--)
            {
                if (backgroundPerFrameTasks.items[i].Done)
                {
                    backgroundPerFrameTasks.RemoveAt(i);
                }
            }
            if (backgroundPerFrameTasks.Count() == 0)
            {
                Move(commitTasks, tasks);
                while (tasks.Count() > 0)
                {
                    tasks.Dequeue().Run(dt);
                }

                Move(newPerFrameTasks, tasks);
                while (tasks.Count() > 0)
                {
                    Task task = tasks.Dequeue();
                    backgroundPerFrameTasks.Add(task);
                    task.Done = false;
                    platform.QueueUserWorkItem(TaskAction.Create(task));
                }
            }
        }
        else
        {
            for (int i = 0; i < backgroundPerFrameTasks.count; i++)
            {
                backgroundPerFrameTasks.items[i].Run(dt);
            }
            backgroundPerFrameTasks.Clear();

            Move(commitTasks, tasks);
            while (tasks.Count() > 0)
            {
                tasks.Dequeue().Run(dt);
            }

            Move(newPerFrameTasks, tasks);
            while (tasks.Count() > 0)
            {
                Task task = tasks.Dequeue();
                backgroundPerFrameTasks.Add(task);
                task.Done = false;
            }
        }
    }

    void Move(QueueTask from, QueueTask to)
    {
        platform.MonitorEnter(lockObject);
        int count = from.count_;
        for (int i = 0; i < count; i++)
        {
            Task task = from.Dequeue();
            to.Enqueue(task);
        }
        platform.MonitorExit(lockObject);
    }
}

public abstract class Action_
{
    public abstract void Run();
}

public class TaskAction : Action_
{
    public static TaskAction Create(Task task)
    {
        TaskAction action = new TaskAction();
        action.task = task;
        return action;
    }
    internal Task task;
    public override void Run()
    {
        task.Run(1);
        task.Done = true;
    }
}

public class Task
{
    internal Game game;
    public virtual void Run(float dt) { }
    internal bool Done;
}
