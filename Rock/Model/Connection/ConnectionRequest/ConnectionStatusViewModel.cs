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

using System.Collections.Generic;

namespace Rock.Model
{
    /// <summary>
    /// Connection Status View Model (columns)
    /// </summary>
    public sealed class ConnectionStatusViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the actual number of requests, since the Requests property may be a limited set.
        /// </summary>
        public int RequestCount { get; set; }

        /// <summary>
        /// Gets or sets the requests. This may be a limited sub-set of the actual set of requests.
        /// </summary>
        public List<ConnectionRequestViewModel> Requests { get; set; }
    }
}
