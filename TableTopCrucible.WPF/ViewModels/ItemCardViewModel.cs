using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using TableTopCrucible.Domain.Models.Sources;

namespace TableTopCrucible.WPF.ViewModels
{
    public class ItemCardViewModel : ReactiveObject
    {
        public SubjectBase<Item?> ItemChanges { get; } = new BehaviorSubject<Item?>(null);
        private readonly ObservableAsPropertyHelper<Item?> _item;
        public Item? Item 
            => _item.Value;

        public ItemCardViewModel()
        {
            this._item = ItemChanges.ToProperty(this, nameof(Item));
        }
    }
}
