using System;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Core.Models.ValueTypes;

namespace TableTopCrucible.Data.Models.ValueTypes
{
    public struct Tag : IComparable
    {
        private readonly string _tag;
        public Tag(string tag)
        {
            var errors = Validate(tag);
            if (errors.Any())
                throw new Exception($"could not create tag {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            _tag = tag.Trim();
        }
        public override string ToString() => _tag;
        public override bool Equals(object obj)
        {
            return obj switch
            {
                Tag tag => _tag == tag._tag,
                string tag => _tag == tag,
                _ => false,
            };
        }
        public override int GetHashCode() => _tag.GetHashCode();
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

        public int CompareTo(object obj)
            => obj is Tag otherTag ? _tag.CompareTo(otherTag._tag) : 1;

        public static IEnumerable<Validator<string>> Validators { get; } = new Validator<string>[] {
            new Validator<string>(tag=>!string.IsNullOrWhiteSpace(tag),"The tag must not be empty")
        };

    }
}
