using Assets.Scripts.Engine;
using Assets.Scripts.UI;
using Assets.Scripts.Visualizer.VisualCommands;
using RSG;
using UnityEngine;

namespace Assets.Scripts.Visualizer
{
    public interface IFieldVisualizer
    {
        public bool IsInputBlocked { get; set; }
        public Vector3 GetCellPosition(float x, float y);
        public void SetField(Match3Field field);
        public IPromise RemoveTokenAnimated(int x, int y, float animSpeed = 1f);
        public FieldTokenVisual SpawnToken(Match3Token tokenType, float xPos, float yPos);
        public Transform SpawnResourceToken(Match3Token t, int x, int y);
        public Transform GetTokensLayer();
        public IPromise MoveVisualToken(FieldTokenVisual token, int x, int y, float totalTime);
        public FieldTokenVisual GetToken(int x, int y);
        public FieldTokenVisual SetToken(FieldTokenVisual token, int x, int y);
        public PlayersPanel PlayerPanel { get; }
        public CommandsRegistry CommandsRegistry { get; }
    }
}