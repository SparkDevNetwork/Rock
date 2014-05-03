// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

namespace Rock.Plugin
{
    /// <summary>
    /// Attribute for defining the plugin version and minimum rock version for a plugin's migration
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple=false)]
    public class PluginVersion : System.Attribute
    {
        /// <summary>
        /// Gets or sets the plugin version for this migration (Migrations will executed in version order)
        /// </summary>
        /// <value>
        /// The plugin version.
        /// </value>
        public Version Version {get; set;}

        /// <summary>
        /// Gets or sets the minimum rock version for this migration 
        /// </summary>
        /// <value>
        /// The minimum rock version.
        /// </value>
        public Version MinimumRockVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginVersion"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="minimumRockVersion">The minimum rock version.</param>
        public PluginVersion( string version, string minimumRockVersion )
            : base()
        {
            Version = new Version( version );
            MinimumRockVersion = new Version( minimumRockVersion );
        }
    }
}
