using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
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


        [Reactive]
        public IEnumerable<Tag> Tags { get; set; }

        public RelayCommand EnterEditmode { get; }
        public RelayCommand LeaveEditmode { get; }
        public RelayCommand AddTag { get; }

        public TagEditorViewModel(IItemTagService tagService)
        {
            this.EnterEditmode = new RelayCommand((_) => this.IsEditmode = true, (_) => !this.IsEditmode);
            this.LeaveEditmode = new RelayCommand((_) => this.IsEditmode = false, (_) => this.IsEditmode);
            //this.AddTag = new RelayCommand((_) => this.addTag(), (_) => this.ErrorList.Any());


            foreach (Validator<string> validator in Tag.Validators)
            {
                this.ValidationRule(
                    vm => vm.NewTag,
                    name => validator.IsValid(name),
                    validator.Message).DisposeWith(disposables);
            }
        }
    }
}
