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
    /// POCO for the Auth Code Check return
    /// </summary>
    public class AuthCodeCheckResponse
    {

        /// <summary>
        /// Gets or sets a value indicating whether this instance is authenticated.
        /// </summary>
        /// <value><c>true</c> if this instance is authenticated; otherwise, <c>false</c>.</value>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public TvPerson CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the Roku token.
        /// </summary>
        public string RokuToken { get; set; }

        /// <summary>
        /// Obsolete as of Rock v14.1. Please use 'IsAuthenticated' as it is proper spelling.
        /// </summary>
        [Obsolete]
        public bool IsAuthenciated { get; set; }
    }
}
