using System.Windows.Threading;

using TableTopCrucible.Core.Services;

namespace TableTopCrucible.App.WPF
{
    public class UiDispatcherService : IUiDispatcherService
    {
        public Dispatcher UiDispatcher => App.Current.Dispatcher;
    }
}
