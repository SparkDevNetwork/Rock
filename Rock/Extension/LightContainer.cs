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

using Microsoft.Extensions.DependencyInjection;

using Rock.Data;
using Rock.Model;
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
        /// The service provider that will be used when constructing components.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        private readonly LightComponentLoader<TComponent> _componentLoader;

        /// <summary>
        /// The factory to create new <see cref="RockContext"/> instances when
        /// we need them.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LightContainer{TComponent}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider that will be used when constructing components.</param>
        public LightContainer( IServiceProvider serviceProvider )
        {
            _serviceProvider = serviceProvider;
            _rockContextFactory = serviceProvider.GetRequiredService<IRockContextFactory>();
            _componentLoader = serviceProvider.GetRequiredService<LightComponentLoader<TComponent>>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the component with the matching Entity Type
        /// unique identifier.
        /// </summary>
        /// <param name="entityTypeGuid">The unique identifier of the entity type representing the component.</param>
        /// <returns>A component instance of <typeparamref name="TComponent"/> or <c>null</c>.</returns>
        public TComponent CreateInstance( Guid entityTypeGuid )
        {
            if ( !_componentLoader.TypesByGuid.TryGetValue( entityTypeGuid, out var type ) )
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
        /// <see cref="RegisterComponents()"/> method first to
        /// register all components in the <see cref="Model.EntityType"/> table.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The context to use when reading data from the database.</param>
        /// <returns>An enumeration of <see cref="EntityTypeCache"/> objects that represent the registered components.</returns>
        public IEnumerable<EntityTypeCache> GetComponentTypes( RockContext rockContext )
        {
            foreach ( var kvp in _componentLoader.TypesByGuid )
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
        public void RegisterComponents()
        {
            using ( var rockContext = _rockContextFactory.CreateRockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                var entityTypeGuids = _componentLoader.TypesByGuid.Keys.ToList();

                var entityTypes = entityTypeService.Queryable()
                    .ToList();

                foreach ( var kvp in _componentLoader.TypesByGuid )
                {
                    var entityType = entityTypes.FirstOrDefault( et => et.Guid == kvp.Key );

                    if ( entityType == null )
                    {
                        // If the entity type was not found by guid, try to find
                        // it by class name. But don't match one that is associated
                        // with another guid we already know. This is designed to
                        // catch cases where the class was registered without a well
                        // known guid, and now has one.
                        entityType = entityTypes.FirstOrDefault( et => et.Name == kvp.Value.FullName
                            && !entityTypeGuids.Contains( et.Guid ) );
                    }

                    // If still not found, create a new one.
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
        }

        #endregion
    }
}
