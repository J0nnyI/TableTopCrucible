using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TableTopCrucible.Domain.ValueTypes
{
    public struct FilePath3d
    {
        private string _path;
        public IEnumerable<string> Errors { get; }
        public static IEnumerable<string> ValidExtensions { get; } = new string[] { ".stl", ".obj" };

        public FilePath3d(string file)
        {
            var errors = new List<string>();
            if (!File.Exists(file))
                setError(ref errors, "File does not exist");
            if (!ValidExtensions.Contains(Path.GetExtension(file)))
                setError(ref errors, "invalid extension");
            this.Errors = errors.ToArray();
            this._path = file ?? throw new ArgumentNullException(nameof(file));

        }
        private static void setError(ref List<string> list, string err)
        {
            if (list == null)
                list = new List<string>();
            list.Add(err);
        }
        public static explicit operator string(FilePath3d path)
            => path._path;
    }
}
