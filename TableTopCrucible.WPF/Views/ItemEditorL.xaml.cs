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
using TableTopCrucible.Domain.ValueTypes.IDs;
using TableTopCrucible.WPF.ViewModels;

namespace TableTopCrucible.WPF.Views
{
    /// <summary>
    /// Interaction logic for ItemEditorL.xaml
    /// </summary>
    public partial class ItemEditorL : UserControl
    {



        public ItemId ItemId
        {
            get { return (ItemId)GetValue(ItemIdProperty); }
            set { SetValue(ItemIdProperty, value); }
        }
        public static readonly DependencyProperty ItemIdProperty =
            DependencyProperty.Register(nameof(ItemId), typeof(ItemId), typeof(ItemEditorL), new PropertyMetadata(default(ItemId)));



        public ItemEditorL()
        { }
    }
}
