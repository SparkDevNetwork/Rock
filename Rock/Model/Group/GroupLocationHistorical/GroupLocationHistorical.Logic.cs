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

using System;
using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    public partial class GroupLocationHistorical
    {
        #region Methods

        /// <summary>
        /// Creates the current row from group location.
        /// </summary>
        /// <param name="groupLocation">The group location.</param>
        /// <param name="effectiveDateTime">The effective date time.</param>
        /// <returns></returns>
        public static GroupLocationHistorical CreateCurrentRowFromGroupLocation( GroupLocation groupLocation, DateTime effectiveDateTime )
        {
            var locationName = groupLocation.Location?.ToString( true );

            var groupLocationHistoricalCurrent = new GroupLocationHistorical
            {
                GroupLocationId = groupLocation.Id,
                GroupId = groupLocation.GroupId,
                GroupLocationTypeValueId = groupLocation.GroupLocationTypeValueId,
                GroupLocationTypeName = groupLocation.GroupLocationTypeValue?.Value,
                LocationId = groupLocation.LocationId,
                LocationName = locationName,
                LocationModifiedDateTime = groupLocation.Location?.ModifiedDateTime,

                // Set the Modified/Created fields for GroupLocationHistorical to be the current values from the GroupLocation table
                ModifiedDateTime = groupLocation.ModifiedDateTime,
                ModifiedByPersonAliasId = groupLocation.ModifiedByPersonAliasId,
                CreatedByPersonAliasId = groupLocation.CreatedByPersonAliasId,
                CreatedDateTime = groupLocation.CreatedDateTime,

                // Set HistoricalTracking fields
                CurrentRowIndicator = true,
                EffectiveDateTime = effectiveDateTime,
                ExpireDateTime = HistoricalTracking.MaxExpireDateTime
            };

            return groupLocationHistoricalCurrent;
        }

        #endregion

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => GroupLocation ?? base.ParentAuthority;

        #endregion
    }
}
