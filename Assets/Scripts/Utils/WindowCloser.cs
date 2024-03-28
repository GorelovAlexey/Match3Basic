using System;
using Assets.Scripts.UI.WindowsManagerSystem;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(Image))]
    public class WindowCloser : MonoBehaviour
    {
        private IDisposable disposable;
        private Window window;

        private Window WND
        {
            get
            {
                if (!window)
                    window = gameObject.GetComponentInParent<Window>();

                return window;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            var image = GetComponent<Image>();

            image.OnPointerClickAsObservable().Subscribe(x =>
            {
                if (WND)
                    WND.Close();

            }).AddTo(gameObject);
        }
    }
}
