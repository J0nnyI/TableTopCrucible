using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.ViewModels
{
    public class DirectorySetupCardViewModel : DisposableReactiveObject
    {
        public BehaviorSubject<DirectorySetup> DirectorySetupChanges = new BehaviorSubject<DirectorySetup>(default);


        [Reactive] public string Name { get; set; }
        [Reactive] public string Description { get; set; }
        [Reactive] public string Path { get; set; }

        public DirectorySetupCardViewModel()
        {
            this.DirectorySetupChanges
                .TakeUntil(destroy)
                .Subscribe(dirSetup =>
            {
                this.Name = (string)dirSetup.Name;
                this.Description = (string)dirSetup.Description;
                this.Path = dirSetup.Path?.LocalPath;
            });
        }
    }
}
