using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

using TableTopCrucible.Core.Enums;
using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Data.SaveFile.WPF.ViewModels
{
    public class SaveFileLoadingViewModel : DisposableReactiveObjectBase
    {
        [Reactive]
        public TaskState PrimaryState { get; private set; }
        [Reactive]
        public int PrimaryRequired { get; private set; }
        [Reactive]
        public int PrimaryCurrent { get; private set; }
        [Reactive]
        public TaskState SecondaryState { get; private set; }
        [Reactive]
        public int SecondaryRequired { get; private set; }
        [Reactive]
        public int SecondaryCurrent { get; private set; }
        [Reactive]
        public string Title { get; private set; }
        [Reactive]
        public string Details { get; private set; }
        [Reactive]
        public string Error { get; private set; }

        public void SetProgressionSource(ISaveFileProgression progression)
        {
            progression
                .MainTaskProgression
                .TaskProgressionStateChanges
                .TakeUntil(destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(v =>
                {
                    this.PrimaryCurrent= v.CurrentProgress;
                    this.PrimaryRequired = v.RequiredProgress;
                    this.PrimaryState= v.State;
                    this.Details = v.Details;
                    this.Title = v.Title;
                });
            progression
                .SubTaskProgressionChanges
                .TakeUntil(destroy)
                .ObserveOn(RxApp.MainThreadScheduler)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(v =>
                {
                    SecondaryCurrent = v.CurrentProgress;
                    SecondaryRequired = v.RequiredProgress;
                    SecondaryState = v.State;
                });
        }
    }
}
