using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Text;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.WPF.PageViewModels;

namespace TableTopCrucible.Core.WPF.Services
{

    public class TabService : DisposableReactiveObjectBase
    {
        ObservableCollection<PageViewModelBase> _tabs = new ObservableCollection<PageViewModelBase>();
        public ReadOnlyObservableCollection<PageViewModelBase> Tabs;
        BehaviorSubject<int> _activeTabIndex = new BehaviorSubject<int>(0);
        public IObservable<int> ActiveTabIndex => _activeTabIndex;

        public TabService()
        {
            Tabs = new ReadOnlyObservableCollection<PageViewModelBase>(_tabs);
        }

        public void Append(PageViewModelBase content, bool select = true)
        {
            _tabs.Add(content);
            if (select)
                SetTabIndex(_tabs.Count - 1);
        }
        public void SetTabIndex(int index)
        {
            if (index >= _tabs.Count  || index < 0)
                throw new IndexOutOfRangeException();

            this._activeTabIndex.OnNext(index);
        }
    }
}
