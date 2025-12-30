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

namespace Rock.Enums.Blocks.Connection.ConnectionOpportunitySignup
{
    /// <summary>
    /// Represents the result type for a connection opportunity signup attempt.
    /// </summary>
    public enum ConnectionOpportunitySignupResultType
    {
        /// <summary>
        /// The signup was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The request was invalid.
        /// </summary>
        CaptchaInvalid = 1,

        /// <summary>
        /// The captcha validation failed.
        /// </summary>
        InvalidRequest = 2,

        /// <summary>
        /// The specified opportunity was not found.
        /// </summary>
        OpportunityNotFound = 3,

        /// <summary>
        /// The connection request was invalid.
        /// </summary>
        InvalidConnectionRequest = 4,

        /// <summary>
        /// An unknown error occurred.
        /// </summary>
        UnknownError = 99
    }
}
