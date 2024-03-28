using Assets.Scripts;
using Assets.Scripts.UI;
using UnityEngine;

namespace Assets
{
    public class DataHolder : ResourceSingleton<DataHolder>
    {
        [SerializeField] private TokenIconsDatabase tokenIconDatabase;
        public static TokenIconsDatabase TokenIconDatabase => Instance.tokenIconDatabase;
    }
}
