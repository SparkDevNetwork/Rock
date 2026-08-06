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

using System.Collections.Generic;

namespace Rock.Configuration.ConnectedServices.RockIntelligence
{
    /// <summary>
    /// The manifest data for Rock Intelligence, which is a connected
    /// service that provides AI capabilities. This manifest contains all
    /// required information to configure and use Rock Intelligence services.
    /// </summary>
    internal class Manifest
    {
        /// <summary>
        /// The bundles available for Rock Intelligence.
        /// </summary>
        public List<ServiceEntryBundle<Settings>> Bundles { get; set; }
    }
}
