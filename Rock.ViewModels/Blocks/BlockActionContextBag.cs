﻿// <copyright>
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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks
{
    /// <summary>
    /// The data structure of the __context parameter included with block
    /// action requests.
    /// </summary>
    public class BlockActionContextBag
    {
        /// <summary>
        /// Gets or sets the page parameters that the page was originally
        /// loaded with.
        /// </summary>
        /// <value>The page parameters.</value>
        public Dictionary<string, string> PageParameters { get; set; }

        /// <summary>
        /// Gets or sets the captcha that should be validated by the server.
        /// </summary>
        /// <value>The captcha that should be validated by the server.</value>
        public string Captcha { get; set; }

        /// <summary>
        /// The Interaction session that this request belongs to. This is used
        /// to group block action API requests back to the main request's
        /// session.
        /// </summary>
        public Guid? SessionGuid { get; set; }

        /// <summary>
        /// Identifies the interaction that represented the original page load.
        /// This is used to correlate actions in this request with the
        /// original interaction.
        /// </summary>
        public Guid? InteractionGuid { get; set; }
    }
}
