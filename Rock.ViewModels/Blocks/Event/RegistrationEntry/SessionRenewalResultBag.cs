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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// SessionRenewalResult
    /// </summary>
    public sealed class SessionRenewalResultBag
    {
        /// <summary>
        /// Gets or sets the spots secured.
        /// </summary>
        /// <value>
        /// The spots secured.
        /// </value>
        public int SpotsSecured { get; set; }

        /// <summary>
        /// Gets or sets the expiration date time.
        /// </summary>
        /// <value>
        /// The expiration date time.
        /// </value>
        public DateTimeOffset ExpirationDateTime { get; set; }
    }
}
