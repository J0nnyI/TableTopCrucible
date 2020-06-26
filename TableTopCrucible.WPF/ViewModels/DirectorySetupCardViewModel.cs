using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class DirectorySetupCardViewModel : DisposableReactiveObject
    {
        public BehaviorSubject<DirectorySetup> DirectorySetupChanges = new BehaviorSubject<DirectorySetup>(default);

        private IFileInfoService _fileInfoService { get; }

        public ICommand EnterEditmode { get; }
        public ICommand Save { get; }
        public ICommand Undo { get; }
        public ICommand Delete { get; }

        public IObservable<int> FileCountChanges;
        private readonly ObservableAsPropertyHelper<int> _fileCount;
        public int FileCount => _fileCount.Value;

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

        public DirectorySetupCardViewModel(IFileInfoService fileInfoService, SaveDirectorySetupCommand save, DeleteDirectorySetupCommand delete)
        {
            this._fileInfoService = fileInfoService;

            this.FileCountChanges = this.DirectorySetupChanges
                .Select(dirSetup => dirSetup.Id)
                .Where(id => id != default)
                .DistinctUntilChanged()
                .Select(id =>
                    this._fileInfoService
                        .Get(id)
                        .Connect())
                .Switch()
                .QueryWhenChanged(query => query.Count)
                .TakeUntil(destroy);

            this._fileCount = this.FileCountChanges
                .TakeUntil(this.destroy)
                .ToProperty(this, nameof(FileCount));

            this.Save = save;
            this.Delete = delete;
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
