using System;
using System.Collections;
using UnityEngine;

namespace Plugins
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static void ExecuteOnMainThread(Action action)
        {
            if (_instance != null)
            {
                _instance.StartCoroutine(_instance.ExecuteOnMainThreadCoroutine(action));
            }
        }

        private IEnumerator ExecuteOnMainThreadCoroutine(Action action)
        {
            yield return null; // Wait for one frame to ensure we're on the main thread
            action?.Invoke();
        }
    }
}
