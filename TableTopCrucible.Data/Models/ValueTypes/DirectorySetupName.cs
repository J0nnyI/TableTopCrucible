using System;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Core.Models.ValueTypes;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct DirectorySetupName : IComparable
    {
        private string _name { get; }
        public DirectorySetupName(string name)
        {
            var errors = Validate(name);
            if (errors.Any())
                throw new Exception($"could not create directoryName {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            this._name = name;
        }
        public override string ToString()
            => this._name;
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case string str:
                    return this._name == str;
                case DirectorySetupName name:
                    return this._name == name._name;
                default:
                    return false;
            }
        }
        public override int GetHashCode()
            => _name.GetHashCode();
        public static explicit operator DirectorySetupName(string text)
            => new DirectorySetupName(text);
        public static explicit operator string(DirectorySetupName name)
            => name._name;

        public static IEnumerable<string> Validate(string tag)
        {
            return Validators
                .Where(x => !x.IsValid(tag))
                .Select(x => x.Message)
                .ToArray();
        }

        public int CompareTo(object obj)
        {
            if (obj is DirectorySetupName otherName)
                return this._name.CompareTo(otherName._name);
            else return -1;
        }

        public static IEnumerable<Validator<string>> Validators { get; } = new Validator<string>[] {
            new Validator<string>(directoryName=>!string.IsNullOrWhiteSpace(directoryName),"The name must not be empty")
        };
    }
}
