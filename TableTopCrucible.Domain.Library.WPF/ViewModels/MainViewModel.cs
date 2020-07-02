using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Domain.Library.WPF.Pages;
using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public class Tab
    {
        [Reactive]
        public Page Page { get; set; }
        [Reactive]
        public bool IsSelected { get; set; }
    }
    public class MainViewModel : DisposableReactiveObjectBase
    {

        [Reactive]
        public Page CurrentPage { get; set; }

        public IEnumerable<Tab> Pages { get; }


        public ICommand TabChangeCommand { get; }

        public MainViewModel(FileSetupPage fileSetup, ItemEditorPage itemEditor, DevTestPage devTest)
        {
            this.Pages =
            new List<Page>
            {
                fileSetup ?? throw new ArgumentNullException(nameof(fileSetup)),
                itemEditor ?? throw new ArgumentNullException(nameof(itemEditor)),
                devTest ?? throw new ArgumentNullException(nameof(devTest))
            }
            .Select(page => new Tab { Page = page })
            .ToArray();

            var firstTab = this.Pages.First();
            firstTab.IsSelected = true;
            this.CurrentPage = firstTab.Page;

            this.TabChangeCommand = new RelayCommand(par =>
            {
                if (par is Tab tab && tab.IsSelected)
                    this.CurrentPage = tab.Page;
            });
        }
    }
}
