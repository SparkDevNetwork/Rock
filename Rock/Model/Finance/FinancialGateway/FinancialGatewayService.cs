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

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.FinancialGateway"/> entity objects.
    /// </summary>
    public partial class FinancialGatewayService
    {
        /// <summary>
        /// Determines whether [is redirection gateway] [the specified gateway identifier].
        /// </summary>
        /// <param name="gatewayId">The gateway identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is redirection gateway] [the specified gateway identifier]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsRedirectionGateway(int? gatewayId)
        {
            // validate gateway
            if ( gatewayId.HasValue )
            {
                var financialGateway = Get( gatewayId.Value );
                return financialGateway.IsRedirectionGateway();
            }
            return false;
        }
    }
}