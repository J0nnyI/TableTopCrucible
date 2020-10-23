using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Startup.WPF.ViewModels;

namespace TableTopCrucible.Startup.WPF.Views
{
    /// <summary>
    /// Interaction logic for StartupView.xaml
    /// </summary>
    public partial class StartupView : UserControl
    {
        public StartupView()
        {
            InitializeComponent();
        }

        private void ListViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(this.DataContext is StartupViewModel vm && 
                sender is ListViewItem item &&
                item.DataContext is LibraryLocation library)
                vm.OpenListedLibraryCommand.Execute(library.Path);
            else
                throw new InvalidOperationException($"{nameof(StartupView)}.{nameof(ListViewItem_MouseDown)}({sender},{e})");
        }
    }
}
