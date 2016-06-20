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
    public partial class WeekendAttendance : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.GroupType", "AttendanceCountsAsWeekendService", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Location", "StandardizeAttemptedResult", c => c.String(maxLength: 200));
            AlterColumn("dbo.Location", "GeocodeAttemptedResult", c => c.String(maxLength: 200));

            Sql( @"
    -- Update the AttendanceCountsAsWeekendService flag
    ;WITH CTE AS (

	    SELECT [Id]
	    FROM [GroupType]
	    WHERE [GroupTypePurposeValueId] in (
		    SELECT [Id] 
		    FROM [DefinedValue] 
		    WHERE [Guid] in ('6BCED84C-69AD-4F5A-9197-5C0F9C02DD34', '4A406CB0-495B-4795-B788-52BDFDE00B01')
	    )

	    UNION ALL

	    SELECT g.[Id]
	    FROM [GroupType] g
	    INNER JOIN CTE ON g.[InheritedGroupTypeId] = CTE.[Id]
    )

    UPDATE G SET [AttendanceCountsAsWeekendService] = 1
    FROM CTE 
    INNER JOIN [GroupType] G ON G.[Id] = CTE.[Id]
" );

            Sql( @"
    /*
    <doc>
	    <summary>
 		    This function returns all group types that are used to denote 
		    groups that are for tracking attendance for weekly services
	    </summary>

	    <returns>
		    * GroupTypeId
		    * Guid
		    * Name
	    </returns>

	    <code>
		    SELECT * FROM [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
	    </code>
    </doc>
    */


    ALTER FUNCTION [dbo].[ufnCheckin_WeeklyServiceGroupTypes]()
    RETURNS TABLE AS

    RETURN ( 
	    SELECT [Id], [Guid], [Name]
	    FROM [GroupType] 
	    WHERE [AttendanceCountsAsWeekendService] = 1
    )
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.Location", "GeocodeAttemptedResult", c => c.String(maxLength: 50));
            AlterColumn("dbo.Location", "StandardizeAttemptedResult", c => c.String(maxLength: 50));
            DropColumn("dbo.GroupType", "AttendanceCountsAsWeekendService");
        }
    }
}
