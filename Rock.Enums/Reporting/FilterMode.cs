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

namespace Rock.Reporting
{
    /// <summary>
    /// The operating mode the filter should use when displaying itself.
    /// </summary>
    [Enums.EnumDomain( "Reporting" )]
    public enum FilterMode
    {
        /// <summary>
        /// Render the UI and process the filter as a simple filter.
        /// This mode can be set if the filter just needs to be simple with minimal UI (like on a public page).
        /// </summary>
        SimpleFilter,

        /// <summary>
        /// Render and process as an advanced filter.
        /// This will be the mode when configuring as a Data Filter.
        /// </summary>
        AdvancedFilter
    }
}
