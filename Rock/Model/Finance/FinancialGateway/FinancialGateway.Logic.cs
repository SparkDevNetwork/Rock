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
using Rock.Financial;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// FinancialGateway Logic
    /// </summary>
    public partial class FinancialGateway : Model<FinancialGateway>, IHasActiveFlag
    {
        #region Public Methods

        /// <summary>
        /// Gets the gateway component.
        /// </summary>
        /// <returns></returns>
        public virtual GatewayComponent GetGatewayComponent()
        {
            if ( EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                if ( entityType != null )
                {
                    return GatewayContainer.GetComponent( entityType.Name );
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the batch time offset.
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetBatchTimeOffset()
        {
            return new TimeSpan( BatchTimeOffsetTicks );
        }

        /// <summary>
        /// Sets the batch time offset.
        /// </summary>
        /// <param name="timespan">The timespan.</param>
        public void SetBatchTimeOffset( TimeSpan? timespan )
        {
            BatchTimeOffsetTicks = timespan.HasValue ? timespan.Value.Ticks : 0;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FinancialGateway.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FinancialGateway.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }
}