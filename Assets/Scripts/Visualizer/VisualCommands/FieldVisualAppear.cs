using Assets.Scripts.Engine;
using RSG;
using UnityEngine;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualAppear : FieldVisualCommand
    {
        private Match3Token[,] field;

        public FieldVisualAppear(Match3Token[,] field)
        {
            this.field = field;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            for (var x = 0; x < field.GetLength(0); x++)
            for (var y = 0; y < field.GetLength(1); y++)
            {
                Object.Destroy(visualizer.GetToken(x, y).GameObject);
                var token = visualizer.SpawnToken(field[x, y], x, y);
                visualizer.SetToken(token, x, y);
            }
            ResolveFinishPromise();
        }

        public override void Break()
        {
            base.Break();
        }
    }
}