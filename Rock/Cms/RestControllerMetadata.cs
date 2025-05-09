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

namespace Rock.Cms
{
    /// <summary>
    /// Contains additional data about a <see cref="Model.RestController"/>
    /// that will be used at runtime. This is meant to provide quick access
    /// to calculated values as well as access to data that might otherwise
    /// require C# reflection.
    /// </summary>
    internal class RestControllerMetadata
    {
        /// <summary>
        /// The version number of the API. This is calculated based on the
        /// route prefix and represents how Rock should handle the API
        /// internally. This should not be used by core or plugins to simply
        /// indicate a new version of a pre-existing controller.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The security actions supported by this controller.
        /// </summary>
        public Dictionary<string, string> SupportedActions { get; set; }

        /// <summary>
        /// The base route to reach actions in this controller, such as
        /// <c>api/v2/models/groups</c>. This may be null or empty for legacy
        /// v1 API controllers.
        /// </summary>
        public string RoutePrefix { get; set; }
    }
}
