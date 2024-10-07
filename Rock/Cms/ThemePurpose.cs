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

namespace Rock.Cms
{
    /// <summary>
    /// The purpose of the theme. This identifies which kinds of sites can
    /// use this theme.
    /// </summary>
    /// <remarks>
    /// If this is made public, it should be moved to Rock.Enums.Cms.
    /// </remarks>
    internal enum ThemePurpose
    {
        /// <summary>
        /// Theme should be used with web sites.
        /// </summary>
        Web = 0,

        /// <summary>
        /// Theme should be used with web check-in sites.
        /// </summary>
        Checkin = 1
    }
}
