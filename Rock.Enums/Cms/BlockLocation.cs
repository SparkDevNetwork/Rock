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

namespace Rock.Model
{
    /// <summary>
    /// The location where the Block is implemented
    /// </summary>
    [Serializable]
    [Enums.EnumDomain( "Cms" )]
    public enum BlockLocation
    {
        /// <summary>
        /// Block is located in the layout (will be rendered for every page using the layout)
        /// </summary>
        Layout = 0,

        /// <summary>
        /// Block is located on the page
        /// </summary>
        Page = 1,

        /// <summary>
        /// Block is located in the site (will be rendered for every page of the site)
        /// </summary>
        Site = 2,

        /// <summary>
        /// Block is doesn't have a PageId, LayoutId, or a SiteId specific (shouldn't happen, but just in case)
        /// </summary>
        None = 3
    }
}
