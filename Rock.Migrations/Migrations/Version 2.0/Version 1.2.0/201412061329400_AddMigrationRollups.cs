// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class AddMigrationRollups : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    alter table AttributeValue drop column ValueAsNumeric;
    alter table AttributeValue add ValueAsNumeric as (case when len([value])<(100) AND isnumeric([value])=(1) AND NOT [value] like '%[^0-9.]%' AND NOT [value] like '%[.]%' then CONVERT([numeric](38,10),[value])  end)
" );

            // update new location of checkinclient installer
            Sql( @"
    UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.2.0/checkinclient.exe' where [Guid] = '7ADC1B5B-D374-4B77-9DE1-4D788B572A10'
" );

            // update AttributeQualifer of "binaryFileType" to use Guid instead of Id
            try
            {
                Sql( @"
    BEGIN
        DECLARE @attributeQualifierId INT
            ,@attributeQualiferValue NVARCHAR(max)

        DECLARE binaryFileQualifierCursor CURSOR FAST_FORWARD
        FOR
        SELECT Id
            ,Value
        FROM AttributeQualifier
        WHERE [Key] = 'binaryFileType'
            AND [Value] IN (
                SELECT cast(Id AS VARCHAR(max))
                FROM BinaryFileType
                )

        OPEN binaryFileQualifierCursor;

        FETCH NEXT
        FROM binaryFileQualifierCursor
        INTO @attributeQualifierId
            ,@attributeQualiferValue

        WHILE @@FETCH_STATUS = 0
        BEGIN
            UPDATE AttributeQualifier set Value = (select top 1 [Guid] from BinaryFileType where Id = @attributeQualiferValue) where Id = @attributeQualifierId
        
            FETCH NEXT
            FROM binaryFileQualifierCursor
            INTO @attributeQualifierId
                ,@attributeQualiferValue
        END

        CLOSE binaryFileQualifierCursor;
        DEALLOCATE binaryFileQualifierCursor;
    END
" );
            }
            catch
            {
                // ignore if the SQL failed
            }

            Sql( @"
    -- create stored proc for attendance duration
    /*
    <doc>
	    <summary>
 		    This function returns the number of weekends a member of a family has attended a weekend service
		    in the last X weeks.
	    </summary>

	    <returns>
		    * Number of weeks
	    </returns>
	    <param name=""PersonId"" datatype=""int"">The person id to use</param>
	    <param name=""WeekDuration"" datatype=""int"">The number of weeks to use as the duration (default 16)</param>
	    <remarks>	
	    </remarks>
	    <code>
		    EXEC [dbo].[spCheckin_WeeksAttendedInDuration] 2 -- Ted Decker
	    </code>
    </doc>
    */

    ALTER PROCEDURE [dbo].[spCheckin_WeeksAttendedInDuration]
	    @PersonId int
	    ,@WeekDuration int = 16
    AS
    BEGIN
	
        DECLARE @LastSunday datetime 

        SET @LastSunday = [dbo].[ufnUtility_GetPreviousSundayDate]()

        SELECT 
	        COUNT(DISTINCT dbo.ufnUtility_GetSundayDate(a.[StartDateTime]) )
        FROM
	        [Attendance] a
	        INNER JOIN [PersonAlias] pa ON pa.[Id] = a.[PersonAliasId]
        WHERE 
	        [GroupId] IN (SELECT [Id] FROM [dbo].[ufnCheckin_WeeklyServiceGroups]())
	        AND pa.[PersonId] IN (SELECT [Id] FROM [dbo].[ufnCrm_FamilyMembersOfPersonId](@PersonId))
	        AND a.[StartDateTime] BETWEEN DATEADD(WEEK, (@WeekDuration * -1), @LastSunday) AND DATEADD(DAY, 1, @LastSunday)

    END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
