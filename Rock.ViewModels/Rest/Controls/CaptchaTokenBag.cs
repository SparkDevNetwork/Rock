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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Represents a captcha token.
    /// </summary>
    public class CaptchaTokenBag
    {
        /// <summary>
        /// Gets or sets the unix timestamp when the captcha token expires.
        /// </summary>
        public long Expires { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the captcha token.
        /// </summary>
        public Guid Guid { get; set; } = Guid.NewGuid();
    }
}
