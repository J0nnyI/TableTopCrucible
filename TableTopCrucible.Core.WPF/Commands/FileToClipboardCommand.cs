using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TableTopCrucible.Core.WPF.Commands
{
    public class FileToClipboardCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
            => parameter is string str && File.Exists(str);

        public void Execute(object parameter)
        {
            if (parameter is string str)
            {
                var files = new StringCollection();
                files.Add(str);
                Clipboard.SetText(str);
            }
        }
    }
}
