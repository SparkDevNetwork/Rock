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

using Rock.Enums.Blocks.Connection.ConnectionOpportunitySignup;

namespace Rock.ViewModels.Blocks.Connection.ConnectionOpportunitySignup
{
    /// <summary>
    /// A bag containing the response information after signing up.
    /// </summary>
    public class ConnectionOpportunitySignupResultBag
    {
        /// <summary>
        /// Gets or sets the result type of the signup attempt.
        /// </summary>
        public ConnectionOpportunitySignupResultType ResultType { get; set; }

        /// <summary>
        /// Gets or sets the response message (success or error details).
        /// </summary>
        public string ResponseMessage { get; set; }
    }
}
