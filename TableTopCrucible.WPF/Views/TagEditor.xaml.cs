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
    /// Interaction logic for TagList.xaml
    /// </summary>
    public partial class TagEditor : UserControl
    {
        public TagEditor()
        {
            InitializeComponent();
        }
        private void _addTag(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && this.DataContext is TagEditorViewModel viewModel)
            {
                if (viewModel.AddTag.CanExecute(viewModel.NewTag))
                    viewModel.AddTag.Execute(viewModel.NewTag);
            }
        }
    }
}
