using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Abstractions;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

using TableTopCrucible.Domain.Models.Sources;
using TableTopCrucible.Domain.Services;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.WPF.Commands
{
    public struct ItemCreatedEventArgs
    {
        public ItemCreatedEventArgs(Item item)
        {
            this.Item = item;
        }

        public Item Item { get; }
    }

    public class CreateItemCommand : ICommand
    {
        private readonly IItemService _itemService;
        public CreateItemCommand(IItemService itemService)
        {
            this._itemService = itemService;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<ItemCreatedEventArgs> ItemCreated;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            var item = new ItemChangeset()
            {
                Name = "new Item",
                Thumbnail = @"https://i.etsystatic.com/19002916/r/il/617e49/1885302518/il_fullxfull.1885302518_9ovo.jpg",
                Tags = new Tag[] { (Tag)"new" }
            };
            var entity = this._itemService.Patch(item);
            this.ItemCreated?.Invoke(this, new ItemCreatedEventArgs(entity));
        }
    }
}






















//public class SampleViewModel : ReactiveObject, IValidatableViewModel
//{
//    // Initialize validation context that manages reactive validations.
//    public ValidationContext ValidationContext { get; } = new ValidationContext(this);

//    // Declare a separate validator for complex rule.
//    public ValidationHelper ComplexRule { get; }

//    // Declare a separate validator for age rule.
//    public ValidationHelper AgeRule { get; }

//    [Reactive] public int Age { get; set; }

//    [Reactive] public string Name { get; set; }

//    public ReactiveCommand<Unit, Unit> Save { get; }

//    public SampleViewModel()
//    {
//        // Name must be at least 3 chars. The selector is the property 
//        // name and the line below is a single property validator.
//        this.ValidationRule(
//            viewModel => viewModel.Name,
//            name => !string.IsNullOrWhiteSpace(name),
//            "You must specify a valid name");

//        // Age must be between 13 and 100, message includes the silly 
//        // length being passed in, stored in a property of the ViewModel.
//        AgeRule = this.ValidationRule(
//            viewModel => viewModel.Age,
//            age => age >= 13 && age <= 100,
//            age => $"{age} is a silly age");

//        var nameAndAgeValid = this
//            .WhenAnyValue(x => x.Age, x => x.Name, (age, name) => new { Age = age, Name = name })
//            .Select(x => x.Age > 10 && !string.IsNullOrEmpty(x.Name));

//        // Create a rule from an IObservable.
//        ComplexRule = this.ValidationRule(
//            _ => nameAndAgeValid,
//            (vm, state) => !state ? "That's a ridiculous name / age combination" : string.Empty);

//        // IsValid extension method returns true when all validations succeed.
//        var canSave = this.IsValid();

//        // Save command is only active when all validators are valid.
//        Save = ReactiveCommand.CreateFromTask(async unit => { }, canSave);
//    }

//}
