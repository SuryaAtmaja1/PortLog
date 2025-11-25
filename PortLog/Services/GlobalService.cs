using System;
using System.Collections.Generic;

namespace PortLog.Services
{
    public static class GlobalServices
    {
        private static readonly Dictionary<Type, object> _services = new();

        public static void Register<T>(T service)
        {
            _services[typeof(T)] = service!;
        }

        public static T Get<T>()
        {
            return (T)_services[typeof(T)];
        }

        public static void Init()
        {
            // Register any static/global app-wide services here.
        }
    }
}
