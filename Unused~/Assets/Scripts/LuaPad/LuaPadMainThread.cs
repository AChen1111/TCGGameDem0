using System;
using System.Collections.Generic;

public static class LuaPadMainThread
{
    static readonly Queue<Action> s_q = new Queue<Action>();

    public static void Enqueue(Action action)
    {
        lock (s_q)
        {
            s_q.Enqueue(action);
        }
    }

    public static void Pump()
    {
        while (true)
        {
            Action action;
            lock (s_q)
            {
                if (s_q.Count == 0)
                {
                    return;
                }
                action = s_q.Dequeue();
            }
            action();
        }
    }
}
