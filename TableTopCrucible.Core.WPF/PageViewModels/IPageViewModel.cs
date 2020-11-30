using MaterialDesignThemes.Wpf;

namespace TableTopCrucible.Core.WPF.PageViewModels
{
    public interface IPageViewModel
    {
        PackIconKind? Icon { get; }
        string Title { get; }
    }
}