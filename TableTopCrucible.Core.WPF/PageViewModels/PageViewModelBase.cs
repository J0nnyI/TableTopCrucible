using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

using TableTopCrucible.Core.Models.Sources;

namespace TableTopCrucible.Core.WPF.PageViewModels
{
    public abstract class PageViewModelBase : DisposableReactiveObjectBase
    {
        public string Title { get;}
        public PackIconKind ?Icon { get; }
        public PageViewModelBase(string title, PackIconKind? icon = null)
        {
            this.Title = title;
            this.Icon = icon;
        }
    }
}
