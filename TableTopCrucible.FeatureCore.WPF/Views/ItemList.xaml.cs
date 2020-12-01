using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using TableTopCrucible.FeatureCore.WPF.ViewModels;

namespace TableTopCrucible.FeatureCore.WPF.Views
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
            if (DataContext is ItemListViewModel vm &&
                sender is ListViewItem rawItem &&
                rawItem.DataContext is ItemSelectionInfo itemVm)
                vm.ItemClickedCommand.Execute(new ItemClickedEventArgs(itemVm, e));
        }
    }
}
