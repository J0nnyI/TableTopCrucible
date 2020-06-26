using System.Windows.Threading;

namespace TableTopCrucible.Domain.Services
{
    public interface IUiDispatcherService
    {
        Dispatcher UiDispatcher { get; }
    }
}
