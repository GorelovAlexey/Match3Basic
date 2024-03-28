using System;
using System.Linq;
using Assets.Scripts.Engine.Player;
using Assets.Scripts.Visualizer.VisualCommands;

namespace Assets.Scripts.Engine.Commands
{
    public class Match3CommandClearField : Match3GameCommand
    {
        private (Match3Token, float)[,] valueField;
        private Match3Player player;
        public override void Apply(Match3Game game)
        {
            var tokens = game.Field.ClearAfterMove(out valueField);
            var tokensContainer = new Match3CollectedTokensContainer();
            tokensContainer.AddValue(tokens);

            player = game.PlayersManager.CurrentPlayer;

            player.Collect(tokensContainer.CollectedCount.Select(x => (x.Key, x.Value)), true, false);
            game.PlayersManager.InspectReward(tokensContainer.MatchesCount);
        }

        public override FieldVisualCommand GetVisuals(Match3Game game)
        {
            var hideMap = CreateHideMask(valueField);

            var hide = new FieldVisualHide(hideMap);
            var pause = new FieldVisualPause(.25f);
            var showFlying = new FieldVisualSpawnResource(player, valueField);

            var visual = new FieldVisualCommandAll(hide, new FieldVisualCommandSequence(pause, showFlying));

            return visual;
        }

        private bool[,] CreateHideMask((Match3Token, float)[,] valueField)
        {
            var result = new bool[valueField.GetLength(0), valueField.GetLength(1)];
            for (var x = 0; x < valueField.GetLength(0); x++)
            {
                for (var y = 0; y < valueField.GetLength(1); y++)
                {
                    if (valueField[x, y].Item1 != Match3Token.Empty && valueField[x, y].Item2 > 0f)
                        result[x, y] = true;
                }
            }

            return result;
        }
    }
}