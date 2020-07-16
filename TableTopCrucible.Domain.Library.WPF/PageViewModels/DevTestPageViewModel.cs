using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.Library.WPF.Commands;
using TableTopCrucible.Domain.Library.WPF.ViewModels;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.PageViewModels
{
    public class DevTestPageViewModel : PageViewModelBase
    {

        public DevTestPageViewModel(
            SaveFileDialogCommand saveFile,
            OpenFileDialogCommand openFile
            ) : base("dev test", PackIconKind.DeveloperBoard)
        {
            this.SaveFile = saveFile;
            this.OpenFile = openFile;
        }

        public ICommand SaveFile { get; }
        public ICommand OpenFile { get; }
    }
}
