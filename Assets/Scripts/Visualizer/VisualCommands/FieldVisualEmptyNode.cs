using RSG;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualEmptyNode : FieldVisualCommand
    {
        protected override void RunInner(IFieldVisualizer visualizer)
        {
            ResolveFinishPromise();
        }
    }
}