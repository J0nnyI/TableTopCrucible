using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class TagEditorViewModel : DisposableReactiveValidationObject<TagEditorViewModel>
    {
        [Reactive]
        public string NewTag { get; set; }

        [Reactive]
        public bool IsEditmode { get; set; }

        [Reactive]
        public bool IsReadOnly { get; set; }


        public BehaviorSubject<IEnumerable<Tag>> TagsChanges { get; } = new BehaviorSubject<IEnumerable<Tag>>(null);
        private readonly ObservableAsPropertyHelper<IEnumerable<Tag>> _tags;
        public IEnumerable<Tag> Tags
        {
            get => _tags.Value;
            set => TagsChanges.OnNext(value);
        }


        private ObservableAsPropertyHelper<IEnumerable<Tag>> _availableTags;
        public IEnumerable<Tag> AvailableTags => _availableTags.Value;

        public RelayCommand EnterEditmode { get; }
        public RelayCommand LeaveEditmode { get; }
        public RelayCommand AddTag { get; }

        public TagEditorViewModel(IItemTagService tagService)
        {
            this.EnterEditmode = new RelayCommand((_) => this.IsEditmode = true, (_) => !this.IsEditmode);
            this.LeaveEditmode = new RelayCommand((_) => this.IsEditmode = false, (_) => this.IsEditmode);
            this.AddTag = new RelayCommand((tag) => _addTag(tag as string), (tag) => _canAddTag(tag as string));
            //this.AddTag = new RelayCommand((_) => this.addTag(), (_) => this.ErrorList.Any());

            this._tags = this.TagsChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Tags));
            this._availableTags = tagService
                .Get()
                .CombineLatest(TagsChanges, (available, used) => available != null && used != null ? available?.Except(used) : null)
                .TakeUntil(destroy)
                .ToProperty(this, nameof(AvailableTags));

            this.ValidationRule(
                vm => vm.NewTag,
                tag => !isTagSelected(tag),
                "This tag has already been added");

            foreach (Validator<string> validator in Tag.Validators)
            {
                this.ValidationRule(
                    vm => vm.NewTag,
                    name => validator.IsValid(name),
                    validator.Message)
                    .DisposeWith(disposables);
            }
        }
        private void _addTag(string tag)
        {
            var tags = this.TagsChanges.Value.ToList();
            tags.Add((Tag)tag);
            this.NewTag = string.Empty;
            this.TagsChanges.OnNext(tags);
        }
        private bool isTagSelected(string tag)
            => Tags?.Any(curTag => (string)curTag == tag) == true;
        private bool _canAddTag(string tag)
        {
            return !Tag.Validate(tag).Any() && !isTagSelected(tag);
        }

    }
}
