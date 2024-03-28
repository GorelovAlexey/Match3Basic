using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Visualizer.VisualCommands;

namespace Assets.Scripts.Visualizer
{
    public class CommandsRegistry
    {
        private readonly HashSet<FieldVisualCommand> commands = new HashSet<FieldVisualCommand>();

        public void RegisterCommand(FieldVisualCommand c)
        {
            commands.Add(c);
        }

        public void RemoveCommand(FieldVisualCommand c)
        {
            if (commands.Contains(c))
                commands.Remove(c);
        }

        public void CancelAllCommands()
        {
            var cmds = commands.ToArray();

            foreach (var c in cmds)
                c.Break();

            commands.Clear();
        }
    }
}