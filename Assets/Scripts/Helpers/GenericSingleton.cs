using UnityEngine;

namespace Helpers
{
    public class GenericSingleton<T> : MonoBehaviour where T : Component
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        var gameObject = new GameObject("GenericSingleton");
                        instance = gameObject.AddComponent<T>();
                        DontDestroyOnLoad(gameObject);
                    }
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (instance != this)
                    Destroy(gameObject);
            }
        }
    }
}