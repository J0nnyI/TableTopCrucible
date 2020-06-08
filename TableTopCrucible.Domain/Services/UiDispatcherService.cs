using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Windows.Threading;

namespace TableTopCrucible.Domain.Services
{
    public interface IUiDispatcherService
    {
        Dispatcher UiDispatcher { get; }
    }
}
