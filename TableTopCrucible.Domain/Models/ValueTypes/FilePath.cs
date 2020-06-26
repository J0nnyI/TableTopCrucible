using System;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Core.Models.ValueTypes;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct FilePath
    {
        public Uri RootPath { get; }
        public Uri Path { get; }

        public FilePath(Uri rootPath, Uri relativePath)
        {
            this.RootPath = rootPath;
            this.Path = relativePath.IsAbsoluteUri
                ? rootPath.MakeRelativeUri(relativePath)
                : relativePath;
            this._asAbsolute = null;
        }
        private string _asAbsolute;
        public string AsAbsolute
        {
            get
            {
                if (this._asAbsolute == null)
                    this._asAbsolute = this.RootPath.MakeRelativeUri(Path).LocalPath;
                return _asAbsolute;
            }
        }
        public override string ToString()
            => this.Path.ToString();
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case string str:
                    return this.Path == new Uri(str);
                case Uri uri:
                    return this.Path == uri;
                case FilePath path:
                    return this.Path == path.Path;
                default:
                    return false;
            }
        }
        public override int GetHashCode()
            => Path.GetHashCode();

        public static IEnumerable<string> Validate(string tag)
        {
            return Validators
                .Where(x => !x.IsValid(tag))
                .Select(x => x.Message)
                .ToArray();
        }
        public static IEnumerable<Validator<string>> Validators { get; } = new Validator<string>[] {
            new Validator<string>(directoryPath=>!string.IsNullOrWhiteSpace(directoryPath),"The path must not be empty")
        };
    }
}
