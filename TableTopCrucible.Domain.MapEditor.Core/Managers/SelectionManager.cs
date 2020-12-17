using ReactiveUI.Fody.Helpers;

using System;
using System.Collections.Generic;
using System.Text;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Data.Models.Views;
using TableTopCrucible.Domain.MapEditor.Core.Models.Views;

namespace TableTopCrucible.Domain.MapEditor.Core.Managers
{
    public interface ISelectionManager
    {

    }
    public class SelectionManager: DisposableReactiveObjectBase,ISelectionManager
    {
        private readonly ICursorManager _cursorManager;

        public SelectionManager(ICursorManager cursorManager)
        {
            _cursorManager = cursorManager;
        }
        [Reactive]
        public ItemEx SelectedItem { get; set; }
        [Reactive]
        public TileManager SelectedTile { get; private set; }
        [Reactive]
        public TileManager HoveredTile { get; private set; }

        public void OnSlotMouseEnter()
        {

        }
        public void OnTileMouseEnter()
        {

        }
        public void OnTileMouseDown()
        {

        }
        public void OnViewportKeyDown()
        {

        }
    }
}
