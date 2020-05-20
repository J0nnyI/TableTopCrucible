using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.ValueTypes;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.WPF.ViewModels
{
    public class TagListViewModel : ValidatingViewModelBase
    {
        private string _newTag;
        public string NewTag
        {
            get => _newTag;
            set => onPropertyChange(value, ref _newTag);
        }

        private bool _isEditmode;
        public bool IsEditmode
        {
            get => _isEditmode;
            set => onPropertyChange(value, ref _isEditmode);
        }

        private bool _readOnly;
        public bool ReadOnly
        {
            get => _readOnly;
            set => this.onPropertyChange(value, ref _readOnly);
        }


        public BehaviorSubject<IEnumerable<Tag>> Tags { get; } = new BehaviorSubject<IEnumerable<Tag>>(null);

        public RelayCommand EnterEditmode { get; }
        public RelayCommand LeaveEditmode { get; }
        public RelayCommand AddTag { get; }

        public TagListViewModel(IItemTagService tagService)
        {
            this.EnterEditmode = new RelayCommand((_) => this.IsEditmode = true, (_) => !this.IsEditmode);
            this.LeaveEditmode= new RelayCommand((_) => this.IsEditmode = false, (_) => this.IsEditmode);
            this.AddTag = new RelayCommand((_) => this.addTag(), (_) => this.ErrorList.Any());

            //tagService.Get()
        }
        private void addTag()
        {

        }
    }
}
