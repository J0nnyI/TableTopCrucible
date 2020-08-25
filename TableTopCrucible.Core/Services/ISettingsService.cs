using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.Services
{
    public interface ISettingsService : INotifyPropertyChanged
    {
        void Save();
        int ThreadCount { get; set; }
        int MaxPatchSize { get; set; }
        ushort CameraRotationMode { get; set; }
    }
}
