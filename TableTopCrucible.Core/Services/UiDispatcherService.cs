using System.Windows.Threading;

namespace TableTopCrucible.Core.Services
{
    public interface IUiDispatcherService
    {
        Dispatcher UiDispatcher { get; }
    }
}
