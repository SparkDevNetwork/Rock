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

using Rock.Utility;

namespace Rock.Attribute
{
    /// <summary>
    /// Defines the Rock platforms that are supported by a type. A type can support
    /// multiple platforms.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false )]
    public class RockPlatformSupportAttribute : System.Attribute
    {
        /// <summary>
        /// The platforms that are supported on the type decorated by this attribute.
        /// </summary>
        public RockPlatform Platform { get; set; }

        /// <summary>
        /// Defines the Rock platforms that are supported by a type. A type can support
        /// multiple platforms.
        /// </summary>
        /// <param name="platform"></param>
        public RockPlatformSupportAttribute( params RockPlatform[] platform )
        {
            foreach ( var p in platform )
            {
                Platform |= p;
            }
        }
    }
}
