using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using RSG;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.UI.WindowsManagerSystem
{
    public class WindowsManager : MonoBehaviour
    {
        #region UNITY_FIELDS
        [SerializeField] private Transform WindowsHolder;
        private CanvasGroup canvasGroup;
        #endregion

        public ReactiveProperty<Window> CurrentWindow = new ReactiveProperty<Window>(null);
        private Stack<Window> WindowsStack { get; }= new Stack<Window>(8);
        private WindowsQueueManager QueueManager { get; } = new WindowsQueueManager();

        private readonly List<IWindowProvider> windowProviders = new List<IWindowProvider> {new ResourceWindowProvider("UI/Windows")};
        #region events
        public event Action<Window> WindowOpened;
        public event Action AllClosed;

        #endregion

        #region UNITY_ACTIONS
        protected void Awake()
        {
            DontDestroyOnLoad(gameObject);
            canvasGroup = WindowsHolder.GetComponent<CanvasGroup>();

            CurrentWindow.Subscribe(x =>
            {
                if (x == null && QueueManager.IsQueueEmpty)
                    AllClosed?.Invoke();

            }).AddTo(gameObject);
        }
        #endregion

        public T OpenWindow<T>(Action<T> action = null) where T : Window
        {
            var window = GetWindowPrefab<T>();
            if (window.IsMinimizeOthers)
                MinimizeTopWindow();

            return OpenWindowInner<T>(window, action);
        }

        public void MinimizeTopWindow()
        {
            if (!WindowsStack.TryPeek(out var wnd))
                return;

            wnd.Minimize();
        }

        protected void OnTopWindowClose(Window closed)
        {
            ClearEmptyClosedWindows();
            var hasTopWindow = WindowsStack.TryPeek(out var topWindow);
            // Сначала 

            if (hasTopWindow)
            {
                var (wnd, action) = QueueManager.DequeueForWindow(topWindow);
                if (wnd != null)
                {
                    OpenWindowInner(wnd, action);
                }
                else
                {
                    wnd = WindowsStack.Peek();
                    wnd.Maximize();
                    CurrentWindow.Value = wnd;
                }
            }
            else
            {
                var (wnd, action) = QueueManager.DequeueForEmpty();
                if (wnd == null)
                    CurrentWindow.Value = null;
                else OpenWindowInner(wnd, action);
            }
        }

        protected void ClearEmptyClosedWindows()
        {
            while (WindowsStack.TryPeek(out var w))
            {
                if (!w || w.IsClosing)
                    WindowsStack.Pop();
                else 
                    break;
            }
        }

        protected T GetWindowPrefab<T>() where T : Window
        {
            T window = null;

            foreach (var provider in windowProviders)
            {
                window = provider.GetWindow<T>();
                if (window != null) break;
            }

            if (!window)
            {
                Debug.LogError($"Window '{typeof(T).Name}' not found");
                return null;
            }

            return window;
        }

        public void OnWindowClose(Window w)
        {
            if (CurrentWindow.Value == w)
                OnTopWindowClose(w);
        }


        protected T OpenWindowInner<T>(T windowPrefab = null, Action<T> action = null) where T : Window
        {
            if (windowPrefab == null)
                windowPrefab = GetWindowPrefab<T>(); 
            var wnd = Instantiate(windowPrefab, WindowsHolder);

            WindowsStack.Push(wnd);
            wnd.SetManager(this);

            CurrentWindow.Value = wnd;
            action?.Invoke(wnd);

            WindowOpened?.Invoke(wnd);

            return wnd;
        }

        public void CloseAllWindows(float? timeScale = null)
        {
            IDisposable windowCloseSub = null;

            //windowCloseSub = Observable.IntervalFrame(5).Subscribe(_ => CloseTopWindow(CurrentWindow.Value));
            windowCloseSub = CurrentWindow.Subscribe(CloseTopWindow).AddTo(Game.Instance.gameObject);

            void CloseTopWindow(Window w)
            {
                CheckStopCondition();

                if (!w)
                    return;

                if (timeScale != null)
                    w.SetAnimationSpeedup(timeScale.Value);

                // Этот анонимный метод вызывается единожды за кадр, даже если открыть новое окно, поэтому каждое окно закрываем в следущем кадре
                Observable.NextFrame().Subscribe(_ => w.CloseWhenAble()).AddTo(w); 
                w.OnDestroyAsObservable().Subscribe(_ => CheckStopCondition()).AddTo(w);
            }

            void CheckStopCondition()
            {
                if (CurrentWindow.Value != null && CurrentWindow.Value.IsClosing == false)
                    return;

                if (QueueManager.IsQueueEmpty == false)
                    return;

                if (WindowsStack.Count > 1)
                    return;

                if (WindowsStack.Count == 1 && WindowsStack.Peek().IsClosing == false)
                    return;

                windowCloseSub?.Dispose();
            }
        }

        private static WindowsManager _instance;
        public static WindowsManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<WindowsManager>(true);

                return _instance;
            }
        }
    }
}