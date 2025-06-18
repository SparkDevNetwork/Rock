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
using System.Runtime.Serialization;

using Microsoft.Extensions.DependencyInjection;

using Rock.Configuration;
using Rock.Core.Automation;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <inheritdoc cref="AutomationTrigger"/>
    [Serializable]
    [DataContract]
    public class AutomationTriggerCache : ModelCache<AutomationTriggerCache, AutomationTrigger>, IHasReadOnlyAdditionalSettings
    {
        #region Fields

        /// <summary>
        /// The lock object for modifying <see cref="_monitors"/> value.
        /// </summary>
        private static readonly object _monitorLock = new object();

        /// <summary>
        /// The lookup dictionary of currently running monitors for the trigger
        /// identifiers.
        /// </summary>
        private static IReadOnlyDictionary<int, IDisposable> _monitors = new Dictionary<int, IDisposable>();

        #endregion

        #region Properties

        /// <inheritdoc cref="AutomationTrigger.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="AutomationTrigger.Description"/>
        [DataMember]
        public string Description { get; private set; }

        /// <inheritdoc cref="AutomationTrigger.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="AutomationTrigger.ComponentEntityTypeId"/>
        [DataMember]
        public int ComponentEntityTypeId { get; private set; }

        /// <inheritdoc cref="AutomationTrigger.ComponentConfigurationJson"/>
        [DataMember]
        public string ComponentConfigurationJson { get; private set; }

        /// <inheritdoc/>
        [DataMember]
        public string AdditionalSettingsJson { get; private set; }

        /// <inheritdoc cref="AutomationTrigger.ComponentEntityType"/>
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

            if ( !( entity is AutomationTrigger automationTrigger ) )
            {
                return;
            }

            Name = automationTrigger.Name;
            Description = automationTrigger.Description;
            IsActive = automationTrigger.IsActive;
            ComponentEntityTypeId = automationTrigger.ComponentEntityTypeId;
            ComponentConfigurationJson = automationTrigger.ComponentConfigurationJson;
            AdditionalSettingsJson = automationTrigger.AdditionalSettingsJson;
        }

        /// <summary>
        /// Creates an instance of the monitor for the trigger. This will handle
        /// any exceptions that occur during the creation of the monitor.
        /// </summary>
        /// <param name="container">The container that will be creating instances for us.</param>
        /// <param name="trigger">The automation trigger that provides the configuration data.</param>
        /// <param name="rockContext">The context to use when accessing the database during monitor creation.</param>
        /// <returns>An instance of the monitor that will be disposed when it should terminate, or <c>null</c> if the monitor could not be created.</returns>
        private static IDisposable CreateMonitor( AutomationTriggerContainer container, AutomationTrigger trigger, RockContext rockContext )
        {
            try
            {
                var component = container.CreateInstance( trigger, rockContext );

                if ( component == null )
                {
                    return null;
                }

                var configurationValues = trigger.ComponentConfigurationJson
                    ?.FromJsonOrNull<Dictionary<string, string>>()
                    ?? new Dictionary<string, string>();

                return component.CreateTriggerMonitor( trigger.Id, configurationValues, rockContext );
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Failed to create automation trigger monitor: {ex.Message}" );
                return null;
            }
        }

        /// <summary>
        /// Creates all the monitors for the triggers that are currently active.
        /// This is intended to be used during Rock startup.
        /// </summary>
        /// <param name="container">The container for all the automation trigger components.</param>
        internal static void CreateAllMonitors( AutomationTriggerContainer container )
        {
            using ( var rockContext = new RockContext() )
            {
                // Don't use cache since we might get executed before the cache
                // is ready and valid.
                var triggers = new AutomationTriggerService( rockContext ).Queryable()
                    .Where( t => t.IsActive )
                    .ToList();

                lock ( _monitorLock )
                {
                    // Shut down all the old monitors.
                    foreach ( var oldMonitor in _monitors.Values )
                    {
                        try
                        {
                            oldMonitor?.Dispose();
                        }
                        catch ( Exception ex )
                        {
                            System.Diagnostics.Debug.WriteLine( $"Failed to dispose automation trigger monitor: {ex.Message}" );
                        }
                    }

                    var monitors = new Dictionary<int, IDisposable>();

                    // Start up all the new monitors.
                    foreach ( var trigger in triggers )
                    {
                        var monitor = CreateMonitor( container, trigger, rockContext );

                        if ( monitor != null )
                        {
                            monitors[trigger.Id] = monitor;
                        }
                    }

                    _monitors = monitors;
                }
            }
        }

        /// <summary>
        /// Replaces the currently running trigger monitor with a new monitor
        /// that will handle the trigger. If the trigger has been removed then
        /// the existing monitor will be destroyed without creating a new
        /// monitor.
        /// </summary>
        /// <param name="triggerId">The identifier of the automation trigger that was added, modified or deleted.</param>
        internal static void UpdateTriggerMonitor( int triggerId )
        {
            var container = RockApp.Current.GetService<AutomationTriggerContainer>();

            using ( var rockContext = new RockContext() )
            {
                // Don't use cache since we might get executed before the cache
                // flush bus message has gone through.
                var trigger = new AutomationTriggerService( rockContext ).Get( triggerId );

                lock ( _monitorLock )
                {
                    var monitors = new Dictionary<int, IDisposable>( ( Dictionary<int, IDisposable> ) _monitors );

                    // Shut down the old monitor.
                    if ( monitors.TryGetValue( triggerId, out var oldMonitor ) )
                    {
                        try
                        {
                            oldMonitor?.Dispose();
                        }
                        catch ( Exception ex )
                        {
                            System.Diagnostics.Debug.WriteLine( $"Failed to dispose automation trigger monitor: {ex.Message}" );
                        }

                        monitors.Remove( triggerId );
                    }

                    if ( trigger != null && trigger.IsActive )
                    {
                        var monitor = CreateMonitor( container, trigger, rockContext );

                        if ( monitor != null )
                        {
                            monitors[trigger.Id] = monitor;
                        }
                    }

                    _monitors = monitors;
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
