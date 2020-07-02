using System;
using System.Reactive.Subjects;

namespace TableTopCrucible.Core.Services
{
    public interface IInjectionProviderService
    {
        ISubject<IServiceProvider> Provider { get; }
    }

}
