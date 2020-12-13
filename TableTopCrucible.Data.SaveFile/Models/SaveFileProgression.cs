using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Data.SaveFile.Models
{
    public class SaveFileProgression : ReactiveObject, ISaveFileProgression
    {
        [Reactive]
        public ITaskProgressionInfo DirectoryTaskState { get; set; }
        [Reactive]
        public ITaskProgressionInfo ImageFileTaskState { get; set; }
        [Reactive]
        public ITaskProgressionInfo ModelFileTaskState { get; set; }
        [Reactive]
        public ITaskProgressionInfo LinkTaskState { get; set; }
        [Reactive]
        public ITaskProgressionInfo ItemTaskState { get; set; }
        private TaskProgression _mainTaskProgression = new TaskProgression();
        public ITaskProgressionInfo MainTaskProgression => _mainTaskProgression;
        [Reactive]
        public TaskProgressionState SubTaskProgression { get; private set; }
        public IObservable<TaskProgressionState> SubTaskProgressionChanges { get; }

        public void OnError(Exception ex)
        {
            this._mainTaskProgression.Error = ex;
            this._mainTaskProgression.State = TaskState.Failed;
        }

        public SaveFileProgression()
        {
            this.MainTaskProgression.Title = "Loading File";
            this._mainTaskProgression.RequiredProgress = 4;

            var onNextPhase = this.WhenAnyValue(
                m => m.DirectoryTaskState,
                m => m.ModelFileTaskState,
                m => m.ImageFileTaskState,
                m => m.LinkTaskState,
                m => m.ItemTaskState,
                (dir, modelFile, imageFile, link, item)
                    => new { dir, modelFile, imageFile, link, item });

            this.SubTaskProgressionChanges =
                onNextPhase
                .Select(tasks => tasks.item ?? tasks.link ?? tasks.modelFile ?? tasks.imageFile ?? tasks.dir)
                .Select(task => task?.TaskProgressionStateChanges ?? Observable.Return<TaskProgressionState>(default))
                .Switch();

            this.SubTaskProgressionChanges.Subscribe(prog => this.SubTaskProgression = prog);

            onNextPhase
                .Select(tasks => Observable.CombineLatest(
                     tasks.dir?.TaskProgressionStateChanges.Select(x => x as TaskProgressionState?) ?? Observable.Return<TaskProgressionState?>(null),
                    tasks.modelFile?.TaskProgressionStateChanges.Select(x => x as TaskProgressionState?) ?? Observable.Return<TaskProgressionState?>(null),
                    tasks.imageFile?.TaskProgressionStateChanges.Select(x => x as TaskProgressionState?) ?? Observable.Return<TaskProgressionState?>(null),
                    tasks.link?.TaskProgressionStateChanges.Select(x => x as TaskProgressionState?) ?? Observable.Return<TaskProgressionState?>(null),
                    tasks.item?.TaskProgressionStateChanges.Select(x => x as TaskProgressionState?) ?? Observable.Return<TaskProgressionState?>(null),
                    (dir, modelFile, imageFile, link, item)=> { return new { dir, modelFile, imageFile, link, item }; }))
                .Switch()
                .Subscribe(x =>
            {
                var progInfos = new TaskProgressionState?[] { x.dir, x.modelFile,x.imageFile, x.link, x.item };
                if ((TaskState.Failed as TaskState?).IsIn(x.dir?.State, x.modelFile?.State,x.imageFile?.State, x.link?.State, x.item?.State))
                {
                    _mainTaskProgression.Error = x.dir?.Error ?? x.modelFile?.Error ?? x.imageFile?.Error ?? x.link?.Error ?? x.item?.Error;
                    _mainTaskProgression.State = TaskState.Failed;
                }
                else if (progInfos.Any(x => x?.State == TaskState.InProgress))
                    _mainTaskProgression.State = TaskState.InProgress;
                else if (progInfos.All(x => x?.State == TaskState.Done))
                    _mainTaskProgression.State = TaskState.Done;
                this._mainTaskProgression.CurrentProgress = progInfos.Count(progInfo => progInfo?.State == TaskState.Done);
                this._mainTaskProgression.Details =
                    x.item?.Title
                    ?? x.link?.Title
                    ?? x.imageFile?.Title
                    ?? x.modelFile?.Title
                    ?? x.dir?.Title;
            });
        }

    }
}
