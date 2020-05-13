using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using TableTopCrucible.Domain.ValueTypes;

namespace TableTopCrucible.WPF.Controls
{
    /// <summary>
    /// Interaction logic for TagEditor.xaml
    /// </summary>
    public partial class TagEditor : UserControl
    {


        public IEnumerable<Tag> Tags
        {
            get { return (IEnumerable<Tag>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register(nameof(Tags), typeof(IEnumerable<Tag>), typeof(TagEditor), new PropertyMetadata(null));



        public bool Editable
        {
            get { return (bool)GetValue(EditableProperty); }
            set { SetValue(EditableProperty, value); }
        }
        public static readonly DependencyProperty EditableProperty =
            DependencyProperty.Register(nameof(Editable), typeof(bool), typeof(TagEditor), new PropertyMetadata(true));



        public TagEditor()
        {
            InitializeComponent();
        }

    }
}
