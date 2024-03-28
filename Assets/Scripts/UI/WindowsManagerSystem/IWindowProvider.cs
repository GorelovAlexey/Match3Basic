using UnityEngine;

namespace Assets.Scripts.UI.WindowsManagerSystem
{
    interface IWindowProvider
    {
        T GetWindow<T>() where T : Window;
    }

    class ResourceWindowProvider : IWindowProvider
    {
        private string path;
        public ResourceWindowProvider(string folder)
        {
            path = folder;
        }

        public T GetWindow<T>() where T : Window
        {
            var type = typeof(T);
            return Resources.Load<T>($"{path}/{type.Name}");
        }
    }
}