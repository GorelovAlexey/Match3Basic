using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Utils
{
    /// <summary>
    /// Нужен для того чтобы можно было менять материал на отдельном обьекте в ЮИ.
    /// Проблема в том что в CanvasRenderer нет SetPropertyBlock который позволял бы
    /// эффективно менять параметры для материала у отдельных обьектов. Когда добавят
    /// (просят с 2016 года) тогда этот компонент станет не нужен. 
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UiMaterialCloner : MonoBehaviour
    {
        private Image img;
        private bool cloneDone = false;
        public void Init()
        {
            if (cloneDone) return;
            cloneDone = true;

            img = GetComponent<Image>();
            var original = img.material;
            img.material = Instantiate(img.material);
            img.material.CopyPropertiesFromMaterial(original);
        }

        public void Awake()
        {
            Init();
        }
    }
}