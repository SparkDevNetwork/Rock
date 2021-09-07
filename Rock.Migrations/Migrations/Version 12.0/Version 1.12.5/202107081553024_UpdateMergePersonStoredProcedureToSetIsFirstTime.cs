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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateMergePersonStoredProcedureToSetIsFirstTime : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._202107081553024_UpdateMergePersonStoredProcedureToSetIsFirstTime_spCrm_PersonMerge );

            Sql( @"UPDATE Attendance
                    SET IsFirstTime = 0
                    WHERE Id IN (
	                    SELECT Id
	                    FROM (
		                    SELECT ROW_NUMBER() OVER(
				                    PARTITION BY PersonAliasId, IsFirstTime
				                    ORDER BY StartDateTime ASC) AS RowId, Attendance.Id
		                    FROM Attendance
		                    WHERE IsFirstTime = 1
	                    ) as t
	                    WHERE t.RowId > 1
                    )" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
