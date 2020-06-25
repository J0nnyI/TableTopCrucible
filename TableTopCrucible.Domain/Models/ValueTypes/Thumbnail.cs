using System;
using System.Collections.Generic;
using System.Linq;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct Thumbnail
    {
        private readonly string _thumbnail;
        public Thumbnail(string thumbnail)
        {
            var errors = Validate(thumbnail);
            if (errors.Any())
                throw new Exception($"could not create thumbnail {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            this._thumbnail = thumbnail;
        }
        public override string ToString() => this._thumbnail;
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case Thumbnail thumbnail:
                    return this._thumbnail == thumbnail._thumbnail;
                case string thumbnail:
                    return this._thumbnail == thumbnail;
                default:
                    return false;
            }
        }
        public override int GetHashCode() => this._thumbnail.GetHashCode();
        public static explicit operator Thumbnail(string thumbnail) => new Thumbnail(thumbnail);
        public static explicit operator Thumbnail?(string thumbnail) => string.IsNullOrWhiteSpace(thumbnail) ? null as Thumbnail? : new Thumbnail(thumbnail);
        public static explicit operator string(Thumbnail thumbnail) => thumbnail._thumbnail;
        public static bool operator ==(Thumbnail thumbnail1, Thumbnail thumbnail2)
        {
            return thumbnail1._thumbnail == thumbnail2._thumbnail;
        }
        public static bool operator !=(Thumbnail thumbnail1, Thumbnail thumbnail2)
        {
            return thumbnail1._thumbnail != thumbnail2._thumbnail;
        }

        public static IEnumerable<string> Validate(string tag)
        {
            return Validators
                .Where(x => !x.IsValid(tag))
                .Select(x => x.Message)
                .ToArray();
        }
        public static IEnumerable<Validator<string>> Validators { get; } = new Validator<string>[] {
            new Validator<string>(thumbnail=>Uri.IsWellFormedUriString(thumbnail, UriKind.RelativeOrAbsolute),
                "the path could not be resolved")
        };
    }
}
