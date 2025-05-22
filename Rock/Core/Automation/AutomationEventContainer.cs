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
using Rock.Data;

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Core.Automation
{
    /// <summary>
    /// The container for automation components.
    /// </summary>
    internal sealed class AutomationEventContainer : EntityTypeContainer<AutomationEventComponent>
    {
        #region Methods

        /// <summary>
        /// Creates an instance of the component for the <see cref="AutomationEvent"/>
        /// instance. This requires that the <see cref="AutomationEvent.ComponentEntityTypeId"/>
        /// be set correctly.
        /// </summary>
        /// <param name="automationEvent">The the event to create the component instance for.</param>
        /// <param name="rockContext">The context to use if access to to the database is required.</param>
        /// <returns>A component instance of <see cref="AutomationEventComponent"/> or <c>null</c>.</returns>
        public AutomationEventComponent CreateInstance( AutomationEvent automationEvent, RockContext rockContext )
        {
            if ( automationEvent == null || automationEvent.ComponentEntityTypeId == 0 )
            {
                return null;
            }

            return CreateInstance( automationEvent.Id, automationEvent.ComponentEntityTypeId, rockContext );
        }

        /// <summary>
        /// Creates an instance of the component for the <see cref="AutomationEventCache"/>
        /// instance.
        /// </summary>
        /// <param name="automationEvent">The the event to create the component instance for.</param>
        /// <param name="rockContext">The context to use if access to to the database is required.</param>
        /// <returns>A component instance of <see cref="AutomationEventComponent"/> or <c>null</c>.</returns>
        public AutomationEventComponent CreateInstance( AutomationEventCache automationEvent, RockContext rockContext )
        {
            if ( automationEvent == null || automationEvent.ComponentEntityTypeId == 0 )
            {
                return null;
            }

            return CreateInstance( automationEvent.Id, automationEvent.ComponentEntityTypeId, rockContext );
        }

        /// <summary>
        /// Creates an instance of the component for a <see cref="AutomationEvent"/>
        /// in the database. This is used to create the component from either database
        /// record or a cache record.
        /// </summary>
        /// <param name="automationEventId">The identifier of the event.</param>
        /// <param name="componentEntityTypeId">The identifier of the entity type component.</param>
        /// <param name="rockContext">The context to use if access to to the database is required.</param>
        /// <returns>A component instance of <see cref="AutomationEventComponent"/> or <c>null</c>.</returns>
        private AutomationEventComponent CreateInstance( int automationEventId, int componentEntityTypeId, RockContext rockContext )
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

            if ( automationEventId != 0 )
            {
                component.AutomationEventId = automationEventId;
            }

            return component;
        }

        #endregion
    }
}
