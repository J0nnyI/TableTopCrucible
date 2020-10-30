using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Server.IIS.Core;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using TableTopCrucible.Core.Helper;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.Library.WPF.Filter.Models;
using TableTopCrucible.Domain.Library.WPF.Tagging.ViewModels;
using TableTopCrucible.Domain.Library.WPF.Views;

namespace TableTopCrucible.Domain.Library.WPF.Filter.ViewModel
{
    public interface IItemFilter
    {
        FilterMode FilterMode { get; set; }
        IObservable<Func<ItemEx, bool>> FilterChanges { get; }
    }
    public class ItemFilterViewModel : ReactiveObject, IItemFilter
    {

        public IEnumerable<KeyValuePair<FilterMode, string>> FilterModeValues { get; } = FilterMode.Blacklist.GetValuesForComboBox();
        public IEnumerable<KeyValuePair<PathFilterComponent, string>> PathFilterComponentValues { get; } = Models.PathFilterComponent.Directory.GetValuesForComboBox();
        public IEnumerable<KeyValuePair<StringFilterMode, string>> StringFilterModeValues { get; } = StringFilterMode.Advanced.GetValuesForComboBox();
        public IEnumerable<KeyValuePair<CaseSensitivityMode, string>> CaseSensitivityModeValues { get; } = CaseSensitivityMode.IgnoreCase.GetValuesForComboBox();

        [Reactive]
        public FilterMode FilterMode { get; set; } = FilterMode.Blacklist;

        [Reactive]
        public StringFilterMode NameFilterMode { get; set; } = StringFilterMode.Contains;
        [Reactive]
        public CaseSensitivityMode NameCaseSensitivity { get; set; } = CaseSensitivityMode.IgnoreCase;
        [Reactive]
        public string NameFilter { get; set; } = string.Empty;

        [Reactive]
        public StringFilterMode PathFilterMode { get; set; } = StringFilterMode.Contains;
        [Reactive]
        public PathFilterComponent PathFilterComponent { get; set; } = PathFilterComponent.Path;
        [Reactive]
        public CaseSensitivityMode PathCaseSensitivity { get; set; } = CaseSensitivityMode.IgnoreCase;
        [Reactive]
        public string PathFilter { get; set; } = string.Empty;

        public IObservable<Func<ItemEx, bool>> FilterChanges { get; }

        private string lowerIfRequired(string value, CaseSensitivityMode mode)
        {
            return mode switch
            {
                CaseSensitivityMode.IgnoreCase => value.ToLower(),
                CaseSensitivityMode.RespectCase => value,
                _ => throw new InvalidOperationException()
            };
        }
        private bool stringFilter(
            string text,
            string filter,
            StringFilterMode compMode,
            CaseSensitivityMode caseMode,
            FilterMode filterMode = FilterMode.Whitelist)
        {
            return string.IsNullOrWhiteSpace(filter) ||
                ((filterMode == FilterMode.Whitelist) == compMode switch
                {
                    StringFilterMode.StartsWith => lowerIfRequired(text, caseMode).StartsWith(lowerIfRequired(filter, caseMode)),
                    StringFilterMode.EndsWith => lowerIfRequired(text, caseMode).EndsWith(lowerIfRequired(filter, caseMode)),
                    StringFilterMode.Contains => lowerIfRequired(text, caseMode).Contains(lowerIfRequired(filter, caseMode)),
                    StringFilterMode.Advanced => lowerIfRequired(text, caseMode).Like(lowerIfRequired(filter, caseMode)),
                    _ => throw new NotImplementedException($"{nameof(ItemFilterViewModel)}.stringFilter {filter} is not implemented yet")
                });
        }

        private bool filePathFilter(
            ItemEx item,
            string filter,
            PathFilterComponent filterComp,
            StringFilterMode compMode,
            CaseSensitivityMode caseMode,
            FilterMode filterMode = FilterMode.Whitelist)
        {
            return string.IsNullOrWhiteSpace(PathFilter) ||
                (
                    (filterMode == FilterMode.Whitelist) ==
                    filterComp switch
                    {
                        PathFilterComponent.Directory =>
                            item.FilePaths.SelectMany(path =>
                                Path.GetDirectoryName(path)
                                    .Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar })
                                )
                                .Any(subDir => stringFilter(subDir, filter, compMode, caseMode)),
                        PathFilterComponent.FileName =>
                            item.FilePaths.Any(file =>
                                stringFilter(Path.GetFileName(file), filter, compMode, caseMode)),
                        PathFilterComponent.Path =>
                            item.FilePaths.Any(file =>
                                stringFilter(file, filter, compMode, caseMode)
                            )
                        ,
                        _ => throw new NotImplementedException($"{nameof(ItemFilterViewModel)}.path filter component {filterComp} is not implemented yet")
                    }
                );

        }


        public ItemFilterViewModel(IManualTagEditor tagEditor)
        {
            this.FilterChanges = this.WhenAnyValue(
                vm => vm.FilterMode,
                vm => vm.NameFilter,
                vm => vm.NameFilterMode,
                vm => vm.NameCaseSensitivity,
                vm => vm.PathFilter,
                vm => vm.PathFilterMode,
                vm => vm.PathFilterComponent,
                vm => vm.PathCaseSensitivity,
                (
                    filterMode,
                    nameFilter,
                    nameFilterMode,
                    nameCaseSensitivity,
                    pathFilter,
                    pathFilterMode,
                    pathFilterComponent,
                    pathCaseSensitivity
                ) =>
                {
                    return new
                    {
                        filterMode,
                        nameFilter,
                        nameFilterMode,
                        nameCaseSensitivity,
                        pathFilter,
                        pathFilterMode,
                        pathFilterComponent,
                        pathCaseSensitivity
                    };
                })
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Select(filters =>
                    new Func<ItemEx, bool>((ItemEx item) =>
                            stringFilter(item.Name.ToString(), filters.nameFilter, filters.nameFilterMode, filters.nameCaseSensitivity, filters.filterMode) &&
                            filePathFilter(item, filters.pathFilter, filters.pathFilterComponent, filters.pathFilterMode, filters.pathCaseSensitivity, filters.filterMode)
                    )
                );
        }
    }
}
