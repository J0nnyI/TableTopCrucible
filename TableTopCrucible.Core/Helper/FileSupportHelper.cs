using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TableTopCrucible.Core.Helper
{
    public enum PathType
    {
        Model,
        Image,
        Directory,
        Other
    }
    public static class FileSupportHelper
    {


        public static readonly IEnumerable<string> ModelExtensions = new string[] { ".stl", ".obj", ".off", ".objz", ".lwo", ".3ds" };
        public static readonly IEnumerable<string> ImageExtensions = new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".hdp", ".jp2", ".pbm", ".psd", ".tga", ".tiff", ".img" };

        public static bool IsModel(string path)
            => Path.GetExtension(path).ToLower().IsIn(ModelExtensions.ToArray());
        public static bool IsImage(string path)
            => Path.GetExtension(path).ToLower().IsIn(ImageExtensions.ToArray());
        public static PathType GetPathType(string path)
        {
            if (Path.GetExtension(path).ToLower().IsIn(ModelExtensions.ToArray()))
                return PathType.Model;
            if (Path.GetExtension(path).ToLower().IsIn(ImageExtensions.ToArray()))
                return PathType.Image;
            if (Directory.Exists(path))
                return PathType.Directory;
            return PathType.Other;
        }
    }
}
