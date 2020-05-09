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
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ItemCard.xaml
    /// </summary>
    public partial class ItemCard : UserControl
    {


        public ItemChangeset ItemChangeset
        {
            get { return (ItemChangeset)GetValue(ItemChangesetProperty); }
            set { SetValue(ItemChangesetProperty, value); }
        }
        public static readonly DependencyProperty ItemChangesetProperty =
            DependencyProperty.Register(nameof(ItemChangeset), typeof(ItemChangeset), typeof(ItemCard), new PropertyMetadata(null));



        public ItemCard()
        {
            InitializeComponent();
        }
    }
}
