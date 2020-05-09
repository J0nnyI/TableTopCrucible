using MaterialDesignThemes.Wpf;
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



        public TagEditor()
        {
            InitializeComponent();
        }

    }
}
