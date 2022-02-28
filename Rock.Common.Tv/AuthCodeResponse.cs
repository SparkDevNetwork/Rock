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

namespace Rock.Common.Tv
{
    /// <summary>
    /// POCO for the auth code return
    /// </summary>
    public class AuthCodeResponse
    {
        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the expire date time.
        /// </summary>
        /// <value>
        /// The expire date time.
        /// </value>
        public DateTime ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the authentication URL qr code.
        /// </summary>
        /// <value>
        /// The authentication URL qr code.
        /// </value>
        public string AuthenticationUrlQrCode { get; set; }
    }
}
