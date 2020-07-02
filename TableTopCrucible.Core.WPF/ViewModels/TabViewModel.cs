using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Core.WPF.Services;

namespace TableTopCrucible.Core.WPF.ViewModels
{
    public class TabViewModel:DisposableReactiveObjectBase
    {
        private readonly TabService _tabService;

        public ReadOnlyObservableCollection<PageViewModelBase> Tabs { get; }
        private ObservableAsPropertyHelper<int> _activeTabIndex;
        public int ActiveTabIndex
        {
            get => _activeTabIndex.Value;
            set => _tabService.SetTabIndex(value);
        }

        public TabViewModel(TabService tabService)
        {
            this._tabService = tabService;

            Tabs = tabService.Tabs;
            this._activeTabIndex = tabService.ActiveTabIndex
                .ToProperty(this, nameof(ActiveTabIndex))
                .DisposeWith(disposables);

            
        }
    }
}
