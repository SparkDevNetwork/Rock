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

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn.v2
{
    /// <summary>
    /// This provides the server check-in data for a single check-in group.
    /// This should not be sent down to clients as it contains additional
    /// data they should not see.
    /// </summary>
    /// <remarks>
    /// Some of this data also exists as properties on the <see cref="GroupTypeCache"/>
    /// object. But we copy it to this class so that we have everything
    /// required for check-in self-contained.
    /// </remarks>
    internal class AreaConfigurationData
    {
        #region Properties

        /// <summary>
        /// Gets the attendance rule used for check-in.
        /// </summary>
        /// <value>The attendance rule.</value>
        public virtual AttendanceRule AttendanceRule { get; }

        /// <summary>
        /// Gets where labels should be printed for groups in this area.
        /// </summary>
        /// <value>The printed label destination.</value>
        public virtual PrintTo PrintTo { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaConfigurationData"/> class.
        /// </summary>
        /// <remarks>
        /// This is meant to be used by unit tests only.
        /// </remarks>
        protected AreaConfigurationData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaConfigurationData"/> class.
        /// </summary>
        /// <param name="groupTypeCache">The group cache.</param>
        /// <param name="rockContext">The rock context.</param>
        internal AreaConfigurationData( GroupTypeCache groupTypeCache, RockContext rockContext )
        {
            AttendanceRule = groupTypeCache.AttendanceRule;
            PrintTo = groupTypeCache.AttendancePrintTo;
        }

        #endregion
    }
}
