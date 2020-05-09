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
    public partial class ItemCard : UserControl
    {
        public ItemChangeset ItemChangeset
        {
            get => (ItemChangeset)GetValue(ItemChangesetProperty); 
            set => SetValue(ItemChangesetProperty, value);
        }
        public static readonly DependencyProperty ItemChangesetProperty =
            DependencyProperty.Register(nameof(ItemChangeset), typeof(ItemChangeset), typeof(ItemCard), new PropertyMetadata(null));


        public bool Editable
        {
            get => (bool)GetValue(EditableProperty); 
            set => SetValue(EditableProperty, value); 
        }
        public static readonly DependencyProperty EditableProperty =
            DependencyProperty.Register(nameof(Editable), typeof(bool), typeof(ItemCard), new PropertyMetadata(true));


        public bool EditMode
        {
            get => (bool)GetValue(EditModeProperty); 
            set => SetValue(EditModeProperty, value);
        }
        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register(nameof(EditMode), typeof(bool), typeof(ItemCard), new PropertyMetadata(false));



        public ItemCard()
        {
            InitializeComponent();
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("delete?");
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = false;
        }

        private void enterEdit_Click(object sender, RoutedEventArgs e)
        {
            this.EditMode = true;
        }
    }
}
