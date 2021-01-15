using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;
using SysFileInfo = System.IO.FileInfo;

namespace TableTopCrucible.Data.Files.Scanner
{
    public class DirectoryScanner : IObservable<DirectoryUpdate>, IDisposable
    {

        private FileSystemWatcher watcher;
        private bool _disposedValue;
        private Subject<Unit> destroy = new Subject<Unit>();
        private readonly IScheduler _taskpoolScheduler;

        public IObservable<DirectoryUpdate> Updates { get; }

        public DirectoryScanner(string directory, IScheduler taskpoolScheduler = null)
        {
            _taskpoolScheduler = taskpoolScheduler ?? RxApp.TaskpoolScheduler;
            watcher = new FileSystemWatcher(directory)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            };

            Updates = Observable.Merge(
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    del => watcher.Deleted += del,
                    del => watcher.Deleted -= del,
                    _taskpoolScheduler)
                    .Select(x => x.EventArgs),
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    del => watcher.Changed += del,
                    del => watcher.Changed -= del,
                    _taskpoolScheduler)
                    .Select(x => x.EventArgs),
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    del => watcher.Created += del,
                    del => watcher.Created -= del,
                    _taskpoolScheduler)
                    .Select(x => x.EventArgs),
                Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
                    del => watcher.Renamed += del,
                    del => watcher.Renamed -= del,
                    _taskpoolScheduler)
                    .Select(x => x.EventArgs)
                )
                .Select(x => new DirectoryUpdate(x.FullPath, x.ChangeType, (x as RenamedEventArgs)?.OldFullPath))
                .Where(x =>
                {
                    var isDir = Directory.Exists(x.Path);
                    var isFile = File.Exists(x.Path);
                    return isFile
                        || x.ChangeType == WatcherChangeTypes.Deleted
                        || isDir && x.ChangeType != WatcherChangeTypes.Changed;
                })
                .TakeUntil(destroy);
        }

        public IDisposable Subscribe(IObserver<DirectoryUpdate> observer)
        {
            return Updates.Subscribe(observer);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.destroy.OnNext(new Unit());
                    watcher.Dispose();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
    public struct DirectoryUpdate
    {
        public DirectoryUpdate(string path, WatcherChangeTypes changeType, string oldPath)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ChangeType = changeType;
            OldPath = oldPath;
        }

        public string Path { get; }
        public string OldPath { get; }
        public WatcherChangeTypes ChangeType { get; }

        public override bool Equals(object obj)
        {
            return obj is DirectoryUpdate update &&
                   Path == update.Path &&
                   ChangeType == update.ChangeType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, ChangeType);
        }
    }
}
