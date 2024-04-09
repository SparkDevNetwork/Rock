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

using System.Linq;

using Rock.Web.Cache;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on any data views that
    /// the person must be a member of.
    /// </summary>
    internal class DataViewOpportunityFilter : OpportunityFilter
    {
        #region Methods

        /// <inheritdoc/>
        public override bool IsGroupValid( GroupOpportunity group )
        {
            if ( group.CheckInData.DataViewGuids.Count == 0 )
            {
                return true;
            }

            foreach ( var dataViewGuid in group.CheckInData.DataViewGuids )
            {
                var dataView = DataViewCache.Get( dataViewGuid, RockContext );

                if ( dataView == null )
                {
                    continue;
                }

                var queryArgs = new Reporting.GetQueryableOptions
                {
                    DbContext = RockContext
                };

                if ( !dataView.GetEntityIds( queryArgs ).Contains( PersonId.Value ) )
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
