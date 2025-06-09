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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    public partial class RegistrationTemplateFeeItem
    {
        /// <summary>
        /// If this fee has a <see cref="MaximumUsageCount" />, returns the number of allowed usages remaining for the specified <see cref="RegistrationInstance" />
        /// </summary>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="otherRegistrants">The other registrants that have been registered so far in this registration</param>
        /// <returns></returns>
        public int? GetUsageCountRemaining( RegistrationInstance registrationInstance, List<RegistrantInfo> otherRegistrants )
        {
            if ( !this.MaximumUsageCount.HasValue || registrationInstance == null )
            {
                return null;
            }

            int? usageCountRemaining;
            var registrationInstanceId = registrationInstance.Id;
            var registrationInstanceFeesQuery = new RegistrationRegistrantFeeService( new RockContext() ).Queryable().Where( a => a.RegistrationRegistrant.Registration.RegistrationInstanceId == registrationInstanceId );

            var feeUsedCount = registrationInstanceFeesQuery.Where( a => a.RegistrationTemplateFeeItemId == this.Id ).Sum( a => ( int? ) a.Quantity ) ?? 0;

            // get a list of fees that the other registrants in this registrant entry have incurred so far
            List<FeeInfo> otherRegistrantsFees = otherRegistrants?.SelectMany( a => a.FeeValues ).Where( a => a.Value != null && a.Key == this.RegistrationTemplateFeeId ).SelectMany( a => a.Value ).ToList();

            // get the count of fees of this same fee item for other registrants
            int otherRegistrantsUsedCount = otherRegistrantsFees?.Where( a => a.RegistrationTemplateFeeItemId == this.Id ).Sum( f => f.Quantity ) ?? 0;

            usageCountRemaining = this.MaximumUsageCount.Value - feeUsedCount - otherRegistrantsUsedCount;
            return usageCountRemaining;
        }

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => RegistrationTemplateFee ?? base.ParentAuthority;

        #endregion
    }
}
