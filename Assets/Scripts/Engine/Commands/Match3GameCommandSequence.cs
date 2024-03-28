using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Visualizer.VisualCommands;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Engine.Commands
{
    public class Match3GameCommandSequence : Match3GameCommand, ISavableCommand
    {
        public new const string TYPE_NAME = "sequence";

        private List<Match3GameCommand> commands = new List<Match3GameCommand>();
        public Match3GameCommandSequence(params Match3GameCommand[] commands)
        {
            this.commands.AddRange(commands);
        }

        public void Add(params Match3GameCommand[] c)
        {
            commands.AddRange(c);
        }

        public override void Apply(Match3Game game)
        {
            foreach (var cmd in commands)
            {
                cmd.Apply(game);
            }
        }

        public override FieldVisualCommand GetVisuals(Match3Game game)
        {
            var visual = new FieldVisualCommandSequence();
            foreach (var cmd in commands)
            {
                visual.Add(cmd.GetVisuals(game));
            }
            return visual;
        }

        public string GetTypeName => TYPE_NAME;

        public JToken Serialize()
        {
            var tokens = commands.OfType<ISavableCommand>().Select(x => x.Serialize());
            return JArray.FromObject(tokens.ToArray());
        }

        public void LoadData(JToken obj)
        {
            var arr = (JArray) obj;
            var tokens = arr.ToObject<JObject[]>();

            commands.Clear();
            foreach (var t in tokens)
                commands.Add(Deserialize(t));
        }

    }
}