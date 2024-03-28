using UnityEngine.UI;

namespace Assets.Scripts.UI.Controls
{
    public class ProgressBarImageFill : BasicProgressBar
    {
        public Image fillMask;

        protected override void OnAwake()
        {
            fillMask.fillMethod = Image.FillMethod.Horizontal;
            base.OnAwake();
        }

        public override void OnProgressUpdate(float f)
        {
            fillMask.fillAmount = f;
        }
    }
}