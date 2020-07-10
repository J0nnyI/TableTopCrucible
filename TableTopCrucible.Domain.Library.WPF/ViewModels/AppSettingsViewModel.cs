using DynamicData.Binding;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

using TableTopCrucible.Core.Models.Sources;
using TableTopCrucible.Core.Models.ValueTypes;
using TableTopCrucible.Core.Services;
using TableTopCrucible.WPF.Commands;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{

    public class AppSettingsViewModel : DisposableReactiveValidationObject<AppSettingsViewModel>
    {
        [Reactive]
        public string ThreadCount { get; set; }
        [Reactive]
        public string MaxPatchSize { get; set; }
        private int? _maxPatchSize
        {
            get
            {
                return int.TryParse(MaxPatchSize, out int res) ? res : (int?)null;
            }
        }

        private readonly ISettingsService _settingsService;
        public ICommand Save { get; }

        public AppSettingsViewModel(ISettingsService settingsService)
        {
            this._settingsService = settingsService;

            this.ThreadCount = settingsService.ThreadCount.ToString();

            this.MaxPatchSize = settingsService.MaxPatchSize.ToString();

            this.Save = new RelayCommand(_ => _save(), _ => !this.HasErrors);

            _createMinMaxValidators(1, vm => vm.ThreadCount, 16);
            _createMinMaxValidators(10, vm => vm.MaxPatchSize, 500);
        }

        enum MinMaxParseResult
        {
            tooSmall,
            tooBig,
            NaN,
            fits
        }
        MinMaxParseResult minMaxParser(int min, string value, int max)
        {
            if (!int.TryParse(value, out int number))
                return MinMaxParseResult.NaN;
            if (number < min)
                return MinMaxParseResult.tooSmall;
            if (number > max)
                return MinMaxParseResult.tooBig;
            return MinMaxParseResult.fits;
        }
        void _createMinMaxValidators(int min, Expression<Func<AppSettingsViewModel, string>> exp, int max)
        {
            this.ValidationRule(exp, str =>
                {
                    var res = minMaxParser(min, str, max);
                    return res != MinMaxParseResult.tooBig && res != MinMaxParseResult.tooSmall;
                },
                $"The value has to be between {min} and {max}");

            this.ValidationRule(exp,
                str => minMaxParser(min, str, max) != MinMaxParseResult.NaN,
                $"This is not a valid number");
        }


        private void _save()
        {
            _settingsService.ThreadCount = int.Parse(this.ThreadCount);
            _settingsService.MaxPatchSize = int.Parse(this.MaxPatchSize);
            _settingsService.Save();
        }
    }
}
