using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shared
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> MainThreadActions = new Queue<Action>();

        public static void RunOnMainThread(Action action)
        {
            lock (MainThreadActions)
            {
                MainThreadActions.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (MainThreadActions)
            {
                while (MainThreadActions.Count > 0)
                {
                    var action = MainThreadActions.Dequeue();
                    action?.Invoke();
                }
            }
        }
    }
}