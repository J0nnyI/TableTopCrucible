using DynamicData;

using System;
using System.Linq;
using System.Reactive.Linq;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.ValueTypes;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Data.Services;
using TableTopCrucible.Domain.Library.WPF.Filter.Models;
using TableTopCrucible.Domain.Library.WPF.Filter.ViewModel;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public enum TagFilterMode
    {
        HasAll,
        HasAny,
        HasNone
    }
    public enum TextFilterMode
    {
        StartsWith,
        EndsWith,
        Equals,
        Contains
    }
    public class ItemListFilterViewModel : DisposableReactiveObjectBase
    {
        public IObservable<Func<ItemEx, bool>> FilterChanges { get; }
        public IObservableList<Tag> Tagpool { get; }

        public ItemListFilterViewModel(
            IItemFilter itemWhitelist,
            IItemFilter itemBlacklist,
            IDirectoryDataService directoryDataService,
            IItemService itemService)
        {
            itemWhitelist.FilterMode = FilterMode.Whitelist;
            ItemWhitelist = itemWhitelist;
            itemBlacklist.FilterMode = FilterMode.Blacklist;
            ItemBlacklist = itemBlacklist;

            this.FilterChanges = Observable.CombineLatest(
                itemWhitelist.FilterChanges,
                itemBlacklist.FilterChanges
            )
            .Select(filters => new Func<ItemEx, bool>( (ItemEx item) => filters.All(filter => filter(item))));

        }

        public IItemFilter ItemWhitelist { get; }
        public IItemFilter ItemBlacklist { get; }
    }
}
