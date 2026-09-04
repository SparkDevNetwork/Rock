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

using System;

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Represents a named service bundle.
    /// </summary>
    internal class ServiceBundle
    {
        /// <summary>
        /// The unique identifier for the bundle.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the bundle.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The order of this bundle in the list of bundles.
        /// </summary>
        public int Order { get; set; }
    }
}
