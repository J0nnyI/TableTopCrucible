using System;
using System.Collections.Generic;
using System.Linq;

using TableTopCrucible.Domain.ValueTypes.IDs;

namespace TableTopCrucible.Domain.Models.Sources
{
    public class InvalidChangesetException : Exception
    {
        public IEnumerable<string> ErrorList { get; }
        public InvalidChangesetException(IEnumerable<string> errorList)
        {
            this.ErrorList = errorList;

        }
    }


    public interface ICommentChangeset : IEntityChangeset<Comment, CommentId>
    {
        string Text { get; set; }
        UserId? UserId { get; set; }
    }

    public class CommentChangeset : EntityChangesetBase<Comment, CommentId>, ICommentChangeset
    {
        private string _text;
        public string Text
        {
            get => getValue(_text, Origin?.Text);
            set => this.setValue(value, ref _text, Origin?.Text);
        }
        private UserId? _userId;
        public UserId? UserId
        {
            get => getValue(_userId, Origin.Value.UserId);
            set => this.setValue(value, ref _userId, Origin?.UserId);
        }

        public CommentChangeset(Comment? origin = null) : base(origin)
        {
        }

        public override Comment Apply()
            => new Comment(this.Origin.Value, this.Text, this.UserId);
        public override Comment ToEntity()
            => new Comment(this.Text, this.UserId);
        private void Guard()
        {
            var errors = this.GetErrors();
            if (errors.Any())
                throw new InvalidChangesetException(errors);
        }
        public override IEnumerable<string> GetErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(this.Text))
                errors.Add("Text must not be empty");

            return errors;
        }
    }
}
