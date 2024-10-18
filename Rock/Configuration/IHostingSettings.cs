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

namespace Rock.Configuration
{
    /// <summary>
    /// The hosting settings for the current Rock instance. These settings
    /// are providing by the environment and cannot be changed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>Plugins should not implement this interface as new properties
    ///         will be added over time.</strong>
    ///     </para>
    /// </remarks>
    public interface IHostingSettings
    {
        /// <summary>
        /// The date and time when the application started. This is in the
        /// organization time zone (as in RockDateTime.Now).
        /// </summary>
        DateTime ApplicationStartDateTime { get; }

        /// <summary>
        /// The description of the .NET version this process is running on.
        /// </summary>
        string DotNetVersion { get; }

        /// <summary>
        /// The physical path to the served web files. This will include a
        /// trailing slash such as <c>c:\inetpub\wwwroot\</c>.
        /// </summary>
        string WebRootPath { get; }

        /// <summary>
        /// The virtual root path. This will include a trailing slash
        /// such as <c>/subapp/</c> or just <c>/</c>.
        /// </summary>
        string VirtualRootPath { get; }

        /// <summary>
        /// The name of the machine this instance is running on.
        /// </summary>
        string MachineName { get; }

        /// <summary>
        /// The name of the node when running in a WebFarm environment
        /// or the name of the machine when not running in a farm.
        /// </summary>
        string NodeName { get; }
    }
}
