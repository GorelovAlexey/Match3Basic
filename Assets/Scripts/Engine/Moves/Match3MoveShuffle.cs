using Assets.Scripts.UI.Utils;
using Assets.Scripts.Visualizer;
using Assets.Scripts.Visualizer.VisualCommands;
using Newtonsoft.Json;

namespace Assets.Scripts.Engine.Moves
{
    public class Match3MoveShuffle : Match3Move
    {
        [JsonProperty]
        private Match3Token[,] generated;
        private Match3Token[,] lastField;
        private bool setLastField = false;

        public Match3MoveShuffle(Match3FieldGenerator generator, int width, int height)
        {
            // для десериализации
            if (generator != null)
                generated = generator.GetField(width, height);
        }

        public override Match3Token[,] ApplyMove(Match3Token[,] field, out FieldVisualCommand command)
        {
            lastField = field.Clone() as Match3Token[,];
            setLastField = true;
            var width = field.GetLength(0);
            var height = field.GetLength(1);

            var mask = new bool[width, height].SetEach((_, __, ___) => true);
            //var moveMask = new (int, int)[width, height].SetEach((_, __, ___) => (0, -height));

            command = new FieldVisualHide(mask);
            var spawnTop = new FieldVisualSpawn(generated, Direction.Top); // TODO CHANGE

            command.SetNext(new FieldVisualPause(.25f), spawnTop);
            return generated;
        }

        public override Match3Token[,] RevertMove(Match3Token[,] field, out FieldVisualCommand command)
        {
            if (!setLastField)
            {
                command = new FieldVisualEmptyNode();
                return field;
            }

            command = new FieldVisualAppear(lastField);
            return lastField;
        }

        public override (bool valid, bool possible) IsValidAndPossible(Match3Matcher matcher, Match3Token[,] field)
        {
            return (true, true);
        }
    }
}