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

namespace Rock.Update.Models
{
    /// <summary>
    /// Class used to send statics to the Rock Site.
    /// </summary>
    [Serializable]
    public class ImpactStatistic
    {
        /// <summary>
        /// Gets or sets the rock instance identifier.
        /// </summary>
        /// <value>
        /// The rock instance identifier.
        /// </value>
        public Guid RockInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the public URL.
        /// </summary>
        /// <value>
        /// The public URL.
        /// </value>
        public string PublicUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the organization.
        /// </summary>
        /// <value>
        /// The name of the organization.
        /// </value>
        public string OrganizationName { get; set; }

        /// <summary>
        /// Gets or sets the organization location.
        /// </summary>
        /// <value>
        /// The organization location.
        /// </value>
        public ImpactLocation OrganizationLocation { get; set; }

        /// <summary>
        /// Gets or sets the number of active records.
        /// </summary>
        /// <value>
        /// The number of active records.
        /// </value>
        public int NumberOfActiveRecords { get; set; }

        /// <summary>
        /// Gets or sets the environment data.
        /// </summary>
        /// <value>
        /// The environment data.
        /// </value>
        public string EnvironmentData { get; set; }
    }
}
