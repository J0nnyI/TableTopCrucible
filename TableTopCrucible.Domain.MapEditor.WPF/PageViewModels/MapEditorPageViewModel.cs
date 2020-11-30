using MaterialDesignThemes.Wpf;

using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Core.WPF.PageViewModels;

namespace TableTopCrucible.Domain.MapEditor.WPF.PageViewModels
{
    public interface IMapEditorPageVm:IPageViewModel
    {

    }
    public class MapEditorPageViewModel : PageViewModelBase, IMapEditorPageVm
    {
        public MapEditorPageViewModel() : base("Map Editor", PackIconKind.Map)
        {
        }
    }
}

