namespace Assets.Scripts.Engine
{
    public static class FieldBasicTransformations
    {
        public static Match3Token[,] Swap(this Match3Token[,] field, int x0, int y0, int x1, int y1)
        {
            var t = field[x0, y0];
            field[x0, y0] = field[x1, y1];
            field[x1, y1] = t;
            return field;
        }
    }
}