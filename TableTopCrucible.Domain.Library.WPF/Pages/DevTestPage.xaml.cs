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
using TableTopCrucible.Domain.Library.WPF.PageViewModels;

namespace TableTopCrucible.Domain.Library.WPF.Pages
{
    /// <summary>
    /// Interaction logic for DevTestPage.xaml
    /// </summary>
    public partial class DevTestPage : Page
    {
        public DevTestPage()
        {
            InitializeComponent();
            this.DataContextChanged += (_, __) =>
            {
                if (DataContext is DevTestPageViewModel vm)
                {
                    vm.Viewport = this.vp;
                }
            };
        }
    }
}
