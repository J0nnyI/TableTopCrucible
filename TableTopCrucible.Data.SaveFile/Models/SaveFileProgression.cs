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
        public ITaskProgressionInfo FileTaskState { get; set; }
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
                m => m.FileTaskState,
                m => m.LinkTaskState,
                m => m.ItemTaskState,
                (dir, file, link, item)
                    => new { dir, file, link, item });

            this.SubTaskProgressionChanges =
                onNextPhase
                .Select(tasks =>
                    tasks.item?.TaskProgressionStateChanges
                    ?? tasks.link?.TaskProgressionStateChanges
                    ?? tasks.file?.TaskProgressionStateChanges
                    ?? tasks.dir?.TaskProgressionStateChanges
                    ?? Observable.Return<TaskProgressionState>(default)
                )
                .Switch();

            this.SubTaskProgressionChanges.Subscribe(prog => this.SubTaskProgression = prog);

            onNextPhase.Subscribe(tasks =>
            {
                var dir = tasks.dir;
                var file = tasks.file;
                var link = tasks.link;
                var item = tasks.item;
                var states = new ITaskProgressionInfo[] { dir, file, link, item };
                if ((TaskState.Failed as TaskState?).IsIn(dir?.State, file?.State, link?.State, item?.State))
                {
                    _mainTaskProgression.State = TaskState.Failed;
                    return;
                }
                this._mainTaskProgression.RequiredProgress = states.Count(states => states?.State == TaskState.Done);
                this._mainTaskProgression.Details =
                    item?.Title
                    ?? link?.Title
                    ?? file?.Title
                    ?? dir?.Title;
            });
        }

    }
}
