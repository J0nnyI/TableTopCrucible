using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;

namespace TableTopCrucible.Core.Services
{
    public class LibraryInfoService:DisposableReactiveObjectBase
    {
        [Reactive]
        public AbsolutePath CurrentLibrary { get; set; }
        public IObservable<AbsolutePath> CurrentLibraryChanges { get; }
        public LibraryInfoService()
        {
            CurrentLibraryChanges = this.WhenAnyValue(vm => vm.CurrentLibrary);
        }
    }
}
