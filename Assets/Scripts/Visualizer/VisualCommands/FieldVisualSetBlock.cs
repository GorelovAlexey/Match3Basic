using RSG;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualSetBlock : FieldVisualCommand
    {
        private bool setBlock;

        public FieldVisualSetBlock(bool setBlock) : base()
        {
            this.setBlock = setBlock;
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            visualizer.IsInputBlocked = setBlock;
            ResolveFinishPromise();
        }
    }
}