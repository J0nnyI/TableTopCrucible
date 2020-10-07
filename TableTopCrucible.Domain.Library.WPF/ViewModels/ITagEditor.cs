using DynamicData;

using TableTopCrucible.Data.Models.ValueTypes;

namespace TableTopCrucible.Domain.Library.WPF.ViewModels
{
    public interface ITagEditor
    {
        bool Editmode { get; set; }
        bool PermitNewTags { get; set; }
        bool CompletePool { get; set; }

        IObservableList<Tag> Selection { get; }
        IObservableList<Tag> Tagpool { get; }

        void SetTagpool(IObservableList<Tag> tagpool);
        void Select(Tag tag);
    }
}
