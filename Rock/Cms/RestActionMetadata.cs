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
    /// Contains additional data about a <see cref="Model.RestAction"/>
    /// that will be used at runtime. This is meant to provide quick access
    /// to calculated values as well as access to data that might otherwise
    /// require C# reflection.
    /// </summary>
    internal class RestActionMetadata
    {
        /// <summary>
        /// Specifies the security action that will be used to authorize access
        /// to the API endpoint. If this is null or an empty string then no
        /// automatic authorization will be performed.
        /// </summary>
        public string SecuredAction { get; set; }

        /// <summary>
        /// The security actions supported by this endpoint.
        /// </summary>
        public Dictionary<string, string> SupportedActions { get; set; }
    }
}
