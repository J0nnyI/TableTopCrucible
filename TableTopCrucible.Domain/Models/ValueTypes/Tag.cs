using System;
using System.Collections.Generic;
using System.Linq;

namespace TableTopCrucible.Domain.Models.ValueTypes
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
            return obj switch
            {
                Tag tag => this._tag == tag._tag,
                string tag => this._tag == tag,
                _ => false,
            };
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
            new Validator<string>(tag=>!string.IsNullOrWhiteSpace(tag),"The tag must not be empty")
        };

    }
}
