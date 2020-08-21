using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.Commands
{
    public class CreateAllThumbnailsCommand : ICommand
    {
        private readonly MassThumbnailCreatorViewModel massThumbnailCreator;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public CreateAllThumbnailsCommand(MassThumbnailCreatorViewModel massThumbnailCreator)
        {
            this.massThumbnailCreator = massThumbnailCreator;
        }


        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            new Window()
            {
                Content = massThumbnailCreator
            }.Show();
        }
    }
}
