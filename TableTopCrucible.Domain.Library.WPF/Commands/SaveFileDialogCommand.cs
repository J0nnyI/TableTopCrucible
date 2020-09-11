using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Core.Services;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class SaveFileDialogCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Table Top Crucible Library (*.ttcl)|*.*";
            if (dialog.ShowDialog() == true && parameter is Action<string> callback)
                callback(dialog.FileName);
        }
    }
}
