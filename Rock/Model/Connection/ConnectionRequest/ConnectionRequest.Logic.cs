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

using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class ConnectionRequest
    {
        #region Methods

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ConnectionOpportunity ?? base.ParentAuthority;
            }
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>True if the person is authorized; false otherwise.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.ConnectionOpportunity != null
                && this.ConnectionOpportunity.ConnectionType != null
                && this.ConnectionOpportunity.ConnectionType.EnableRequestSecurity
                && this.ConnectorPersonAlias != null
                && this.ConnectorPersonAlias.PersonId == person.Id )
            {
                return true;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// If changing the <see cref="ConnectionStatus"/> while looping thru all of its <see cref="ConnectionStatus.ConnectionStatusAutomations"/>, use this
        /// to set the status to indicate that the loop in PostSaveChanges doesn't also need to run.
        /// </summary>
        /// <param name="connectionStatusAutomation">The connection status automation.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SetConnectionStatusFromAutomationLoop( ConnectionStatusAutomation connectionStatusAutomation )
        {
            if ( !_runAutomationsInPostSaveChanges )
            {
                return false;
            }

            _runAutomationsInPostSaveChanges = false;

            bool alreadyProcessed = this.processedConnectionStatusAutomations.Contains( connectionStatusAutomation.Id );

            if ( alreadyProcessed )
            {
                // to avoid recursion
                return false;
            }

            this.processedConnectionStatusAutomations.Add( connectionStatusAutomation.Id );

            if ( this.ConnectionStatusId == connectionStatusAutomation.DestinationStatusId )
            {
                // already set to this status
                return false;
            }

            this.ConnectionStatusId = connectionStatusAutomation.DestinationStatusId;

            return true;
        }

        /// <summary>
        /// Get a list of all inherited Attributes that should be applied to this entity.
        /// </summary>
        /// <returns>A list of all inherited AttributeCache objects.</returns>
        public override List<AttributeCache> GetInheritedAttributes( Rock.Data.RockContext rockContext )
        {
            var connectionTypeId = ConnectionTypeId;

            // If this instance hasn't been saved yet, it might not have this
            // auto generated value set yet.
            if ( connectionTypeId == 0 )
            {
                if ( ConnectionOpportunity == null )
                {
                    connectionTypeId = new ConnectionOpportunityService( rockContext ).Queryable()
                        .Where( co => co.Id == ConnectionOpportunityId )
                        .Select( co => co.ConnectionTypeId )
                        .FirstOrDefault();
                }
                else
                {
                    connectionTypeId = ConnectionOpportunity.ConnectionTypeId;
                }
            }

            if ( connectionTypeId == 0 )
            {
                return null;
            }

            var connectionTypeCache = ConnectionTypeCache.Get( connectionTypeId );

            return connectionTypeCache?.GetInheritedAttributesForQualifier( TypeId, "ConnectionTypeId" );
        }

        #endregion
    }
}
