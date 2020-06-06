using DynamicData;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

using TableTopCrucible.Domain.Models;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Services;
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

        private readonly BehaviorSubject<IEnumerable<Tag>> SelectedTagsChanges = new BehaviorSubject<IEnumerable<Tag>>(null);
        private readonly ObservableAsPropertyHelper<IEnumerable<Tag>> _selectedTags;
        public IEnumerable<Tag> SelectedTags => _selectedTags.Value;

        private ObservableAsPropertyHelper<IEnumerable<Tag>> _availableTags;
        public IEnumerable<Tag> AvailableTags => _availableTags.Value;

        public ICommand AddTag { get; }
        public ICommand SelectedTagsChanged { get; }
        public ICommand RemoveTags { get; }

        public TagEditorViewModel(IItemTagService tagService)
        {
            #region commands
            this.AddTag = new RelayCommand((tag) => _addTag(tag as string), (tag) => _canAddTag(tag as string));
            this.SelectedTagsChanged = new RelayCommand(items =>
                this.SelectedTagsChanges.OnNext((items as IEnumerable).Cast<Tag>()));
            this.RemoveTags = new RelayCommand(tags => _removeTags(tags as IEnumerable<Tag>), _ => this.SelectedTags?.Any() == true);
            #endregion

            #region observables to property
            this._tags = this.TagsChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(Tags));

            this._availableTags = tagService
                .Get()
                .CombineLatest(TagsChanges,
                    (available, used)
                        => available != null && used != null
                        ? available?.Except(used)
                        : null)
                .TakeUntil(destroy)
                .ToProperty(this, nameof(AvailableTags));

            this._selectedTags = this.SelectedTagsChanges
                .TakeUntil(destroy)
                .ToProperty(this, nameof(SelectedTags));
            #endregion

            #region validations
            this.ValidationRule(
                vm => vm.NewTag,
                tag => !isTagSelected(tag),
                "This tag has already been added")
                .DisposeWith(disposables);

            foreach (Validator<string> validator in Tag.Validators)
            {
                this.ValidationRule(
                    vm => vm.NewTag,
                    name => validator.IsValid(name),
                    validator.Message)
                    .DisposeWith(disposables);
            }
            #endregion
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

        private void _removeTags(IEnumerable<Tag> tags)
        {
            var tagList = this.Tags.ToList();
            tagList.RemoveMany(tags);
            this.Tags = tagList;
        }
        private bool _canAddTag(string tag)
        {
            return !Tag.Validate(tag).Any() && !isTagSelected(tag);
        }
        protected override void OnDispose()
        {
            this.TagsChanges.OnCompleted();
            this.TagsChanges.Dispose();
            this.SelectedTagsChanges.OnCompleted();
            this.SelectedTagsChanges.Dispose();
            base.OnDispose();
        }
    }
}
