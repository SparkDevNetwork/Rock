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

using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Extension method class for Registration
    /// </summary>
    public static partial class RegistrationExtensionMethods
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<FinancialTransactionDetail> GetPayments( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetPayments( registration != null ? registration.Id : 0 );
        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static decimal GetTotalPaid( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetTotalPayments( registration != null ? registration.Id : 0 );
        }
    }
}
