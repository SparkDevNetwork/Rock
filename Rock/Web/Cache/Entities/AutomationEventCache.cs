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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Core.Automation;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <inheritdoc cref="AutomationEvent"/>
    [Serializable]
    [DataContract]
    public class AutomationEventCache : ModelCache<AutomationEventCache, AutomationEvent>, IHasReadOnlyAdditionalSettings
    {
        #region Fields

        /// <summary>
        /// The lock object for modifying <see cref="_executors"/> value.
        /// </summary>
        private static readonly object _executorLock = new object();

        /// <summary>
        /// The lookup dictionary of cached executors for the trigger identifiers.
        /// </summary>
        private static IReadOnlyDictionary<int, List<AutomationEventExecutor>> _executors = new Dictionary<int, List<AutomationEventExecutor>>();

        #endregion

        #region Properties

        /// <inheritdoc cref="AutomationEvent.AutomationTriggerId"/>
        [DataMember]
        public int AutomationTriggerId { get; set; }

        /// <inheritdoc cref="AutomationEvent.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="AutomationEvent.Order"/>
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="AutomationEvent.ComponentEntityTypeId"/>
        [DataMember]
        public int ComponentEntityTypeId { get; private set; }

        /// <inheritdoc cref="AutomationEvent.ComponentConfigurationJson"/>
        [DataMember]
        public string ComponentConfigurationJson { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        /// <inheritdoc cref="AutomationEvent.AutomationTrigger"/>
        [LavaVisible]
        public virtual AutomationTriggerCache AutomationTrigger => AutomationTriggerCache.Get( AutomationTriggerId );

        /// <inheritdoc cref="AutomationEvent.ComponentEntityType"/>
        [LavaVisible]
        public virtual EntityTypeCache ComponentEntityType => EntityTypeCache.Get( ComponentEntityTypeId );

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is AutomationEvent automationEvent ) )
            {
                return;
            }

            AutomationTriggerId = automationEvent.AutomationTriggerId;
            IsActive = automationEvent.IsActive;
            Order = automationEvent.Order;
            ComponentEntityTypeId = automationEvent.ComponentEntityTypeId;
            ComponentConfigurationJson = automationEvent.ComponentConfigurationJson;
            AdditionalSettingsJson = automationEvent.AdditionalSettingsJson;
        }

        /// <summary>
        /// Creates all the executors for the various triggers that are currently active.
        /// This is intended to be used during Rock startup.
        /// </summary>
        /// <param name="container">The container for all the automation event components.</param>
        internal static void CreateAllExecutors( AutomationEventContainer container )
        {
            using ( var rockContext = new RockContext() )
            {
                // Don't use cache since we might get executed before the cache
                // is ready and valid.
                var triggerIds = new AutomationTriggerService( rockContext ).Queryable()
                    .Where( t => t.IsActive )
                    .Select( t => t.Id )
                    .ToList();

                lock ( _executorLock )
                {
                    foreach ( var triggerId in triggerIds )
                    {
                        UpdateTriggerExecutors( container, triggerId, rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Updates the executors for the trigger with the specified identifier.
        /// Any existing executors are disposed and re-created.
        /// </summary>
        /// <param name="automationTriggerId">The identifier of the <see cref="AutomationTrigger"/> to initialize executors for.</param>
        internal static void UpdateTriggerExecutors( int automationTriggerId )
        {
            var container = RockApp.Current.GetService<AutomationEventContainer>();

            if ( container == null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                lock ( _executorLock )
                {
                    UpdateTriggerExecutors( container, automationTriggerId, rockContext );
                }
            }
        }

        /// <summary>
        /// Updates the executors for the trigger with the specified identifier.
        /// Any existing executors are disposed and re-created. This must be
        /// called inside a lock on <see cref="_executorLock"/>.
        /// </summary>
        /// <param name="container">The container that will handle component creation.</param>
        /// <param name="automationTriggerId">The identifier of the <see cref="AutomationTrigger"/> to initialize executors for.</param>
        /// <param name="rockContext">The context to use for any database access that is required.</param>
        private static void UpdateTriggerExecutors( AutomationEventContainer container, int automationTriggerId, RockContext rockContext )
        {
            var triggerExecutors = new List<AutomationEventExecutor>();

            var automationEvents = new AutomationEventService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( e => e.AutomationTriggerId == automationTriggerId
                    && e.AutomationTrigger.IsActive )
                .OrderBy( e => e.Order )
                .ThenBy( e => e.Id )
                .ToList();

            foreach ( var automationEvent in automationEvents )
            {
                try
                {
                    var component = container.CreateInstance( automationEvent, rockContext );
                    var configurationValues = automationEvent.ComponentConfigurationJson
                        ?.FromJsonOrNull<Dictionary<string, string>>()
                        ?? new Dictionary<string, string>();

                    var executor = component?.CreateExecutor( automationEvent.Id, configurationValues, rockContext );

                    if ( executor != null )
                    {
                        triggerExecutors.Add( executor );
                    }
                }
                catch
                {
                    // Intentionally ignore any errors creating the executor.
                }
            }

            var executors = new Dictionary<int, List<AutomationEventExecutor>>();

            foreach ( var kvp in _executors )
            {
                executors[kvp.Key] = kvp.Value;
            }

            // Dispose any old executors for this trigger.
            if ( executors.TryGetValue( automationTriggerId, out var oldExecutors ) )
            {
                foreach ( var oldExecutor in oldExecutors )
                {
                    try
                    {
                        oldExecutor.Dispose();
                    }
                    catch
                    {
                        // Intentionally ignore any dispose errors.
                    }
                }

                executors.Remove( automationTriggerId );
            }

            // Add the new executors for this trigger if we have any.
            if ( triggerExecutors.Any() )
            {
                executors[automationTriggerId] = triggerExecutors;
            }

            _executors = executors;
        }

        /// <summary>
        /// Executes the events for the trigger with the specified identifier.
        /// </summary>
        /// <param name="automationTriggerId">The identifier of the trigger.</param>
        /// <param name="request">The <see cref="AutomationRequest"/> that contains the details from the trigger.</param>
        internal static void ExecuteEvents( int automationTriggerId, AutomationRequest request )
        {
            if ( !_executors.TryGetValue( automationTriggerId, out var triggerExecutors ) )
            {
                return;
            }

            foreach ( var executor in triggerExecutors )
            {
                try
                {
                    executor.Execute( request );
                }
                catch
                {
                    // Intentionally ignore any errors executing the event.
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return base.ToString();
        }

        #endregion
    }
}
