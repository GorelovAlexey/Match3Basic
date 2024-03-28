namespace Assets.Scripts.UI.WindowsManagerSystem
{
    public struct WindowProperties
    {
        public bool CanCloseWithHotKey;
        public bool CanMinimize;

        public static WindowProperties Default =>
            new WindowProperties()
            {
                CanCloseWithHotKey = true,
                CanMinimize = true
            };
    }
}