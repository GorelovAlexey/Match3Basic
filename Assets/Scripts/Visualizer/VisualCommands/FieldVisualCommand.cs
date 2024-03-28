using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Engine;
using RSG;
using UnityEngine;

namespace Assets.Scripts.Visualizer.VisualCommands
{
    public abstract class FieldVisualCommand
    {
        public FieldVisualCommand Next { get; private set; }
        private readonly Promise finishPromise = new Promise();
        public IPromise FinishPromise => finishPromise;

        private bool _break = false;
        public bool IsDone => _break || finishPromise.CurState == PromiseState.Resolved;

        protected void ResolveFinishPromise()
        {
            if (finishPromise.CurState == PromiseState.Pending)
                finishPromise.Resolve();
        }

        public FieldVisualCommand SetNext(params FieldVisualCommand[] next)
        {
            var cur = this;
            foreach (var cmd in next)
            {
                cur.Next = cmd;
                cur = cmd;
            }

            return cur;
        }

        protected FieldVisualCommand()
        {
        }

        public IPromise Run(IFieldVisualizer field)
        {
            if (IsDone)
                return Promise.Resolved();

            field.CommandsRegistry.RegisterCommand(this);
            FinishPromise.Then(() => field.CommandsRegistry.RemoveCommand(this));

            RunInner(field);

            return FinishPromise;
        }

        protected abstract void RunInner(IFieldVisualizer visualizer);
        
        public virtual void Break()
        {
            var next = Next;

            _break = true;

            Next = null;
            next?.Break();
        }

        public IPromise PromiseAllDone()
        {
            var node = this;
            while (node.Next != null)
                node = node.Next;

            return node.FinishPromise;
        }

        public IPromise RunAllSequence(IFieldVisualizer field)
        {
            var funcs = new List<Func<IPromise>>();

            foreach (var c in GetCommandsList())
            {
                var command = c;
                var f = field;
                funcs.Add(() => command.Run(f));
            }

            return Promise.Sequence(funcs).Catch(Debug.LogError);
        }

        public List<FieldVisualCommand> GetCommandsList()
        {
            var list = new List<FieldVisualCommand>();
            var node = this;
            while (node != null)
            {
                list.Add(node);
                node = node.Next;
            }

            return list;
        }

        public FieldVisualCommand Last
        {
            get
            {
                var cur = this;
                while (true)
                {
                    if (cur.Next == null)
                        return cur;

                    cur = cur.Next;
                    if (cur == this)
                        throw new Exception("[FieldVisualCommand] Last - endless loop");
                }
            }
        }
    }
}