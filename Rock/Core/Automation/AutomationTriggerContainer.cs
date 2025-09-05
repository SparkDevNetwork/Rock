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

using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Core.Automation
{
    /// <summary>
    /// The container for automation components.
    /// </summary>
    internal class AutomationTriggerContainer : LightContainer<AutomationTriggerComponent>
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationTriggerContainer"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider that will be used to construct component instances.</param>
        public AutomationTriggerContainer( IServiceProvider serviceProvider )
            : base( serviceProvider )
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the component for the <see cref="AutomationTrigger"/>
        /// instance. This requires that the <see cref="AutomationTrigger.ComponentEntityTypeId"/>
        /// be set correctly.
        /// </summary>
        /// <param name="automationTrigger">The the trigger to create the component instance for.</param>
        /// <param name="rockContext">The context to use if access to to the database is required.</param>
        /// <returns>A component instance of <see cref="AutomationTriggerComponent"/> or <c>null</c>.</returns>
        public AutomationTriggerComponent CreateInstance( AutomationTrigger automationTrigger, RockContext rockContext )
        {
            if ( automationTrigger == null || automationTrigger.ComponentEntityTypeId == 0 )
            {
                return null;
            }

            return CreateInstance( automationTrigger.Id, automationTrigger.ComponentEntityTypeId, rockContext );
        }

        /// <summary>
        /// Creates an instance of the component for the <see cref="AutomationTriggerCache"/>
        /// instance.
        /// </summary>
        /// <param name="automationTrigger">The the trigger to create the component instance for.</param>
        /// <param name="rockContext">The context to use if access to to the database is required.</param>
        /// <returns>A component instance of <see cref="AutomationTriggerComponent"/> or <c>null</c>.</returns>
        public AutomationTriggerComponent CreateInstance( AutomationTriggerCache automationTrigger, RockContext rockContext )
        {
            if ( automationTrigger == null || automationTrigger.ComponentEntityTypeId == 0 )
            {
                return null;
            }

            return CreateInstance( automationTrigger.Id, automationTrigger.ComponentEntityTypeId, rockContext );
        }

        /// <summary>
        /// Creates an instance of the component for a <see cref="AutomationTrigger"/>
        /// in the database. This is used to create the component from either database
        /// record or a cache record.
        /// </summary>
        /// <param name="automationTriggerId">The identifier of the trigger.</param>
        /// <param name="componentEntityTypeId">The identifier of the entity type component.</param>
        /// <param name="rockContext">The context to use if access to to the database is required.</param>
        /// <returns>A component instance of <see cref="AutomationTriggerComponent"/> or <c>null</c>.</returns>
        private AutomationTriggerComponent CreateInstance( int automationTriggerId, int componentEntityTypeId, RockContext rockContext )
        {
            var entityTypeCache = EntityTypeCache.Get( componentEntityTypeId, rockContext );

            if ( entityTypeCache == null )
            {
                return null;
            }

            var component = CreateInstance( entityTypeCache.Guid );

            if ( component == null )
            {
                return null;
            }

            if ( automationTriggerId != 0 )
            {
                component.AutomationTriggerId = automationTriggerId;
            }

            return component;
        }

        #endregion
    }
}
