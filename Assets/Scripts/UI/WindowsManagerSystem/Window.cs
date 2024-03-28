using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.WindowsManagerSystem
{
    public abstract class Window : MonoBehaviour
    {
        #region Unity

        [SerializeField] public CanvasGroup canvasGroup;
        [SerializeField] public Transform content;

        #endregion

        public bool IsClosing { get; private set; } = false;
        public ReactiveProperty<float> Alpha { get; } = new ReactiveProperty<float>(1f);
        public ReactiveProperty<bool> IsMinimized { get; } = new ReactiveProperty<bool>(false);
        public ReactiveProperty<bool> CanClose { get; } = new ReactiveProperty<bool>(true);
        public WindowsManagerSystem.WindowsManager Manager { get; private set; }

        private WindowsTweenAnimationCreator<Window> _animationCreator;
        protected virtual WindowsTweenAnimationCreator<Window> AnimationCreator => _animationCreator ??= new WindowsTweenAnimationCreator<Window>(this);
        private Tween animationTween;
        public virtual WindowProperties Properties { get; } = WindowProperties.Default;
        public virtual bool IsMinimizeOthers => false;


        #region UnityActions
        public void Awake()
        {
            Alpha.Subscribe(x => canvasGroup.alpha = x).AddTo(gameObject);
            IsMinimized.Subscribe(minimized => SetInteractive(!minimized)).AddTo(gameObject);
            OnAwake();
            Appear();
        }
        #endregion

        public float AnimationTimeScale { get; protected set; } = 1f;
        public void SetAnimationSpeedup(float timeScale)
        {
            animationTween.Pause();
            animationTween.timeScale = timeScale;
            animationTween.Play();

            AnimationTimeScale = timeScale;
        }


        #region WindowsManager
        public void SetManager(WindowsManager m)
        {
            Manager = m;
        }
        #endregion

        public void Minimize()
        {
            if (IsClosing)
                return;

            if (!Properties.CanMinimize)
                return;

            OnMinimizeStart();
            StartAnimation(AnimationCreator.CreateMinimizeAnimation(), OnMinimizeComplete);
        }

        private void OnMinimizeComplete()
        {
            IsMinimized.Value = true;
            OnMinimizeEnd();
        }

        public void Maximize()
        {
            if (IsClosing)
                return;

            if (!Properties.CanMinimize)
                return;

            OnMaximizeStart();
            StartAnimation(AnimationCreator.CreateMaximizeAnimation(), OnMaximizeComplete);
        }

        private void OnMaximizeComplete()
        {
            IsMinimized.Value = false;
            OnMaximizeEnd();
        }

        public void CloseHotKey()
        {
            if (!Properties.CanCloseWithHotKey)
                return;
            Close();
        }

        public void Close()
        {
            if (IsClosing)
                return;

            if (CanClose.Value == false)
                return;

            IsClosing = true;
            OnClose();
            StartAnimation(AnimationCreator.CreateCloseAnimation(), CloseFinal);
        }

        public void CloseWhenAble()
        {
            if (CanClose.Value)
                Close();
            else
            {
                CanClose.Subscribe(x => Close()).AddTo(gameObject);
            }
        }

        private void CloseFinal()
        {
            animationTween?.Kill();

            if (gameObject)
                Destroy(gameObject);

            Manager.OnWindowClose(this);
        }

        private void Appear()
        {
            OnAppear();
            SetInteractive(false);
            StartAnimation(AnimationCreator.CreateAppearAnimation(), AppearFinal);
        }

        private void AppearFinal()
        {
            SetInteractive(true);
        }

        private void StartAnimation(Tween anim, TweenCallback onComplete = null, float? timeScale = null)
        {
            animationTween?.Kill();
            anim.timeScale = AnimationTimeScale;
            if (timeScale.HasValue)
                anim.timeScale = timeScale.Value;
            animationTween = anim.SetLink(gameObject).Play();
            if (onComplete != null)
                anim.onComplete = onComplete;
        }

        protected void SetInteractive(bool state)
        {
            canvasGroup.interactable = state;
        }

        protected virtual void OnAwake() { }

        protected virtual void OnClose() {}
        protected virtual void OnAppear() {}

        protected virtual void OnMinimizeStart() { }
        protected virtual void OnMinimizeEnd() { }
        protected virtual void OnMaximizeStart() { }
        protected virtual void OnMaximizeEnd() { }
    }
}