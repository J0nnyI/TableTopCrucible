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

    public class SychronizeFilesCommand : ICommand
    {
        private readonly IFileInfoService _fileInfoService;
        public SychronizeFilesCommand(IFileInfoService fileInfoService)
        {
            this._fileInfoService = fileInfoService;
        }

        public event EventHandler CanExecuteChanged;
        public event EventHandler<ItemCreatedEventArgs> ItemCreated;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter)
        {
            this._fileInfoService.Synchronize();
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
