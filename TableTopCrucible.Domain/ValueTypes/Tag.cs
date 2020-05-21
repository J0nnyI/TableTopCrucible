﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace TableTopCrucible.Domain.ValueTypes
{
    public struct Tag
    {
        private readonly string _tag;
        public Tag(string tag)
        {
            var errors = Validate(tag);
            if (errors.Any())
                throw new Exception($"could not create tag {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            this._tag = tag.Trim();
        }
        public override string ToString() => this._tag;
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Tag tag:
                    return this._tag == tag._tag;
                case string tag:
                    return this._tag == tag;
                default:
                    return false;
            }
        }
        public override int GetHashCode() => this._tag.GetHashCode();
        public static explicit operator Tag(string tag) => new Tag(tag);
        public static explicit operator string(Tag tag) => tag._tag;
        public static bool operator ==(Tag tag1, Tag tag2)
        {
            return tag1._tag == tag2._tag;
        }
        public static bool operator !=(Tag tag1, Tag tag2)
        {
            return tag1._tag != tag2._tag;
        }
        public static IEnumerable<string> Validate(string tag)
        {
            return Validators
                .Where(x => !x.IsValid(tag))
                .Select(x => x.Message)
                .ToArray();
        }
        public static IEnumerable<Validator<string>> Validators { get; } = new Validator<string>[] { 
            new Validator<string>(tag=>!string.IsNullOrWhiteSpace(tag),"The Tag mustn not be empty")
        };

    }
    public struct Validator<T>
    {
        public Func<T, bool> IsValid { get; }
        public string Message;

        public Validator(Func<T, bool> isValid, string error)
        {
            this.IsValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
            this.Message = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
