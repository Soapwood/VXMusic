using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace Plugins
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> Queue = new ConcurrentQueue<Action>();
        private static MainThreadDispatcher _instance;

        public static MainThreadDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("MainThreadDispatcher instance is not initialized yet.");
                }
                return _instance;
            }
        }

        public static void Initialize()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("MainThreadDispatcher");
                _instance = go.AddComponent<MainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
        }

        public void Enqueue(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            Queue.Enqueue(action);
        }

        void Update()
        {
            while (Queue.TryDequeue(out Action action))
            {
                action.Invoke();
            }
        }
    }
}
