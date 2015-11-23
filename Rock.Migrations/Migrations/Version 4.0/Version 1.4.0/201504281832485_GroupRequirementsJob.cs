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
    using Rock.Security;

    /// <summary>
    ///
    /// </summary>
    public partial class GroupRequirementsJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BENEVOLENCE_FUNCTIONS, 2, Authorization.VIEW, false, null, Rock.Model.SpecialRole.AllUsers.ConvertToInt(), "DB55261E-59AA-44D1-BC90-E5BE2A7798FF" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BENEVOLENCE_FUNCTIONS, 1, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "DB185E0D-6C34-4B8E-9EA4-27198049D6C0" );
            RockMigrationHelper.AddSecurityAuthForPage( Rock.SystemGuid.Page.BENEVOLENCE_FUNCTIONS, 0, Authorization.VIEW, true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, 0, "39899A73-77BD-48D7-9F62-676490A4C4AB" );

            // Benevolence Request Detail
            RockMigrationHelper.AddSecurityAuthForBlock( "596CE410-99BF-420F-A86E-CFFDF0BB45F3", 0, Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "B6F3C992-CF00-47FF-B3DB-FE6360CBC184" );

            // Benevolence Request List
            RockMigrationHelper.AddSecurityAuthForBlock( "76519A99-2E29-4481-95B8-DCFF8E3225A1", 0, Authorization.EDIT, true, Rock.SystemGuid.Group.GROUP_BENEVOLENCE, 0, "4435AC68-7452-4ADC-809B-E4F35B846722" );

            // Calculate Metrics Job
            Sql( @"
IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[ServiceJob]
    WHERE [Class] = 'Rock.Jobs.CalculateMetrics'
)
BEGIN
    INSERT INTO [ServiceJob]
                    ([IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Assembly]
                    ,[Class]
                    ,[CronExpression]
                    ,[Guid]
		            ,[NotificationStatus])
                VALUES
                    (1
                    ,1
                    ,'Calculate Metrics'
                    ,'A job that processes any metrics with schedules.'
                    ,''
                    ,'Rock.Jobs.CalculateMetrics'
                    ,'0 0/15 * 1/1 * ? *'
                    ,'3425AE05-354C-4C0E-AF5E-50CD6A7F8740'
		            ,3)
END
" );

            // Calculate Group Requirements Job
            Sql( @"
IF NOT EXISTS (
    SELECT [Id] FROM [dbo].[ServiceJob]
    WHERE [Class] = 'Rock.Jobs.CalculateGroupRequirements'
)
BEGIN
    INSERT INTO [ServiceJob]
                    ([IsSystem]
                    ,[IsActive]
                    ,[Name]
                    ,[Description]
                    ,[Assembly]
                    ,[Class]
                    ,[CronExpression]
                    ,[Guid]
		            ,[NotificationStatus])
                VALUES
                    (1
                    ,1
                    ,'Calculate Group Requirements'
                    ,'Calculate Group Requirements for group members that are in groups that have group requirements.'
                    ,''
                    ,'Rock.Jobs.CalculateGroupRequirements'
                    ,'0 0 3 1/1 * ? *'
                    ,'ADC8FE8B-2C7D-46A4-885D-3EBB811DC03F'
		            ,3)
END
" );


            // DT: Update all global attribute lava mergfields in the attribute defaultvalue and attributevalue value fields to use the new syntax.
            Sql(@"
	-- Get all the affected attributes ( to make updates in cursor faster )
	SELECT [Id]
	INTO #AttributeIds
	FROM [Attribute]
	WHERE [DefaultValue] LIKE '%GlobalAttribute.%'

	-- Get all the affected attribute values ( to make updates in cursor faster )
	SELECT [Id]
	INTO #AttributeValueIds
	FROM [AttributeValue]
	WHERE [Value] LIKE '%GlobalAttribute.%'

	-- Create a cursor that selects both the old and new ways of referencing a global attribute, making 
	-- sure to order them by longest to shortest key in case one attribute's key partially contains another 
	-- attributes full key. i.e. don't do a replace on 'EmailHeader' before doing a replace on 'EmailHeaderLogo'
	DECLARE @OldWay varchar(300)
	DECLARE @NewWay varchar(300)
	DECLARE RockCursor INSENSITIVE CURSOR FOR
	WITH CTE
	AS 
	(
		SELECT 
			'GlobalAttribute.' + [Key] AS [OldWay],
			'''Global'' | Attribute:''' + [Key] + '''' AS [NewWay],
			LEN([Key])  AS [KeySize]
		FROM [Attribute]
		WHERE [EntityTypeId] IS NULL
	)
	SELECT [OldWay], [NewWay]
	FROM [CTE] WITH (NOLOCK)
	ORDER BY [KeySize] DESC

	OPEN RockCursor
	FETCH NEXT FROM RockCursor
	INTO @OldWay, @NewWay

	-- For each global attribute replace any that are referenced the old way with the new way
	WHILE (@@FETCH_STATUS <> -1)
	BEGIN

		IF (@@FETCH_STATUS = 0)
		BEGIN

			-- Update the attribute default values
			UPDATE [Attribute] 
			SET [DefaultValue] = REPLACE([DefaultValue], @OldWay, @NewWay) 
			WHERE [Id] IN ( SELECT [Id] FROM #AttributeIds )
			AND [DefaultValue] LIKE '%' + @OldWay + '%'

			-- Udpate the attribute values
			UPDATE [AttributeValue] 
			SET [Value] = REPLACE([Value], @OldWay, @NewWay) 
			WHERE [Id] IN ( SELECT [Id] FROM #AttributeValueIds )
			AND [Value] LIKE '%' + @OldWay + '%'
		
			FETCH NEXT FROM RockCursor
			INTO @OldWay, @NewWay

		END
	
	END

	CLOSE RockCursor
	DEALLOCATE RockCursor

	DROP TABLE #AttributeIds
	DROP TABLE #AttributeValueIds
" );


        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSecurityAuth( "DB55261E-59AA-44D1-BC90-E5BE2A7798FF" );
            RockMigrationHelper.DeleteSecurityAuth( "DB185E0D-6C34-4B8E-9EA4-27198049D6C0" );
            RockMigrationHelper.DeleteSecurityAuth( "39899A73-77BD-48D7-9F62-676490A4C4AB" );
            RockMigrationHelper.DeleteSecurityAuth( "B6F3C992-CF00-47FF-B3DB-FE6360CBC184" );
            RockMigrationHelper.DeleteSecurityAuth( "4435AC68-7452-4ADC-809B-E4F35B846722" );
        }
    }
}
