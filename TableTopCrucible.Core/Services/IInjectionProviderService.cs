using System;
using System.Reactive.Subjects;

namespace TableTopCrucible.Core.Services
{
    public interface IInjectionProviderService
    {
        IObservable<IServiceProvider> ProviderChanges { get; }
        IServiceProvider Provider { get; }
    }

}
