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
using TableTopCrucible.WPF.ViewModels;

namespace TableTopCrucible.WPF.Pages
{
    /// <summary>
    /// Interaction logic for ItemEditorPage.xaml
    /// </summary>
    public partial class ItemEditorPage : Page
    {
        public ItemListViewModel ItemList { get; }
        public ItemEditorViewModel ItemEditorVm { get; }

        public ItemEditorPage(ItemListViewModel itemList, ItemEditorViewModel itemEditorVm)
        {
            this.ItemList = itemList ?? throw new ArgumentNullException(nameof(itemList));
            this.ItemEditorVm = itemEditorVm ?? throw new ArgumentNullException(nameof(itemEditorVm));
            InitializeComponent();
        }
    }
}
