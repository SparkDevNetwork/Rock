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

using Rock.Model;

namespace Rock.Attribute
{
    /// <summary>
    /// Decorates a class as a custom settings provider for the specified block
    /// type.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class CustomSettingsBlockTypeAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        /// <value>
        /// The type of the target.
        /// </value>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the type of the site these custom settings will apply to.
        /// </summary>
        /// <value>The type of the site these custom settings will apply to.</value>
        public SiteType? SiteType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target block type class.</param>
        public CustomSettingsBlockTypeAttribute( Type targetType )
        {
            TargetType = targetType;
            SiteType = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetTypeAttribute"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target block type class.</param>
        /// <param name="siteType">The type of site these custom settings will apply to or <c>null</c> to apply to all site types.</param>
        public CustomSettingsBlockTypeAttribute( Type targetType, SiteType siteType )
        {
            TargetType = targetType;
            SiteType = siteType;
        }
    }
}
