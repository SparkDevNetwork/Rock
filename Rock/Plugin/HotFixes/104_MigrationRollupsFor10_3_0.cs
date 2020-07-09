﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 104, "1.10.0" )]
    public class MigrationRollupsFor10_3_0 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddUpgradeSQLServerMessage();
            FixAgeCalcError();
            AttendanceAnalyticsSortAttendeesByCampus();
            UpdateSampleDataUrl();
            RemoveGlobalAttributeRouteDomainMatching();
            Issue3478();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// MB: [Migration] Adds Upgrade to SQL Server 2014 message.
        /// </summary>
        private void AddUpgradeSQLServerMessage()
        {
            Sql( @"
                IF NOT EXISTS(SELECT 1 FROM [dbo].[DefinedValue] WHERE [Guid] = '0b16bd4b-f55b-4adb-a744-fc4751731a7d')
                BEGIN

                -- Get Defined Type Id
                DECLARE @DefinedTypeId AS INT
                SELECT @DefinedTypeId = Id
                FROM DefinedType
                WHERE [Guid] = '4BF34677-37E9-4E71-BD03-252B66C9373D'

                -- Get Completed Attribute Id
                DECLARE @CompletedAttributeId AS INT
                SELECT @CompletedAttributeId = Id
                FROM Attribute
                WHERE [Guid] = 'FBB2E564-29A3-4756-A255-38565B486000'

                -- Make room at the top of the list.
                UPDATE DefinedValue
                SET [Order] = [Order] + 1
                WHERE DefinedTypeId = @DefinedTypeId

                -- Insert new item into list.
                INSERT INTO [dbo].[DefinedValue]
                    ([IsSystem]
                    ,[DefinedTypeId]
                    ,[Order]
                    ,[Value]
                    ,[Description]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[ModifiedDateTime]
                    ,[IsActive])
                VALUES
                    (1
                    ,@DefinedTypeId
                    ,0
                    ,'Upgrade to SQL Server 2014 or Later'
                    ,'Please remember that starting with v11 Rock will no longer support SQL Server 2012 (see this <a href=''https://community.rockrms.com/connect/ending-support-for-sql-server-2012''>link</a> for more details).'
                    ,'0B16BD4B-F55B-4ADB-A744-FC4751731A7D'
                    ,GETDATE()
                    ,GETDATE()
                    ,1)

                DECLARE @DefinedValueId AS INT
                SET @DefinedValueId = @@IDENTITY

                DECLARE @IsCompleted AS NVARCHAR(20)
                SET @IsCompleted = 'False'

                -- Calculate IsCompleted Value
                DECLARE @SqlServerVersion AS NVARCHAR(20) 
                SET @SqlServerVersion = CONVERT(NVARCHAR(20), SERVERPROPERTY('productversion'))
                SET @SqlServerVersion = SUBSTRING(@SqlServerVersion, 1, CHARINDEX('.', @SqlServerVersion) - 1)

                IF CAST(@SqlServerVersion AS INT) > 11
                BEGIN
	                SET @IsCompleted = 'True'
                END

                -- Insert Completed Attribute Value
                INSERT INTO [dbo].[AttributeValue]
                    ([IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Value]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[ModifiedDateTime])
                VALUES
                    (0
                    ,@CompletedAttributeId
                    ,@DefinedValueId
                    ,@IsCompleted
                    ,'125EB24D-7BFC-4440-AD8C-014FB49EB95E'
                    ,GETDATE()
                    ,GETDATE())
                END"
            );
        }

        /// <summary>
        /// MB: [Migration] Fixes Age Calculation error ISSUE #3881s
        /// </summary>
        private void FixAgeCalcError()
        {
            Sql( @"/*
<doc>
	<summary>
 		This function returns the age given a birthdate.
	</summary>

	<returns>
		The age based on birthdate
	</returns>
	<remarks>
		
	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAge]( '2000-01-01')
	</code>
</doc>
*/

ALTER FUNCTION [dbo].[ufnCrm_GetAge](@BirthDate date) 

RETURNS INT WITH SCHEMABINDING AS

BEGIN

	DECLARE @Age INT
	DECLARE @CurrentDate AS DATE
	SET @CurrentDate = GETDATE()

	-- Year 0001 is a special year, which denotes no year selected therefore we shouldn't calculate the age.
	IF @BirthDate IS NOT NULL AND DATEPART( year, @BirthDate ) > 1
	BEGIN

		SET @Age = DATEPART( year, @CurrentDate ) - DATEPART( year, @BirthDate )
		IF @BirthDate > DATEADD( year, 0 - @Age, @CurrentDate )
		BEGIN
			SET @Age = @Age - 1
		END

	END
		
	RETURN @Age

END" );
        }

        /// <summary>
        /// SK:  [Migration] Fixed Attendance Analytics block to sort attendees grid by Campus
        /// </summary>
        private void AttendanceAnalyticsSortAttendeesByCampus()
        {
            Sql( HotFixMigrationResource._104_MigrationRollupsFor10_3_0_spCheckin_AttendanceAnalyticsQuery_AttendeeLastAttendance );
            Sql( HotFixMigrationResource._104_MigrationRollupsFor10_3_0_spCheckin_AttendanceAnalyticsQuery_NonAttendees );
        }

        /// <summary>
        /// SK: Updated Sample Data Url
        /// </summary>
        private void UpdateSampleDataUrl()
        {
            RockMigrationHelper.AddBlockAttributeValue( "34CA1FA0-F8F1-449F-9788-B5E6315DC058", "5E26439E-4E98-45B1-B19B-D5B2F3405963", @"http://storage.rockrms.com/sampledata/sampledata_1_7_0.xml" );
        }

        /// <summary>
        /// ED/NA: Remove reverted Global Attribute : Enable Route Domain Matching
        /// </summary>
        private void RemoveGlobalAttributeRouteDomainMatching()
        {
            // This was reverted by MP https://github.com/sparkdevnetwork/rock/commit/7d5461970d2296170e57e676477dafd322b308c6 but could have been inserted from pre-alpha-release.
            // Remove the reverted Global Attribute : Enable Route Domain Matching
            RockMigrationHelper.DeleteAttribute( "0B7DD63E-AD00-445E-8E9D-047956FEAFB3" );    // Global Attribute : Enable Route Domain Matching
        }

        /// <summary>
        /// MB: [Migration] Fixes Issue 3478
        /// </summary>
        private void Issue3478()
        {
            Sql( HotFixMigrationResource._104_MigrationRollupsFor10_3_0_spFinance_PledgeAnalyticsQuery );
        }

    }
}
