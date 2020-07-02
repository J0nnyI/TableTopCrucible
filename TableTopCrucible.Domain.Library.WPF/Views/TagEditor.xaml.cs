using System.Windows.Controls;
using System.Windows.Input;

using TableTopCrucible.Domain.Library.WPF.ViewModels;

namespace TableTopCrucible.Domain.Library.WPF.Views
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
            if (e.Key == Key.Enter && this.DataContext is TagEditorViewModel viewModel)
            {
                if (viewModel.AddTag.CanExecute(viewModel.NewTag))
                    viewModel.AddTag.Execute(viewModel.NewTag);
            }
        }
    }
}
