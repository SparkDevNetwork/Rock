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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Fix for issue #6959. In [spCrm_PersonDuplicateFinder] there are three
    /// UPDATE statements that join the Person table to itself to match on
    /// BirthDate, MaritalStatusValueId, and SuffixValueId. Those columns are
    /// nullable, and when a lot of rows have NULL in them (which can happen
    /// on larger databases where these fields aren't consistently captured)
    /// the query slows down badly. SQL Server can pick a join plan that
    /// walks through all the NULL rows first and only afterward realizes
    /// none of them can match (in SQL, NULL never equals NULL). On the
    /// database that triggered this issue, one of the three statements ran
    /// for about two hours.
    ///
    /// The fix adds "AND column IS NOT NULL" for both sides of each of the
    /// three joins. That tells SQL Server up front to skip the NULL rows,
    /// so it can use the existing index on the column and finish quickly.
    /// It doesn't change what rows are returned (NULLs were already being
    /// filtered out by the equality check); it just gets there faster. The
    /// same pattern is already used a few lines earlier in the procedure
    /// for LastName and Campus, so this brings BirthDate, MaritalStatus,
    /// and Suffix into line with them.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 313, "19.5" )]
    public class FixPersonDuplicateFinderNullSelfJoinPerformance6959 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( HotFixMigrationResource._313_FixPersonDuplicateFinderNullSelfJoinPerformance6959_spCrm_PersonDuplicateFinder );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
