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
using System.Collections.Generic;

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Represents all available connected services options in Rock. This
    /// contains all the setting bundles for the various connected
    /// services supported by Rock.
    /// </summary>
    internal class ConnectedServicesManifest
    {
        /// <summary>
        /// The date and time this manifest was produced by the Connected
        /// Services API. This value is set by the server and is used to
        /// determine how long it has been since the manifest was last
        /// refreshed. The value should be UTC, but the type is
        /// <see cref="DateTimeOffset"/> because we don't fully trust the
        /// server to normalize the offset.
        /// </summary>
        public DateTimeOffset CreatedDateTime { get; set; }

        /// <summary>
        /// The manifests for each of the connected services that are
        /// enabled and available.
        /// </summary>
        public List<ServiceEntry> Services { get; set; }
    }
}
