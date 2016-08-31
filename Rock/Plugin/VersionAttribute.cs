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
using System;

namespace Rock.Plugin
{
    /// <summary>
    /// Attribute for defining the a plugin migrations' number.  Migrations are execued in sequential order based on migration number
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class MigrationNumberAttribute : System.Attribute
    {

        /// <summary>
        /// Gets or sets the number.
        /// </summary>
        /// <value>
        /// The number.
        /// </value>
        public int Number { get; set; }

        /// <summary>
        /// Gets or sets the minimum rock version for this migration 
        /// </summary>
        /// <value>
        /// The minimum rock version.
        /// </value>
        public string MinimumRockVersion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationNumberAttribute"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="minimumRockVersion">The minimum rock version.</param>
        public MigrationNumberAttribute( int number, string minimumRockVersion )
            : base()
        {
            Number = number;
            MinimumRockVersion = minimumRockVersion;
        }
    }
}
