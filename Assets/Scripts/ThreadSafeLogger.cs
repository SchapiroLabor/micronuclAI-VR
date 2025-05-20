using System;
using System.Collections.Concurrent;
using UnityEngine;

public class ThreadSafeLogger : MonoBehaviour
{
    private static readonly ConcurrentQueue<Action> logQueue = new();

    public static void Log(string message)
    {
        logQueue.Enqueue(() => Debug.Log(message));
    }

    void Update()
    {
        while (logQueue.TryDequeue(out var logAction))
        {
            logAction.Invoke();
        }
    }
}
