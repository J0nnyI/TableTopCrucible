using System.Reactive.Subjects;
using TableTopCrucible.Base.Models.Sources;
using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemEditorViewModel : DisposableReactiveValidationObject<ItemEditorViewModel>
    {
        public BehaviorSubject<Item?> ItemChanges = new BehaviorSubject<Item?>(null);

        public ItemEditorViewModel()
        {

        }
    }
}
