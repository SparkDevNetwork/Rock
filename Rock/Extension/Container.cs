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
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Rock.Extension
{
    /// <summary>
    /// Singleton generic class that uses MEF to load and cache all of the component classes
    /// </summary>
    public abstract class Container<T, TData> : IContainer, IDisposable
        where T : Component
        where TData : IComponentData
    {
        // MEF Container
        private CompositionContainer container;
        private bool IsDisposed;

        /// <summary>
        /// Gets the components.
        /// </summary>
        public Dictionary<int, Lazy<T, TData>> Components { get; private set; }

        /// <summary>
        /// Gets the component names and their attributes
        /// </summary>
        public Dictionary<int, KeyValuePair<string, Component>> Dictionary
        {
            get
            {
                var dictionary = new Dictionary<int, KeyValuePair<string, Component>>();
                foreach ( var component in Components )
                {
                    dictionary.Add( component.Key, new KeyValuePair<string, Component>(
                        component.Value.Metadata.ComponentName, component.Value.Value ) );
                }

                return dictionary;
            }
        }

        /// <summary>
        /// Gets or sets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        protected abstract IEnumerable<Lazy<T, TData>> MEFComponents { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Container()
        {
            IsDisposed = false;
            Refresh();
        }

        /// <summary>
        /// Gets the component.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        protected T GetComponentByEntity( string entityType )
        {
            foreach ( var serviceEntry in this.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.TypeName.Equals( entityType, StringComparison.OrdinalIgnoreCase ) ||
                    component.TypeGuid.ToString().Equals( entityType, StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( component.IsActive )
                    {
                        return component;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        protected string GetComponentNameByEntity( string entityType )
        {
            foreach ( var serviceEntry in this.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.TypeName.Equals( entityType, StringComparison.OrdinalIgnoreCase ) ||
                    component.TypeGuid.ToString().Equals( entityType, StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( component.IsActive )
                    {
                        return serviceEntry.Value.Metadata.ComponentName;
                    }
                    else
                    {
                        //break;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Forces a reloading of all the components
        /// </summary>
        public virtual void Refresh()
        {
            Components = new Dictionary<int, Lazy<T, TData>>();

            // Create the MEF Catalog
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add( new SafeDirectoryCatalog( typeof( T ) ) );

            // Create the container from the catalog
            container = new CompositionContainer( catalog );

            // Compose the MEF container with any classes that export the same definition
            container.ComposeParts( this );

            // Create a temporary sorted dictionary of the classes so that they can be executed in a specific order
            var components = new SortedDictionary<int, List<Lazy<T, TData>>>();
            foreach ( Lazy<T, TData> i in MEFComponents )
            {
                if ( !components.ContainsKey( i.Value.Order ) )
                {
                    components.Add( i.Value.Order, new List<Lazy<T, TData>>() );
                }

                components[i.Value.Order].Add( i );
            }

            // Add each class found through MEF into the Services property value in the correct order
            int id = 0;
            foreach ( KeyValuePair<int, List<Lazy<T, TData>>> entry in components )
            {
                foreach ( Lazy<T, TData> component in entry.Value )
                {
                    Components.Add( id++, component );
                }
            }
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose( bool disposing )
        {
            if ( !IsDisposed )
            {
                if ( disposing )
                {
                    if ( container != null )
                        container.Dispose();
                }

                container = null;
                IsDisposed = true;
            }
        }
    }
}