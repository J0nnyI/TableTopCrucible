using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TableTopCrucible.Core.WPF.Commands
{
    public class OpenFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
            => parameter is string path && File.Exists(path);

        public void Execute(object parameter)
        {
            if (parameter is string path && File.Exists(path))
            {

                Process fileopener = new Process();
                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + path + "\"";
                fileopener.Start();
            }
            else
                MessageBox.Show("could not find file " + parameter.ToString());
        }
    }
}
