using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace TableTopCrucible.Domain.Services
{
    public interface IInjectionProviderService
    {
        ISubject<IServiceProvider> Provider { get; }
    }

}
