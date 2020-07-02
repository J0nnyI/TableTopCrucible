﻿using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Utilities;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;
using TableTopCrucible.WPF.Helper;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class DirectorySetupCardViewModel : DisposableReactiveObjectBase
    {
        public BehaviorSubject<DirectorySetup> DirectorySetupChanges = new BehaviorSubject<DirectorySetup>(default);

        private IFileDataService _fileInfoService { get; }

        public ICommand EnterEditmode { get; }
        public ICommand Save { get; }
        public ICommand Undo { get; }
        public ICommand Delete { get; }

        public IObservable<int> FileCountChanges;
        private readonly ObservableAsPropertyHelper<int> _fileCount;
        public int FileCount => _fileCount.Value;

        public IObservable<int> DistinctFileCountChanges;
        private readonly ObservableAsPropertyHelper<int> _distinctFileCount;
        public int DistinctFileCount => _distinctFileCount.Value;

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

        public DirectorySetupCardViewModel(IFileDataService fileInfoService, SaveDirectorySetupCommand save, DeleteDirectorySetupCommand delete)
        {
            this._fileInfoService = fileInfoService;

            var fileCount = this.DirectorySetupChanges
                .Select(dirSetup => dirSetup.Id)
                .Where(id => id != default)
                .DistinctUntilChanged()
                .Select(id =>
                    this._fileInfoService
                        .Get(id)
                        .Connect())
                .Switch();
            this.FileCountChanges =
                fileCount
                .QueryWhenChanged(query => query.Count)
                .TakeUntil(destroy);
            this.DistinctFileCountChanges =
                fileCount
                .Filter(fileInfo => fileInfo.HashKey.HasValue)
                .Transform(fileInfo => fileInfo.HashKey)
                .RemoveKey()
                .QueryWhenChanged(query => query.Distinct().Count())
                .TakeUntil(destroy);

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



            this._fileCount = this.FileCountChanges.ToProperty(this, nameof(FileCount));
            this._distinctFileCount = this.DistinctFileCountChanges.ToProperty(this, nameof(DistinctFileCount));
            this._name = NameChanges.ToProperty(this, nameof(Name));
            this._description = DescriptionChanges.ToProperty(this, nameof(Description));
            this._path = this.PathChanges.ToProperty(this, nameof(Path));

            this.disposables.Add(_fileCount, _distinctFileCount, NameChanges, DescriptionChanges, PathChanges, DirectorySetupChanges);

            this.NameChanges
                .Subscribe(name => { if (Changeset != null) Changeset.Name = name; });
            this.DescriptionChanges
                .Subscribe(description => { if (Changeset != null) Changeset.Description = description; });
            this.PathChanges
                .Subscribe(path => { if (Changeset != null) Changeset.Path = path; });

            this.DirectorySetupChanges
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