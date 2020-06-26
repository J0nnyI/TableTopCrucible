using System;
using System.IO;
using TableTopCrucible.Base.Models.Sources;
using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct DirectorySetup : IEntity<DirectorySetupId>
    {
        public Uri Path { get; }
        public DirectorySetupName Name { get; }
        public Description Description { get; }

        public bool IsValid
            => Path != null && Directory.Exists(Path.LocalPath);

        public Guid Identity { get; }

        public DirectorySetupId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }


        public DirectorySetup(Uri path, DirectorySetupName name, Description description)
            : this(path, name, description, (DirectorySetupId)Guid.NewGuid(), DateTime.Now)
        { }
        public DirectorySetup(DirectorySetup origin, Uri path, DirectorySetupName name, Description description)
            : this(path, name, description, origin.Id, origin.Created)
        { }

        public DirectorySetup(Uri path, DirectorySetupName name, Description description, DirectorySetupId id, DateTime created)
        {
            this.Path = path;
            this.Name = name;
            this.Description = description;

            this.Id = id;
            this.Identity = Guid.NewGuid();
            this.LastChange = DateTime.Now;
            this.Created = created;
        }

        public override string ToString() => $"directory setup {Id} ({Name})";

        public static bool operator ==(DirectorySetup directorySetupA, DirectorySetup directorySetupB)
            => directorySetupA.Identity == directorySetupB.Identity;
        public static bool operator !=(DirectorySetup directorySetupA, DirectorySetup directorySetupB)
            => directorySetupA.Identity != directorySetupB.Identity;

        public override bool Equals(object obj) => obj is DirectorySetup dirSetup && this == dirSetup;
        public override int GetHashCode() => HashCode.Combine(this.Identity);
    }
}
