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
using Rock.Security;

namespace Rock.Model
{
    public partial class GroupHistorical
    {
        #region Methods

        /// <summary>
        /// Creates a GroupHistorical with CurrentRowIndicator = true for the specified group
        /// </summary>
        /// <param name="group">The group.</param>
        /// <param name="effectiveDateTime">The effective date time.</param>
        /// <returns></returns>
        public static GroupHistorical CreateCurrentRowFromGroup( Group group, DateTime effectiveDateTime )
        {
            var groupHistoricalCurrent = new GroupHistorical
            {
                GroupId = group.Id,
                GroupName = group.Name,
                GroupTypeId = group.GroupTypeId,
                GroupTypeName = group.GroupType.Name,
                CampusId = group.CampusId,
                ParentGroupId = group.ParentGroupId,
                ScheduleId = group.ScheduleId,
                ScheduleName = group.Schedule?.ToString(),
                ScheduleModifiedDateTime = group.Schedule?.ModifiedDateTime,
                Description = group.Description,
                StatusValueId = group.StatusValueId,
                IsArchived = group.IsArchived,
                ArchivedDateTime = group.ArchivedDateTime,
                ArchivedByPersonAliasId = group.ArchivedByPersonAliasId,
                IsActive = group.IsActive,
                InactiveDateTime = group.InactiveDateTime,

                // Set the Modified/Created fields for GroupHistorical to be the current values from Group table
                ModifiedDateTime = group.ModifiedDateTime,
                ModifiedByPersonAliasId = group.ModifiedByPersonAliasId,
                CreatedByPersonAliasId = group.CreatedByPersonAliasId,
                CreatedDateTime = group.CreatedDateTime,

                // Set HistoricalTracking fields
                CurrentRowIndicator = true,
                EffectiveDateTime = effectiveDateTime,
                ExpireDateTime = HistoricalTracking.MaxExpireDateTime
            };

            return groupHistoricalCurrent;
        }

        #endregion

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => Group ?? base.ParentAuthority;

        #endregion
    }
}
