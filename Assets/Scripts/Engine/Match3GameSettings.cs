using System;

namespace Assets.Scripts.Engine
{
    public struct Match3GameSettings
    {
        public Direction tokensSpawnDirection;
        public int width;
        public int height;
        public int seed;

        public bool freeTurnsEnabled;
        public int turnsPerPlayer;
        public int tokensForFreeTurn;

        public static Match3GameSettings CreateDefault()
        {
            return new Match3GameSettings
            {
                tokensSpawnDirection = Direction.Top,
                width = 8,
                height = 8,
                seed = new Random().Next(),

                freeTurnsEnabled = true,
                turnsPerPlayer = 1,
                tokensForFreeTurn = 4,
            };
        }
    }

}