using System.Collections.Generic;
using System.Text;
using TableTopCrucible.Domain.Models.ValueTypes;

namespace TableTopCrucible.Data.SaveFile.DataTransferObjects
{
    public class VersionDTO
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public VersionDTO(Version source)
        {
            this.Major = source.Major;
            this.Minor = source.Minor;
            this.Patch = source.Patch;
        }
        public VersionDTO()
        {

        }
        public Version ToEntity()
            => new Version(Major, Minor, Patch);
    }
}
