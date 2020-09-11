using Microsoft.Win32;

using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Core.Services;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class OpenFileDialogCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Table Top Crucible Library (*.ttcl)|*.*";
            if (dialog.ShowDialog() == true && parameter is Action<string> callback)
                callback(dialog.FileName);
        }
    }
}
