using System;

namespace TableTopCrucible.Core.Models.ValueTypes
{
    public struct Validator<T>
    {
        public Func<T, bool> IsValid { get; }
        public string Message { get; }

        public Validator(Func<T, bool> isValid, string error)
        {
            this.IsValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
            this.Message = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
