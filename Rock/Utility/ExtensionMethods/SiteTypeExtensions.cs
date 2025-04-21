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
using Rock.Enums.Cms;
using Rock.Model;

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="SiteType"/>.
    /// </summary>
    public static class SiteTypeExtensions
    {
        /// <summary>
        /// Converts the single site type into a <see cref="SiteTypeFlags"/>
        /// value.
        /// </summary>
        /// <param name="siteType">The single site type value.</param>
        /// <returns>The flag representation.</returns>
        public static SiteTypeFlags ConvertToFlags( this SiteType siteType )
        {
            if ( siteType == SiteType.Web )
            {
                return SiteTypeFlags.Web;
            }
            else if ( siteType == SiteType.Mobile )
            {
                return SiteTypeFlags.Mobile;
            }
            else if ( siteType == SiteType.Tv )
            {
                return SiteTypeFlags.Tv;
            }
            else
            {
                return SiteTypeFlags.None;
            }
        }
    }
}
