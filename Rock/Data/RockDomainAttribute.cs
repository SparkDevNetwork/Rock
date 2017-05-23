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

namespace Rock.Data
{
    /// <summary>
    /// Custom attribute used to decorate model to help group them
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class RockDomainAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the rock domain name.
        /// </summary>
        /// <value>
        /// The rock domain name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDomainAttribute"/> class.
        /// </summary>
        public RockDomainAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDomainAttribute" /> class.
        /// </summary>
        /// <param name="name">The defined type GUID.</param>
        public RockDomainAttribute( string name )
        {
            Name = name;
        }

    }
}
