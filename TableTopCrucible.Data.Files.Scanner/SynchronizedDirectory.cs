using DynamicData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Core.Jobs;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

using FileInfo = TableTopCrucible.Data.Models.Sources.FileInfo;

namespace TableTopCrucible.Data.Files.Scanner
{
    public class SynchronizedDirectory : DisposableReactiveObjectBase
    {
        private readonly SourceCache<FileInfo, FileInfoId> cache = new SourceCache<FileInfo, FileInfoId>(file => file.Id);
        private readonly DirectorySetup dirSetup;
        private readonly IJobManagementService _jobManagementService;
        private DirectoryScanner scanner;

        public string Path { get; private set; }

        public SynchronizedDirectory(IJobManagementService jobManagementService)
        {
            _jobManagementService = jobManagementService;
        }
        public void Update(IEnumerable<FileSystemEventArgs> change)
        {

        }
        public void Init(IEnumerable<FileInfo> files, string path)
        {
            this.Path = path;
        }
        public void Enable()
        {
            this.scanner = new DirectoryScanner(Path);
            scanner
                .Buffer(new TimeSpan(0, 1, 0))
                .Subscribe(changes =>
            {
                _jobManagementService.Start<Unit>(job =>
                {
                    var renames = changes.Where(change => change.ChangeType == WatcherChangeTypes.Renamed);
                    this.handleRenames(renames);
                    var deletes = changes.Where(change => change.ChacngeType == WatcherChangeTypes.Deleted);
                    this.handleDeletes(deletes);

                    var updates = changes.Where(change => change.ChangeType.IsIn(WatcherChangeTypes.Created, WatcherChangeTypes.Changed));
                    this.handleUpdates(updates);
                });
            });
        }

        private void handleUpdates(IEnumerable<DirectoryUpdate> updates)
        {
            throw new NotImplementedException();
        }

        private void handleDeletes(IEnumerable<DirectoryUpdate> updates)
        {
            //this.cache.Remove()
        }

        private void handleRenames(IEnumerable<DirectoryUpdate> renames)
        {
            throw new NotImplementedException("todo: pass new path (outer) for dir & file");
            var groups = renames.GroupBy(change => Directory.Exists(change.Path)).ToDictionary(x => x.Key);
            var dirs =
                groups[true]
                .Select(change => change.Path)
                .Distinct();

            var files =getMatchingFiles(groups[false]);

            //this.cache.Items.Where(file =>
            //    dirs.Any(dir =>
            //        file.Path == new Uri(dir, UriKind.Relative))
            //    )
            //    .Concat(files)
            //    .Distinct();

        }


        public void Disable()
        {
            this.scanner.Dispose();
            this.scanner = null;
        }
        private IEnumerable<MatchedFile> getMatchingFiles(IEnumerable<DirectoryUpdate> updates, Func<DirectoryUpdate, string> pathSelector = null)
            => updates
                .Distinct()
                .Join(cache.Items,
                update => new Uri(
                    pathSelector == null ? update.Path : pathSelector(update),
                    UriKind.Relative),
                    inner => inner.Path,
                    (update, file) => new MatchedFile(update, file));
        struct MatchedFile
        {
            public MatchedFile(DirectoryUpdate update, FileInfo file)
            {
                Update = update;
                File = file;
            }

            public DirectoryUpdate Update { get; }
            public FileInfo File { get; }
        }
    }
}
