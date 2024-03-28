using System;
using System.Threading;
using Assets.Scripts.Visualizer;
using UnityEngine;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Подстраивает игровое поле чтобы оно умещалось в ректе на котором висит данный скрипт
    /// </summary>
    public class FieldArea : MonoBehaviour
    {
        private FieldVisualUI gameField;
        private RectTransform rect;
        private RectTransform boardRect;

        private Vector2 size;

        public void Awake()
        {
            gameField = GetComponentInChildren<FieldVisualUI>();
            rect = GetComponent<RectTransform>();
            boardRect = gameField.GetComponent<RectTransform>();

            size = GetSize();
            ForceUpdate();
        }

        private Vector2 GetSize() => rect.rect.size;

        public void Update()
        {
            var size2 = GetSize();
            if (size2.Equals(size))
                return;

            ForceUpdate();
        }

        private void ForceUpdate()
        {
            size = GetSize();
            ReCalculateScales();
        }

        // изменение размера аналогично "Contain" в CSS
        public void ReCalculateScales()
        {
            var boardSize = boardRect.rect.size;

            var needWidthScale = size.x / boardSize.x;
            var needHeightScale = size.y / boardSize.y;
            var min = MathF.Min(needHeightScale, needWidthScale);

            boardRect.localScale = new Vector3(min, min, 1);
        }
    }
}