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
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.Extension
{
    /// <summary>
    /// A container for components that are simple C# classes to handle the
    /// logic. These components do not support attribute values. They may
    /// support <see cref="Security.ISecured"/> if the component base class
    /// implements the interface itself.
    /// </summary>
    /// <remarks>
    /// Containers must be registered with DI in order to be used as they
    /// can use additional services from our DI provider.
    /// </remarks>
    /// <typeparam name="TComponent">The type of components that will be handled by this container.</typeparam>
    internal class LightContainer<TComponent>
        where TComponent : LightComponent
    {
        #region Fields

        /// <summary>
        /// The dictionary of component types that have been detected in the
        /// loaded assemblies. This will only include types that are available
        /// to this running instance.
        /// </summary>
        private readonly Lazy<IReadOnlyDictionary<Guid, Type>> _typesByGuid = new Lazy<IReadOnlyDictionary<Guid, Type>>( LoadTypes );

        /// <summary>
        /// The service provider that will be used when constructing components.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LightContainer{TComponent}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider that will be used when constructing components.</param>
        public LightContainer( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load the types using reflection. This is called lazily so that
        /// we don't waste CPU cycles at startup until we actually need them.
        /// The components must be decorated with an <see cref="EntityTypeGuidAttribute"/>
        /// in order to be included in the list.
        /// </summary>
        /// <returns>A dictionary of component types with the key being the component unique identifier.</returns>
        private static IReadOnlyDictionary<Guid, Type> LoadTypes()
        {
            try
            {
                var sortedTypes = Reflection.FindTypes( typeof( TComponent ) );
                var typesByGuid = new Dictionary<Guid, Type>();

                foreach ( var type in sortedTypes.Values )
                {
                    var entityTypeGuidAttribute = type.GetCustomAttribute<EntityTypeGuidAttribute>();

                    if ( entityTypeGuidAttribute == null )
                    {
                        continue;
                    }

                    typesByGuid.TryAdd( entityTypeGuidAttribute.Guid, type );
                }

                return typesByGuid;
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Error loading entity container types: {ex.Message}" );

                return new Dictionary<Guid, Type>();
            }
        }

        /// <summary>
        /// Creates an instance of the component with the matching Entity Type
        /// unique identifier.
        /// </summary>
        /// <param name="entityTypeGuid">The unique identifier of the entity type representing the component.</param>
        /// <returns>A component instance of <typeparamref name="TComponent"/> or <c>null</c>.</returns>
        public TComponent CreateInstance( Guid entityTypeGuid )
        {
            if ( !_typesByGuid.Value.TryGetValue( entityTypeGuid, out var type ) )
            {
                return default;
            }

            return ActivatorUtilities.CreateInstance( _serviceProvider, type ) as TComponent;
        }

        /// <summary>
        /// <para>
        /// Gets the <see cref="EntityTypeCache"/> objects that represent the
        /// registered components. This is normally used to display the list
        /// of components in the UI.
        /// </para>
        /// <para>
        /// If you need to ensure that all components are returned, call the
        /// <see cref="RegisterComponents(RockContext)"/> method first to
        /// register all components in the <see cref="Model.EntityType"/> table.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>An enumeration of <see cref="EntityTypeCache"/> objects that represent the registered components.</returns>
        public IEnumerable<EntityTypeCache> GetComponentTypes( RockContext rockContext )
        {
            foreach ( var kvp in _typesByGuid.Value )
            {
                var entityType = EntityTypeCache.Get( kvp.Key, rockContext );

                if ( entityType != null )
                {
                    yield return entityType;
                }
            }
        }

        /// <summary>
        /// <para>
        /// Registers all components in the database. This creates or updates
        /// the <see cref="Model.EntityType"/> records that will represent these
        /// components.
        /// </para>
        /// <para>
        /// If the component C# class is decorated with a <c>[DisplayName]</c>
        /// attribute then that will be used for the friendly name of the component.
        /// Otherwise the class name will be split into words and used as the
        /// friendly name.
        /// </para>
        /// <para>
        /// C# classes must be decorated with an <c>[EntityTypeGuid]</c> attribute
        /// to identify the unique identifier of the component otherwise it will
        /// not be registered.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        public void RegisterComponents( RockContext rockContext )
        {
            var entityTypeService = new EntityTypeService( rockContext );
            var entityTypeGuids = _typesByGuid.Value.Keys.ToList();

            var entityTypes = entityTypeService.Queryable()
                .Where( et => entityTypeGuids.Contains( et.Guid ) )
                .ToList();

            foreach ( var kvp in _typesByGuid.Value )
            {
                var entityType = entityTypes.FirstOrDefault( et => et.Guid == kvp.Key );

                if ( entityType == null )
                {
                    entityType = new Model.EntityType();
                    entityTypeService.Add( entityType );
                }

                entityType.Name = kvp.Value.FullName;
                entityType.FriendlyName = Reflection.GetDisplayName( kvp.Value )
                    ?? kvp.Value.Name.SplitCase();
                entityType.AssemblyName = kvp.Value.AssemblyQualifiedName;
                entityType.IsEntity = false;
                entityType.IsSecured = typeof( Security.ISecured ).IsAssignableFrom( kvp.Value );
                entityType.Guid = kvp.Key;
            }

            rockContext.SaveChanges();
        }

        #endregion
    }

}