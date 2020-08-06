using System;
using System.Windows.Controls;
using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.Views
{
    /// <summary>
    /// Interaction logic for ItemEditor.xaml
    /// </summary>
    public partial class ItemEditor : UserControl
    {

        public ItemEditor()
        {
            InitializeComponent();
            this.DataContextChanged += ItemEditor_DataContextChanged;
        }

        private void ItemEditor_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if(this.DataContext is ItemEditorViewModel vm)
            {
                vm.ViewportControl = viewport;
            }
        }
    }
}
