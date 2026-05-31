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

        /// <summary>
        /// Registers or replaces a global service instance.
        /// </summary>
        public static void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Services[typeof(TService)] = service;
        }

        /// <summary>
        /// Removes a service only when the registered instance matches the supplied instance.
        /// </summary>
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

        /// <summary>
        /// Attempts to resolve a registered service without throwing when it is unavailable.
        /// </summary>
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

        /// <summary>
        /// Resolves a registered service or throws when the service is unavailable.
        /// </summary>
        public static TService Get<TService>() where TService : class
        {
            if (TryGet(out TService service))
            {
                return service;
            }

            throw new InvalidOperationException($"{typeof(TService).Name} is not registered.");
        }

        /// <summary>
        /// Removes every registered service, primarily for tests or play mode reset flows.
        /// </summary>
        public static void Clear()
        {
            Services.Clear();
        }
    }
}
