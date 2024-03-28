using System.Collections.Generic;
using System.Linq;
using RSG;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualCommandAll : FieldVisualCommand
    {
        private List<FieldVisualCommand> commands;
        public FieldVisualCommandAll(params FieldVisualCommand[] commands) : base()
        {
            this.commands = commands.ToList();
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            var all = commands.Select(x => x.RunAllSequence(visualizer));
            Promise.All(all).Then(ResolveFinishPromise);
        }

        public override void Break()
        {
            commands.ForEach(x => x.Break());

            base.Break();
        }
    }
}