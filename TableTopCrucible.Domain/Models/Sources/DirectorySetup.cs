using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using TableTopCrucible.Domain.Models.ValueTypes;
using TableTopCrucible.Domain.Models.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct DirectorySetup : IEntity<DirectorySetupId>
    {
        public Uri Path { get; }
        public DirectorySetupName Name { get; }
        public Description Description { get; }




        public DirectorySetupId Id { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }


        public DirectorySetup(Uri path, DirectorySetupName name, Description description)
        {
            this.Path = path;
            this.Name = name;
            this.Description = description;

            this.Id = (DirectorySetupId)Guid.NewGuid();
            this.LastChange = DateTime.Now;
            this.Created = DateTime.Now;
        }
        public DirectorySetup(DirectorySetup origin, Uri path, DirectorySetupName name, Description description)
        {
            this.Path = path;
            this.Name = name;
            this.Description = description;
            
            this.Id = origin.Id;
            this.LastChange = DateTime.Now;
            this.Created = origin.Created;
        }
        public override string ToString() => $"directory setup {Id} ({Name})";
    }
}
