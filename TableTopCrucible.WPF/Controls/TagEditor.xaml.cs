using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TableTopCrucible.Domain.ValueTypes;
using System;

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
                window.Deactivated += (_, __) => this.Editing = false;
            }
        }

        private void self_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsKeyboardFocusWithin)
                this.Editing = false;
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


        #region new tag errors
        private static readonly DependencyPropertyKey NewTagErrorsPropertyKey
           = DependencyProperty.RegisterReadOnly(
               nameof(NewTagErrors),
               typeof(IEnumerable<string>), typeof(TagEditor),
               new FrameworkPropertyMetadata(default(IEnumerable<string>),
                   FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty NewTagErrorsProperty
            = NewTagErrorsPropertyKey.DependencyProperty;

        public IEnumerable<string> NewTagErrors
        {
            get => (IEnumerable<string>)GetValue(NewTagErrorsProperty);
            protected set => SetValue(NewTagErrorsPropertyKey, value);
        }

        #endregion


        #endregion


        private IEnumerable<string> validateNewTag(string tag)
        {
            var res = new List<string>();

            res.AddRange(Domain.ValueTypes.Tag.Validate(tag));

            if (this.Tags.Any(curTag => (string)curTag == tag))
                res.Add("this Tag is already added to the list");

            return res;
        }

        /// <summary>
        /// this methods expects valid data
        /// </summary>
        /// <param name="tag"></param>
        private void handleNewTagConfirmation(Tag tag)
        {
            var tags = this.Tags.ToList();
            var newTag = tag;

            tags.Add(newTag);
            this.Tags = tags.ToArray();
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is ComboBox comboBox 
                && e.Key == Key.Enter)
                this.handleNewTagConfirmation((Tag)comboBox.Text);
        }
        private void onNewTag(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox)
                {
                    var errors = this.validateNewTag(comboBox.Text).ToList();
                    errors.Add(DateTime.Now.ToString());
                    this.NewTagErrors = errors;
                    if (errors.Any())
                        return;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
