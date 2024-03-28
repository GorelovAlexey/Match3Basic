using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;

namespace Assets.Scripts.Engine.Commands
{
    public class Match3ReplayTurn
    {
        private List<Match3GameCommand> commands= new List<Match3GameCommand>();
        private int _current = 0;

        public Match3GameCommand Current => _current >= commands.Count ? null : commands[_current];

        private Match3GameCommand Advance()
        {
            _current++;
            return Current;
        }

        public void Reset()
        {
            _current = 0;
        }

        public Match3GameCommand AdvanceAndLoadCommand<T>(T cmd) where T : Match3GameCommand
        {
            if (!(cmd is ISavableCommand savableInput))
                return cmd;

            for (var i = _current; i < commands.Count; i++)
            {
                var c = commands[i];
                if (cmd.GetType() != c.GetType())
                    continue;
                   // !(c is ISavableCommand savableFound)) continue;
                _current = i + 1;

                var savableFound = c as ISavableCommand;
                savableInput.LoadData(savableFound.Serialize());
                break;
            }

            return cmd;
        }

        public Match3CommandMove AdvanceTillMove()
        {
            var cmd = Current;
            while (cmd != null)
            {
                if (cmd is Match3CommandMove move)
                    return move;

                cmd = Advance();
            }

            return null;
        }

        public void AddSaveCommand(Match3GameCommand cmd)
        {
            if (cmd is ISavableCommand)
            {
                commands.Add(cmd);
            }
        }

        public JToken Serialize()
        {
            var tokens = commands.OfType<ISavableCommand>().Select(x => x.Serialize());
            return JArray.FromObject(tokens.ToArray());
        }

        public void LoadData(JToken obj)
        {
            var arr = (JArray)obj;
            var tokens = arr.ToObject<JObject[]>();

            commands.Clear();
            foreach (var t in tokens)
            {
                var cmd = Match3GameCommand.Deserialize(t);
                if (cmd == null)
                    continue;
                commands.Add(cmd);
            }
        }
    }
}