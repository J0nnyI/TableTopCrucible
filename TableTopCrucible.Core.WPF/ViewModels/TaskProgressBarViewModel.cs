using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.WPF.ViewModels
{
    public interface ITaskProgressBar
    {
        ITaskProgressionInfo PrimaryProgress { get; set; }
        ITaskProgressionInfo SecondaryProgress { get; set; }
    }
    public class TaskProgressBarViewModel : DisposableReactiveObjectBase, ITaskProgressBar
    {
        [Reactive]
        public ITaskProgressionInfo PrimaryProgress { get; set; }
        [Reactive]
        public ITaskProgressionInfo SecondaryProgress { get; set; }
    }
}
