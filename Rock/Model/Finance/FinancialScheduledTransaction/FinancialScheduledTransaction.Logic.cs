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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// FinancialScheduledTransaction Logic
    /// </summary>
    public partial class FinancialScheduledTransaction : Model<FinancialScheduledTransaction>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// The JSON for <see cref="PreviousGatewayScheduleIds"/>. If this is null,
        /// there are no PreviousGatewayScheduleIds.
        /// </summary>
        /// <value></value>
        [DataMember]
        public string PreviousGatewayScheduleIdsJson
        {
            get
            {
                // If there are any PreviousGatewayScheduleIds, store them as JSON.
                // Otherwise, store as NULL so it is easy to figure out which scheduled transaction have PreviousGatewayScheduleIds.
                if ( PreviousGatewayScheduleIds != null && PreviousGatewayScheduleIds.Any() )
                {
                    // at least one PreviousGatewayScheduleId, so store it in the database
                    return PreviousGatewayScheduleIds?.ToJson();
                }
                else
                {
                    // no PreviousGatewayScheduleIds, so leave PreviousGatewayScheduleIdsJson as null;
                    return null;
                }
            }

            set
            {
                PreviousGatewayScheduleIds = value.FromJsonOrNull<List<string>>() ?? new List<string>();
            }
        }

        #endregion Entity Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this transaction.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this transaction.
        /// </returns>
        public override string ToString()
        {
            return this.TotalAmount.ToStringSafe();
        }

        #endregion Public Methods
    }
}