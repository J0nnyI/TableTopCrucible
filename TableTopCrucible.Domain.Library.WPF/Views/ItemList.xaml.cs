using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.Views
{
    /// <summary>
    /// Interaction logic for ItemList.xaml
    /// </summary>
    public partial class ItemList : UserControl
    {
        public ItemList()
        {
            InitializeComponent();
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is ItemListViewModel vm &&
                sender is ListViewItem rawItem &&
                rawItem.DataContext is ItemSelectionInfo itemVm)
                vm.ItemClickedCommand.Execute(new ItemClickedEventArgs(itemVm, e));
        }
    }
}
