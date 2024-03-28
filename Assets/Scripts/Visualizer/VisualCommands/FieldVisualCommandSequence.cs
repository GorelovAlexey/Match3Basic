using System;
using System.Collections.Generic;
using System.Linq;
using RSG;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public class FieldVisualCommandSequence : FieldVisualCommand
    {
        private List<FieldVisualCommand> commands;
        public FieldVisualCommandSequence(params FieldVisualCommand[] commands) : base()
        {
            this.commands = commands.ToList();
        }

        public void Add(FieldVisualCommand c)
        {
            commands.Add(c);
        }

        protected override void RunInner(IFieldVisualizer visualizer)
        {
            var funcs = commands.Select<FieldVisualCommand, Func<IPromise>>(x => 
                () => x.RunAllSequence(visualizer)).ToArray();
            Promise.Sequence(funcs).Then(ResolveFinishPromise);
        }

        public override void Break()
        {
            commands.ForEach(x => x.Break());
            base.Break();
        }
    }
}