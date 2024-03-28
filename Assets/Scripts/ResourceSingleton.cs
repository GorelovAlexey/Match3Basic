using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class ResourceSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _instanceLock = new object();
        private static bool _quitting = false;

        public static T Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null && !_quitting)
                    {

                        _instance = GameObject.FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            var name = typeof(T).ToString().Split('.').Last();
                            var res = Resources.Load<T>(name);
                            _instance = Instantiate(res);

                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null) _instance = gameObject.GetComponent<T>();
            else if (_instance.GetInstanceID() != GetInstanceID())
            {
                Destroy(gameObject);
                throw new System.Exception(string.Format("Instance of {0} already exists, removing {1}", GetType().FullName, ToString()));
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _quitting = true;
        }
    }
}