using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class FileVersionListViewModel : DisposableReactiveObjectBase
    {
        private BehaviorSubject<IEnumerable<VersionedFile>> _versionedFilesChanges = new BehaviorSubject<IEnumerable<VersionedFile>>(null);
        public ObservableAsPropertyHelper<IEnumerable<VersionedFile>> _versionedFiles;
        public IEnumerable<VersionedFile> VersionedFiles => _versionedFiles.Value;
        public void SetFiles(IEnumerable<VersionedFile> itemLinks)
        {
            _versionedFilesChanges.OnNext(itemLinks);
        }
        public FileVersionListViewModel()
        {
            this._versionedFiles = _versionedFilesChanges.ToProperty(this, nameof(VersionedFiles));
        }
    }
}
