using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TableTopCrucible.Domain.Models.ValueTypes
{
    public struct FilePath
    {
        private Uri _rootPath { get; }
        private Uri _path { get; }

        public FilePath(string rootPath, string relativePath)
        {
            this._rootPath = new Uri(rootPath);
            this._path = new Uri(relativePath);
            this._asAbsolute = null;
        }
        private string _asAbsolute;
        public string AsAbsolute
        {
            get
            {
                if (this._asAbsolute == null)
                    this._asAbsolute = this._rootPath.MakeRelativeUri(_path).LocalPath;
                return _asAbsolute;
            }
        }
        public override string ToString()
            => this._path.ToString();
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case string str:
                    return this._path == new Uri(str);
                case Uri uri:
                    return this._path == uri;
                case FilePath path:
                    return this._path == path._path;
                default:
                    return false;
            }
        }
        public override int GetHashCode()
            => _path.GetHashCode();

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
