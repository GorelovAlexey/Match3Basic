using UnityEngine.UI;

namespace Assets.Scripts.UI.Controls
{
    public class ProgressBarSlider : BasicProgressBar
    {
        public Slider slider;
        protected override void OnAwake()
        {
            slider.interactable = false;
            base.OnAwake();
        }

        public override void OnProgressUpdate(float f)
        {
            slider.value = f;
        }
    }
}