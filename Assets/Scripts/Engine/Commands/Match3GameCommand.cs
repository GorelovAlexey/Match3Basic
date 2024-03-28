using System;
using Assets.Scripts.Visualizer.VisualCommands;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Engine.Commands
{
    public abstract class Match3GameCommand
    {
        public const string TYPE_TOKEN_NAME = "type";
        public const string TYPE_NAME = "basic";

        public abstract void Apply(Match3Game game);
        public abstract FieldVisualCommand GetVisuals(Match3Game game);

        public static Match3GameCommand Deserialize(JObject token)
        {
            Match3GameCommand cmd;
            switch (token.Value<string>(TYPE_TOKEN_NAME))
            {
                case Match3CommandMoveSwap.TYPE_NAME:
                    cmd = new Match3CommandMoveSwap(0, 0, 0, 0);
                    break;

                case Match3CommandShuffle.TYPE_NAME:
                    cmd = new Match3CommandShuffle(null, 0, 0);
                    break;

                default:
                    return null;
            }

            ((ISavableCommand)cmd).LoadData(token);
            return cmd;
        }
    }
}