using HelixToolkit.Wpf;

using MaterialDesignThemes.Wpf;

using System;
using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Core.WPF.PageViewModels;
using TableTopCrucible.Domain.MapEditor.WPF.ViewModels;

namespace TableTopCrucible.Domain.MapEditor.WPF.PageViewModels
{
    public interface IMapEditorPageVm:IPageViewModel
    {

    }
    public class MapEditorPageViewModel : PageViewModelBase, IMapEditorPageVm
    {
        public MapEditorPageViewModel(IMapEditorVm mapEditor) : base("Map Editor", PackIconKind.Map)
        {
            MapEditor = mapEditor;
        }

        public IMapEditorVm MapEditor { get; }
    }
}

