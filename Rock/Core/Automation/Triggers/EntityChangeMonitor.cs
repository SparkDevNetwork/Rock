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
using System.Linq.Dynamic.Core;

using Rock.Data;
using Rock.Enums.Core.Automation.Triggers;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// The monitor for the <see cref="EntityChange"/> trigger. This does not
    /// actually monitor for entity changes, but acts as a lookup table for
    /// the logic in DbContext to give the monitor a chance to process the
    /// save entry.
    /// </summary>
    internal sealed class EntityChangeMonitor : IDisposable
    {
        #region Fields

        /// <summary>
        /// The lock object used to synchronize updates to <see cref="_monitors"/>.
        /// This is only used when making changes, not when reading the value.
        /// </summary>
        private static readonly object _monitorsLock = new object();

        /// <summary>
        /// The lookup dictionary of entity type names (e.g. Rock.Model.Person)
        /// to the change monitors that are registered for that type.
        /// </summary>
        private static IReadOnlyDictionary<string, MonitorItems> _monitors = new Dictionary<string, MonitorItems>();

        /// <summary>
        /// The full name of the entity type that this monitor is registered for.
        /// </summary>
        private readonly string _entityTypeName;

        /// <summary>
        /// The identifier of the automation trigger that this monitor
        /// registered for.
        /// </summary>
        private readonly int _triggerId;

        /// <summary>
        /// The criteria object that will handle checking save entries to see
        /// if they match and the events should be executed.
        /// </summary>
        private readonly EntityChangeCriteria _criteria;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="EntityChangeMonitor"/> class.
        /// </summary>
        /// <param name="triggerId">The automation trigger identifier this monitor represents.</param>
        /// <param name="type">The type of the entity being monitors for changes.</param>
        /// <param name="modificationType">The type of database modification that was performed.</param>
        /// <param name="criteria">The criteria that will be used to check if entity entries match.</param>
        public EntityChangeMonitor( int triggerId, Type type, EntityChangeModificationType modificationType, EntityChangeCriteria criteria )
        {
            _triggerId = triggerId;
            _entityTypeName = type?.FullName;
            _criteria = criteria;

            if ( _entityTypeName != null )
            {
                lock ( _monitorsLock )
                {
                    var monitors = DuplicateMonitors();

                    if ( !monitors.TryGetValue( _entityTypeName, out var entityMonitors ) )
                    {
                        entityMonitors = monitors[_entityTypeName] = new MonitorItems();
                    }

                    if ( modificationType.HasFlag( EntityChangeModificationType.Added ) )
                    {
                        entityMonitors.Added.Add( this );
                    }

                    if ( modificationType.HasFlag( EntityChangeModificationType.Modified ) )
                    {
                        entityMonitors.Modified.Add( this );
                    }

                    if ( modificationType.HasFlag( EntityChangeModificationType.Deleted ) )
                    {
                        entityMonitors.Deleted.Add( this );
                    }

                    _monitors = monitors;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock ( _monitorsLock )
            {
                var monitors = DuplicateMonitors();

                if ( monitors.TryGetValue( _entityTypeName, out var entityMonitors ) )
                {
                    entityMonitors.Added.Remove( this );
                    entityMonitors.Modified.Remove( this );
                    entityMonitors.Deleted.Remove( this );

                    if ( !entityMonitors.Added.Any() && !entityMonitors.Modified.Any() && !entityMonitors.Deleted.Any() )
                    {
                        monitors.Remove( _entityTypeName );
                    }
                }

                _monitors = monitors;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Duplicates the current monitor dictionary. This is used to ensure
        /// that any changes we make to the dictionary or the list of monitors
        /// do not affect the current lookup table, which could cause a crash.
        /// </summary>
        /// <returns>A new dictionary that represents the same data as <see cref="_monitors"/>.</returns>
        private static Dictionary<string, MonitorItems> DuplicateMonitors()
        {
            var monitors = new Dictionary<string, MonitorItems>( _monitors.Count );

            foreach ( var kvp in _monitors )
            {
                monitors[kvp.Key] = new MonitorItems
                {
                    Added = kvp.Value.Added.ToList(),
                    Modified = kvp.Value.Modified.ToList(),
                    Deleted = kvp.Value.Deleted.ToList()
                };
            }

            return monitors;
        }

        /// <summary>
        /// Processes the entity save entry and executes any events that are
        /// attached to the trigger.
        /// </summary>
        /// <param name="entry"></param>
        public static void ProcessEntity( IEntitySaveEntry entry )
        {
            if ( !( entry.Entity is IEntity entity ) )
            {
                return;
            }

            if ( !_monitors.TryGetValue( entity.TypeName, out var entityMonitors ) )
            {
                return;
            }

            var request = new AutomationRequest
            {
                Values = new Dictionary<string, object>
                {
                    [AutomationRequest.KnownKeys.Entity] = entry.Entity,
                    ["Person"] = RockRequestContextAccessor.Current?.CurrentPerson,
                    ["OriginalValues"] = entry.OriginalValues,
                    ["ModifiedProperties"] = entry.ModifiedProperties,
                    ["State"] = entry.PreSaveState
                }
            };

            List<EntityChangeMonitor> monitors;

            // Order the if statements so that the most common state is first.
            if ( entry.State == EntityContextState.Modified )
            {
                monitors = entityMonitors.Modified;
            }
            else if ( entry.State == EntityContextState.Added )
            {
                monitors = entityMonitors.Added;
            }
            else if ( entry.State == EntityContextState.Deleted )
            {
                monitors = entityMonitors.Deleted;
            }
            else
            {
                return;
            }

            foreach ( var monitor in monitors )
            {
                if ( monitor._criteria.IsMatch( entry ) )
                {
                    AutomationEventCache.ExecuteEvents( monitor._triggerId, request );
                }
            }
        }

        #endregion

        #region Support Classes

        private class MonitorItems
        {
            public List<EntityChangeMonitor> Added { get; set; } = new List<EntityChangeMonitor>();

            public List<EntityChangeMonitor> Modified { get; set; } = new List<EntityChangeMonitor>();

            public List<EntityChangeMonitor> Deleted { get; set; } = new List<EntityChangeMonitor>();
        }

        #endregion
    }
}
