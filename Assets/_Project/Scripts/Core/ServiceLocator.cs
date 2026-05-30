using System;
using System.Collections.Generic;

namespace BondiSimulator.Core
{
    /// <summary>
    /// Minimal registry for unavoidable global services that need explicit command-style access.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Services[typeof(TService)] = service;
        }

        public static void Unregister<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                return;
            }

            Type serviceType = typeof(TService);
            if (Services.TryGetValue(serviceType, out object registeredService) && ReferenceEquals(registeredService, service))
            {
                Services.Remove(serviceType);
            }
        }

        public static bool TryGet<TService>(out TService service) where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out object registeredService))
            {
                service = registeredService as TService;
                return service != null;
            }

            service = null;
            return false;
        }

        public static TService Get<TService>() where TService : class
        {
            if (TryGet(out TService service))
            {
                return service;
            }

            throw new InvalidOperationException($"{typeof(TService).Name} is not registered.");
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}
