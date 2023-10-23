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
using System.Collections.Generic;

namespace Rock.Blocks
{
    /// <summary>
    /// Defines the site types that are supported by a block.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class SupportedSiteTypesAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the site types that the block type supports.
        /// </summary>
        /// <value>The site types that the block type supports.</value>
        public IEnumerable<Model.SiteType> SiteTypes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportedSiteTypesAttribute"/> class.
        /// </summary>
        /// <param name="siteTypes">The supported site types.</param>
        public SupportedSiteTypesAttribute( params Model.SiteType[] siteTypes )
        {
            SiteTypes = siteTypes;
        }
    }
}
