using System.Windows;
using System.Windows.Controls;

using TableTopCrucible.Domain.ValueTypes.IDs;

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
