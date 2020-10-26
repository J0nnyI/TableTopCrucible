using ReactiveUI.Fody.Helpers;

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

using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Library.WPF.Filter.Models;
using TableTopCrucible.Domain.Library.WPF.Tagging.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Views;

namespace TableTopCrucible.Domain.Library.WPF.Filter.ViewModel
{
    public interface IItemFilter
    {
        FilterType FilterType { get; set; }
        IObservable<Func<ItemEx,bool>> FilterChanges { get; }
    }
    public class ItemFilterViewModel:IItemFilter
    {
        [Reactive]
        public FilterType FilterType { get; set; } = FilterType.Blacklist;

        public IObservable<Func<ItemEx,bool>> FilterChanges { get; }
        public ItemFilterViewModel(ITagEditor tagEditor)
        {
        }
    }
}
