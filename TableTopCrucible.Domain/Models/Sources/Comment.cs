using System;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Windows.Automation;
using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public struct Comment : IEntity<CommentId>
    {
        /// <summary>
        /// this constructor creates an entirely new entity
        /// </summary>
        /// <param name="text"></param>
        /// <param name="userId"></param>
        internal Comment(string text, UserId? userId = null)
        {
            this.Text = text ?? throw new ArgumentNullException(nameof(text));
            this.UserId = userId;
            this.Id = (CommentId)Guid.NewGuid();
            this.LastChange = DateTime.Now;
            this.Created = DateTime.Now;
        }
        /// <summary>
        /// this constructor creates an update-entity
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="text"></param>
        /// <param name="userId"></param>
        internal Comment(Comment origin, string text, UserId? userId = null)
        {
            this.Text = text ?? throw new ArgumentNullException(nameof(text));
            this.UserId = userId;
            this.Id = origin.Id;
            this.LastChange = DateTime.Now;
            this.Created = origin.Created;
        }

        public CommentId Id { get; }
        public string Text { get; }
        public UserId? UserId { get; }
        public DateTime Created { get; }
        public DateTime LastChange { get; }

        Guid IEntity.Id => (Guid)this.Id;


    }
}
