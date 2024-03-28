using System;
using Assets.Scripts.Engine;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [CreateAssetMenu(fileName = "TokensIconsDatabase", menuName = "Scriptable/IconsDatabase")]
    public class TokenIconsDatabase : ScriptableObject
    {
        [SerializeField] private TokensIconsDictionary iconsForTokens;

        public Sprite GetSpriteForToken( Match3Token t)
        {
            if (iconsForTokens.ContainsKey(t))
                return iconsForTokens[t];

            return null;
        }
    }


    [Serializable]
    public class TokensIconsDictionary : SerializableDictionaryBase<Match3Token, Sprite> { }
}