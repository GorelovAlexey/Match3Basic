using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI.Controls;
using DG.Tweening;
using RSG;
using UniRx;
using UnityEngine;

namespace Assets.Scripts
{
    public class Loader : MonoBehaviour
    {
        [SerializeField] private BasicProgressBar progressBar;


        void Awake()
        {
            GameLoadInit();
        }


        private void GameLoadInit()
        {
            void FakeProcessTween(Promise finishPromise, float time, Ease easing = Ease.InOutSine)
            {
                float progress = 0f;
                void SetProgress(float f)
                {
                    progress = f;
                    finishPromise.ReportProgress(progress);
                }

                var tweeen = DOTween.To(() => progress, SetProgress, 1f, time).SetEase(easing);
                tweeen.OnComplete(finishPromise.Resolve);
            }

            var totalTask = new LoaderTaskContainer("Game loading", 1);
            var loadAssets = new LoaderTaskContainer("AssetsLoading", 4f);
            var createLevel = new LoaderSingleTask("Level creation", 1, x => FakeProcessTween(x, 5, Ease.InCubic));
            var loadResources = new LoaderSingleTask("Load resources", 1f, x => FakeProcessTween(x, 3));
            var loadWebResources = new LoaderSingleTask("Load web resources", 5f, x => FakeProcessTween(x, 8));

            totalTask.AddTasks(loadAssets, createLevel);
            loadAssets.AddTasks(loadResources, loadWebResources);

            totalTask.ProgressRatio.Subscribe(x => progressBar.Progress = x);
            totalTask.Start();
        }



        private abstract class LoaderTaskAbstract
        {
            protected readonly Promise finishPromise = new Promise();
            public IPromise FinishPromise => finishPromise;
            public float Weight { get; }
            public string Name { get; }
            public abstract IPromise Start();
            public ReactiveProperty<float> ProgressRatio { get; } = new ReactiveProperty<float>(0f);

            //public float GetProgressRatio => Weight > 0 ? Progress.Value / Weight : 1f;

            protected LoaderTaskAbstract(string name, float weight)
            {
                Weight = weight;
                Name = name;

                finishPromise.Then(FinishTask);
            }

            protected void FinishTask()
            {
                ProgressRatio.Value = 1f;
                if (finishPromise.CurState == PromiseState.Pending)
                    finishPromise.Resolve();
            }
        }
        private class LoaderSingleTask : LoaderTaskAbstract
        {
            private Action<Promise> action;

            public LoaderSingleTask(string name, float weight, Action<Promise> action) : base(name, weight)
            {
                this.action = action;
                finishPromise.Progress(x => ProgressRatio.Value = x);
            }

            public override IPromise Start()
            {
                action?.Invoke(finishPromise);
                if (action == null)
                {
                    FinishTask();
                }

                return FinishPromise;
            }
        }
        private class LoaderTaskContainer : LoaderTaskAbstract
        {
            private List<LoaderTaskAbstract> tasksToDo = new List<LoaderTaskAbstract>();
            public LoaderTaskContainer(string name, float weight) : base(name, weight) { }
            public void AddTasks(params LoaderTaskAbstract[] tasks)
            {
                tasksToDo.AddRange(tasks);
            }
            private CompositeDisposable disposable = new CompositeDisposable();
            public override IPromise Start()
            {
                disposable?.Dispose();
                disposable = new CompositeDisposable();

                if (tasksToDo.Count > 0)
                {
                    //Promise.All(tasksToDo.Select(x => x.FinishPromise)).Then(() => finishPromise.Resolve());
                    Promise.Sequence(tasksToDo.Select<LoaderTaskAbstract, Func<IPromise>>(x => x.Start))
                        .Then(FinishTask);

                    foreach (var t in tasksToDo)
                        t.ProgressRatio.Subscribe(x => UpdateTotalProgress()).AddTo(disposable);
                }
                else
                    FinishTask();

                return FinishPromise;
            }
            private void UpdateTotalProgress()
            {
                float weight = 0f;
                float progress = 0f;
                foreach (var t in tasksToDo)
                {
                    weight += t.Weight;
                    progress += t.ProgressRatio.Value * t.Weight;
                }

                ProgressRatio.Value = weight > 0 ? progress / weight : 1f;
            }
        }
    }
}

