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

        public TagEditor()
        {
            InitializeComponent();
        }

        private void TagEditor_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is Window window)
            {
                window.SizeChanged += (_, __) => updatePopupPos();
                window.LocationChanged += (_,__)=>updatePopupPos();
                window.Deactivated += (_, __) => this.Editing = false;
            }
        }

        private void self_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!this.IsKeyboardFocusWithin)
                this.Editing = false;
        }

        private void updatePopupPos()
        {
            var offset = popup.HorizontalOffset;
            popup.HorizontalOffset = offset + .1;
            popup.HorizontalOffset = offset;

        }


        private void newTagClicked(object sender, RoutedEventArgs e)
        {
            this.Editing = true;
        }

        #region dep props

        public IEnumerable<Tag> Tags
        {
            get => (IEnumerable<Tag>)GetValue(TagsProperty);
            set => SetValue(TagsProperty, value);
        }
        public static readonly DependencyProperty TagsProperty =
            DependencyProperty.Register(nameof(Tags), typeof(IEnumerable<Tag>), typeof(TagEditor), new PropertyMetadata(null));

        public bool Editable
        {
            get => (bool)GetValue(EditableProperty);
            set => SetValue(EditableProperty, value);
        }
        public static readonly DependencyProperty EditableProperty =
            DependencyProperty.Register(nameof(Editable), typeof(bool), typeof(TagEditor), new PropertyMetadata(true));

        public bool Editing
        {
            get => (bool)GetValue(ShowPopupProperty);
            set => SetValue(ShowPopupProperty, value);
        }
        public static readonly DependencyProperty ShowPopupProperty =
            DependencyProperty.Register(nameof(Editing), typeof(bool), typeof(TagEditor), new PropertyMetadata(false));
        #endregion

    }
}
