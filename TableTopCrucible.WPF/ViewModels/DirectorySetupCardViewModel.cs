using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class DirectorySetupCardViewModel : DisposableReactiveObject
    {
        public BehaviorSubject<DirectorySetup> DirectorySetupChanges = new BehaviorSubject<DirectorySetup>(default);


        public ICommand EnterEditmode { get; }
        public ICommand Save { get; }
        public ICommand Undo { get; }
        public ICommand Delete { get; }

        public BehaviorSubject<string> NameChanges = new BehaviorSubject<string>(null);
        private readonly ObservableAsPropertyHelper<string> _name;
        public string Name
        {
            get => _name.Value;
            set => NameChanges.OnNext(value);
        }

        public BehaviorSubject<string> DescriptionChanges = new BehaviorSubject<string>(null);
        private readonly ObservableAsPropertyHelper<string> _description;
        public string Description
        {
            get => _description.Value;
            set => DescriptionChanges.OnNext(value);
        }
        public BehaviorSubject<string> PathChanges = new BehaviorSubject<string>(null);
        private readonly ObservableAsPropertyHelper<string> _path;
        public string Path
        {
            get => _path.Value;
            set => PathChanges.OnNext(value);
        }
        [Reactive] public bool EditMode { get; set; }

        [Reactive] public DirectorySetupChangeset Changeset { get; set; }

        public DirectorySetupCardViewModel()
        {
            this.EnterEditmode = new RelayCommand(
                _ => this.EditMode = true,
                _ => !this.EditMode);
            this.Undo = new RelayCommand(
                _ =>
                {
                    this.EditMode = false;
                    DirectorySetupChanges.OnNext(DirectorySetupChanges.Value);
                },
                _ => this.EditMode);



            this._name = NameChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Name));
            this._description = DescriptionChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Description));
            this._path = this.PathChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Path));

            this.NameChanges
                .TakeUntil(destroy)
                .Subscribe(name => { if (Changeset != null) Changeset.Name = name; });
            this.DescriptionChanges
                .TakeUntil(destroy)
                .Subscribe(description => { if (Changeset != null) Changeset.Description = description; });
            this.PathChanges
                .TakeUntil(destroy)
                .Subscribe(path => { if (Changeset != null) Changeset.Path = path; });

            this.DirectorySetupChanges
                .TakeUntil(destroy)
                .Subscribe(_setValues);
        }
        private void _setValues(DirectorySetup dirSetup)
        {

            this.Changeset = new DirectorySetupChangeset(dirSetup);
            this.Name = (string)dirSetup.Name;
            this.Description = (string)dirSetup.Description;
            this.Path = dirSetup.Path?.LocalPath;
        }
    }
}
