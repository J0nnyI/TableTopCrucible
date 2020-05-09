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

namespace TableTopCrucible.WPF.Views
{
    /// <summary>
    /// Interaction logic for ItemList.xaml
    /// </summary>
    public partial class ItemList : UserControl
    {
        public ItemList()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.ItemListViewModel();
        }
    }
}
