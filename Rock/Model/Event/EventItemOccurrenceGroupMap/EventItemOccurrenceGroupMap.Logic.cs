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

using Rock.Security;

namespace Rock.Model
{
    public partial class EventItemOccurrenceGroupMap
    {
        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString( false, true, true );
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="includeEventItem">if set to <c>true</c> [include event item].</param>
        /// <param name="includeRegistrationInstance">if set to <c>true</c> [include registration instance].</param>
        /// <param name="includeGroup">if set to <c>true</c> [include group].</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString( bool includeEventItem, bool includeRegistrationInstance, bool includeGroup )
        {
            var parts = new List<string>();

            if ( includeEventItem && EventItemOccurrence != null )
            {
                parts.Add( EventItemOccurrence.ToString() );
            }

            if ( includeRegistrationInstance && RegistrationInstance != null )
            {
                parts.Add( RegistrationInstance.ToString() );
            }

            if ( includeGroup && Group != null )
            {
                parts.Add( Group.ToString() );
            }

            return parts.AsDelimited( " - " );
        }

        #endregion

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => EventItemOccurrence ?? base.ParentAuthority;

        #endregion
    }
}
