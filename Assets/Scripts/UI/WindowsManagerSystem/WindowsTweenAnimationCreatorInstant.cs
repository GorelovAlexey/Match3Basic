using DG.Tweening;
using JetBrains.Annotations;

namespace Assets.Scripts.UI.WindowsManagerSystem
{
    public class WindowsTweenAnimationCreatorInstant<T> : WindowsTweenAnimationCreator<T> where T : Window
    {
        public WindowsTweenAnimationCreatorInstant([NotNull] T window) : base(window)
        {
        }

        Tween GetEmpty => DOVirtual.DelayedCall(0.001f, () => { });

        public override Tween CreateAppearAnimation()
        {
            return GetEmpty;
        }
        public override Tween CreateCloseAnimation()
        {
            return GetEmpty;
        }
        public override Tween CreateMinimizeAnimation()
        {
            return GetEmpty;
        }
        public override Tween CreateMaximizeAnimation()
        {
            return GetEmpty;
        }
    }
}