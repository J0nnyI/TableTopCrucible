using System;
using System.Reactive.Subjects;

namespace TableTopCrucible.Domain.Services
{
    public interface IInjectionProviderService
    {
        ISubject<IServiceProvider> Provider { get; }
    }

}
