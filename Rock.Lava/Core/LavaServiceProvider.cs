// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// A simple service container that is capable of providing instances of Lava services.
    /// </summary>
    /// <remarks>
    /// In the future, this container should be reimplemented using the Microsoft.Extensions.DependencyInjection library.
    /// </remarks>
    public class LavaServiceProvider
    {
        private Dictionary<Type, Func<Type, object, ILavaService>> _services = new Dictionary<Type, Func<Type, object, ILavaService>>();
        private readonly object _servicesWriteLock = new object();

        /// <summary>
        /// Register a Lava service component with the specified factory method.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public void RegisterService<TService>( Func<Type, object, TService> factoryMethod )
            where TService : class, ILavaService
        {
            RegisterServiceInternal( typeof( TService ), factoryMethod );
        }

        /// <summary>
        /// Register a Lava service component with the specified factory method.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="factoryMethod"></param>
        /// <returns></returns>
        public void RegisterService( Type serviceType, Func<Type, object, ILavaService> factoryMethod )
        {
            RegisterServiceInternal( serviceType, factoryMethod );
        }

        /// <summary>
        /// Adds or replaces a service registration using a copy-on-write strategy
        /// so that concurrent readers always observe a fully populated dictionary.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="factoryMethod"></param>
        private void RegisterServiceInternal( Type serviceType, Func<Type, object, ILavaService> factoryMethod )
        {
            lock ( _servicesWriteLock )
            {
                var newServices = new Dictionary<Type, Func<Type, object, ILavaService>>( _services );
                newServices[serviceType] = factoryMethod;
                _services = newServices;
            }
        }

        /// <summary>
        /// Get an instance of a Lava service component of the specified type.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public TService GetService<TService>()
            where TService : class, ILavaService
        {
            var service = GetService( typeof( TService ) ) as TService;

            return service;
        }

        /// <summary>
        /// Get an instance of a Lava service component of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public ILavaService GetService( Type serviceType, object configuration = null )
        {
            var services = _services;

            if ( !services.TryGetValue( serviceType, out var factoryFunc ) || factoryFunc == null )
            {
                throw new LavaException( $"GetService failed. The service type \"{ serviceType.FullName }\" is not registered." );
            }

            // Create a new instance of the service using the provided configuration object.
            var service = factoryFunc( serviceType, configuration );

            if ( service == null )
            {
                throw new LavaException( $"GetService failed. The service type \"{ serviceType.FullName }\" could not be created." );
            }

            return service;
        }
    }
}