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

namespace Rock.Data.Internal
{
    /// <summary>
    /// Provides the entity save hook instances for each type of entity that
    /// is being saved.
    /// </summary>
    internal class EntitySaveHookProvider
    {
        #region Fields

        /// <summary>
        /// The lock is only used when adding or removing a hook from the
        /// provider. Thus the overhead is actually minimal as that should
        /// generally only happen at application startup.
        /// </summary>
        private readonly object _hooksLock = new object();

        /// <summary>
        /// The hooks that have been registered with this provider.
        /// </summary>
        private readonly List<KeyValuePair<Type, Type>> _hooks = new List<KeyValuePair<Type, Type>>();

        /// <summary>
        /// The hooks that have been registered with this provider. This is
        /// the collection that is used during read-only operations to ensure
        /// fast response without the need for a lock.
        /// </summary>
        private Dictionary<Type, List<Type>> _readSafeHooks = new Dictionary<Type, List<Type>>();

        #endregion

        #region Methods

        /// <summary>
        /// Adds the hook into the provider.
        /// </summary>
        /// <param name="entityType">Type of the entity that the hook should be triggered for.</param>
        /// <param name="hookType">Type of the hook that will be triggered.</param>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentOutOfRangeException" />
        public void AddHook( Type entityType, Type hookType )
        {
            if ( entityType == null )
            {
                throw new ArgumentNullException( nameof( entityType ) );
            }

            if ( !typeof( IEntity ).IsAssignableFrom( entityType ) )
            {
                throw new ArgumentOutOfRangeException( nameof( entityType ), $"Must be a type that implements {nameof( IEntity )}." );
            }

            if ( hookType == null )
            {
                throw new ArgumentNullException( nameof( hookType ) );
            }

            if ( !typeof( IEntitySaveHook ).IsAssignableFrom( hookType ) )
            {
                throw new ArgumentOutOfRangeException( nameof( hookType ), $"Must be a type that implements {nameof( IEntitySaveHook )}." );
            }

            var hook = new KeyValuePair<Type, Type>( entityType, hookType );

            lock ( _hooksLock )
            {
                if ( !_hooks.Contains( hook ) )
                {
                    _hooks.Add( hook );

                    RebuildHooks();
                }
            }
        }

        /// <summary>
        /// Removes the hook from the provider.
        /// </summary>
        /// <param name="entityType">Type of the entity that the hook should no longer be triggered for.</param>
        /// <param name="hookType">Type of the hook that will no longer be triggered.</param>
        /// <exception cref="System.ArgumentNullException">
        /// entityType
        /// or
        /// hookType
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// entityType - Must be a type that implements {nameof( IEntity )}.
        /// or
        /// hookType - Must be a type that implements {nameof( IEntitySaveHook )}.
        /// </exception>
        public void RemoveHook( Type entityType, Type hookType )
        {
            if ( entityType == null )
            {
                throw new ArgumentNullException( nameof( entityType ) );
            }

            if ( !typeof( IEntity ).IsAssignableFrom( entityType ) )
            {
                throw new ArgumentOutOfRangeException( nameof( entityType ), $"Must be a type that implements {nameof( IEntity )}." );
            }

            if ( hookType == null )
            {
                throw new ArgumentNullException( nameof( hookType ) );
            }

            if ( !typeof( IEntitySaveHook ).IsAssignableFrom( hookType ) )
            {
                throw new ArgumentOutOfRangeException( nameof( hookType ), $"Must be a type that implements {nameof( IEntitySaveHook )}." );
            }

            var hook = new KeyValuePair<Type, Type>( entityType, hookType );

            lock ( _hooksLock )
            {
                if ( _hooks.Contains( hook ) )
                {
                    _hooks.Remove( hook );

                    RebuildHooks();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IEntitySaveHook"/> instances that should be
        /// executed for entities of the given type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>A collection of <see cref="IEntitySaveHook"/> instances that should be executed.</returns>
        public List<IEntitySaveHook> GetHooksForEntityType( Type entityType )
        {
            var hookList = new List<IEntitySaveHook>();

            // Get all hooks for the class tree.
            for ( var type = entityType; type != null; type = type.BaseType )
            {
                if ( _readSafeHooks.TryGetValue( type, out var hooks ) )
                {
                    foreach ( var hookType in hooks )
                    {
                        hookList.Add( ( IEntitySaveHook ) Activator.CreateInstance( hookType ) );
                    }
                }
            }

            // Get all hooks for interfaces the type implements.
            foreach ( var interfaceType in entityType.GetInterfaces() )
            {
                if ( _readSafeHooks.TryGetValue( interfaceType, out var hooks ) )
                {
                    foreach ( var hookType in hooks )
                    {
                        hookList.Add( ( IEntitySaveHook ) Activator.CreateInstance( hookType ) );
                    }
                }
            }

            return hookList;
        }

        /// <summary>
        /// Rebuilds the read-only hooks dictionary from the read-write
        /// list of hooks.
        /// </summary>
        private void RebuildHooks()
        {
            _readSafeHooks = _hooks
                .GroupBy( a => a.Key )
                .ToDictionary( g => g.Key, g => g.Select( a => a.Value ).ToList() );
        }

        #endregion
    }
}
