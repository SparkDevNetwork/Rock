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
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// GroupLocationHistoricalSchedule Service class
    /// </summary>
    [Obsolete( "Group Location Historical Schedule is not used and is not reflected in the UI.  Consider using 'History' entity instead." )]
    [RockObsolete( "1.16" )]
    public partial class GroupLocationHistoricalScheduleService : Service<GroupLocationHistoricalSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationHistoricalScheduleService"/> class
        /// </summary>
        /// <param name="context">The context.</param>
        public GroupLocationHistoricalScheduleService( RockContext context ) : base( context )
        {
        }
    }

    /// <summary>
    /// GroupLocationHistoricalSchedule Extension Methods class
    /// </summary>
    [Obsolete( "Group Location Historical Schedule is not used and is not reflected in the UI.  Consider using 'History' entity instead." )]
    [RockObsolete( "1.16" )]
    public static partial class GroupLocationHistoricalScheduleExtensionMethods
    {
    }
}
