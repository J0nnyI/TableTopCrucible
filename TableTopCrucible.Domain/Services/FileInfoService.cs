using System;
using System.Collections.Generic;
using System.IO;
using SysFileInfo = System.IO.FileInfo;
using System.Text;

using TableTopCrucible.Domain.Models.Sources;
using FileInfo = TableTopCrucible.Domain.Models.Sources.FileInfo;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;
using System.Dynamic;
using DynamicData;
using System.Reactive.Linq;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive;
using TableTopCrucible.Domain.Models;
using ReactiveUI;

namespace TableTopCrucible.Domain.Services
{
    public class DirectoryObserver : IDisposable
    {
        internal struct fileEntry
        {
            public fileEntry(Guid id, string path)
            {
                this.Id = id;
                this.Path = path ?? throw new ArgumentNullException(nameof(path));
                this.fileChanged = new Subject<Unit>();
            }
            public fileEntry(string path)
            {
                this.Id = Guid.NewGuid();
                this.Path = path ?? throw new ArgumentNullException(nameof(path));
                this.fileChanged = new Subject<Unit>();
            }

            internal readonly Subject<Unit> fileChanged;
            public IObservable<Unit> FileChanged => fileChanged;

            public Guid Id { get; }
            public string Path { get; }
        }

        private SourceCache<fileEntry, Guid> cache = new SourceCache<fileEntry, Guid>(file => file.Id);
        internal ISourceCache<fileEntry, Guid> Cache => cache;
        private FileSystemWatcher fileWatcher;
        private CompositeDisposable disposables = new CompositeDisposable();
        private Uri _path;
        public DirectorySetupId DirectorySetupId { get; private set; }
        public DirectoryObserver(Uri path, DirectorySetupId directorySetupId)
        {
            this._path = path;
            this.DirectorySetupId = directorySetupId;
            fileWatcher = new FileSystemWatcher(_path.LocalPath);
            fileWatcher.EnableRaisingEvents = true;
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.Created += this._fileWatcher_Created;
            fileWatcher.Deleted += this._fileWatcher_Deleted;
            fileWatcher.Renamed += this._fileWatcher_Renamed;
            fileWatcher.Changed += this._fileWatcher_Changed;
            fileWatcher.DisposeWith(disposables);

            Directory.GetFiles(_path.LocalPath, "*", SearchOption.AllDirectories)
                .ToList()
                .ForEach(path => cache.AddOrUpdate(new fileEntry(path)));

            fileWatcher.BeginInit();
        }

        private void _fileWatcher_Changed(object sender, FileSystemEventArgs e) => throw new NotImplementedException();
        private void _fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            var element = cache.Items.FirstOrDefault(file => file.Path == e.FullPath);
            cache.AddOrUpdate(new fileEntry(element.Id, e.FullPath));
        }
        private void _fileWatcher_Deleted(object sender, FileSystemEventArgs e) => throw new NotImplementedException();
        private void _fileWatcher_Created(object sender, FileSystemEventArgs e) => throw new NotImplementedException();
        public void Dispose() => throw new NotImplementedException();
    }

    public interface IFileInfoService
    {
        void Synchronize();
    }
    public class FileInfoService : DisposableReactiveObject, IFileInfoService
    {
        private bool synchronizing = false;
        private IDirectorySetupService _directorySetupService;
        public FileInfoService(IDirectorySetupService directorySetupService)
        {
            this._directorySetupService = directorySetupService;
        }

        public void Synchronize()
        {
            if (synchronizing)
                return;
            synchronizing = true;
            Console.WriteLine("### Synchronizing");
            this._directorySetupService.Get()
                .Connect()
                .Transform(x => new DirectoryObserver(x.Path, x.Id), true)
                .RemoveKey()
                .DisposeMany()
                .TakeUntil(destroy)
                .SubscribeMany(dirObserver=>
                {
                    Console.WriteLine("### got new dir observer", dirObserver);
                    return dirObserver
                           .Cache
                           .Connect()
                           .RemoveKey()
                           .Transform(x =>
                               x.FileChanged.Select(_ => x.Path)
                           )
                           .sele
                           .SubscribeMany(path =>
                           {
                               Console.WriteLine("### path updating", path);
                           });
                })
                .Subscribe();





        }
    }
}
