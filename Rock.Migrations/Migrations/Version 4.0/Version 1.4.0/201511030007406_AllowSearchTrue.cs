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
    public partial class AllowSearchTrue : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // NA: AllowSearch = true
            // Set the standard connection opportunity attributes to AllowSearch
            Sql( @"UPDATE 
	[dbo].[Attribute] 
SET 
	[AllowSearch] = 1 
WHERE 
	[Guid] IN ( N'02DE7773-34AC-4A9E-A21B-0453DC2494D5', N'378B49A5-CC8C-4346-AE78-DCD087325150', N'12CC78B6-5571-46B7-8234-69D9FB4CF022' )" );

            // DT: Add new page settings to group member list/detail blocks
            // Attrib for BlockType: Group Member List:Registration Page
            RockMigrationHelper.AddBlockTypeAttribute( "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", "Page used for viewing the registration(s) associated with a particular group member", 3, @"", "EDF79295-04A4-42B4-B382-DDEF5888D565" );
            // Attrib for BlockType: Group Member Detail:Registration Page
            RockMigrationHelper.AddBlockTypeAttribute( "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", "Page used for viewing the registration(s) associated with a particular group member", 0, @"", "2EDA5282-EA3E-446F-9CD6-5B3F323FC245" );
            // Attrib Value for Block:Group Member List, Attribute:Registration Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BA0F3E7D-1C3A-47CB-9058-893DBAA35B89", "EDF79295-04A4-42B4-B382-DDEF5888D565", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );
            // Attrib Value for Block:Group Member Detail, Attribute:Registration Page Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C66D11C8-DA55-40EA-925C-C9D7AC71F879", "2EDA5282-EA3E-446F-9CD6-5B3F323FC245", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );

            // DT: Fix divide-by-zero error in person dup finder
            Sql( @"DROP INDEX [IX_ConfidenceScore] on [PersonDuplicate]

ALTER TABLE [PersonDuplicate] DROP COLUMN [ConfidenceScore]

ALTER TABLE [PersonDuplicate] ADD [ConfidenceScore] AS (
	sqrt (
		( CASE WHEN [TotalCapacity] > 0 
			THEN [Capacity] / ( [TotalCapacity] * 0.01 ) 
			ELSE 0 END )
		*
		( CASE WHEN [Capacity] > 0 
			THEN [Score] / ( [Capacity] * 0.01 ) 
			ELSE 0 END )
		)
    ) PERSISTED

CREATE INDEX [IX_ConfidenceScore] on [PersonDuplicate] (ConfidenceScore)" );


            // DT: Add new Location setting page
            RockMigrationHelper.AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Location Settings", "", "4CE2A5DA-15F3-454C-8172-D146D938E203", "" ); // Site:Rock RMS
            // Add Block to Page: Location Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4CE2A5DA-15F3-454C-8172-D146D938E203", "", "08189564-1245-48F8-86CC-560F4DD48733", "Location Detail", "Main", "", "", 0, "27982E23-332F-4285-AD5C-F0237C0F0BF1" );
            // Attrib for BlockType: Family Members:Location Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Location Detail Page", "LocationDetailPage", "", "Page used to edit the settings for a particular location.", 0, @"", "A1C5EAB7-B507-4DB7-916D-64A58EEF8691" );
            // Attrib Value for Block:Family Members, Attribute:Location Detail Page , Layout: PersonDetail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC", "A1C5EAB7-B507-4DB7-916D-64A58EEF8691", @"4ce2a5da-15f3-454c-8172-d146d938e203" );

            // Update content channel query filter param to enable filtering for detail channel blocks
            Sql( @"
    DECLARE @TemplateAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '8026FEA1-35C1-41CF-9D09-E8B1DB6CBDA8' )
    DECLARE @QueryParamAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'AA9CD867-FA21-43C2-822B-CAC06E1F18B8' )

    IF ( @TemplateAttributeId IS NOT NULL AND @QueryParamAttributeId IS NOT NULL )
    BEGIN

	    UPDATE Q SET [Value] = 'True'
	    FROM [AttributeValue] T
	    INNER JOIN [AttributeValue] Q
		    ON Q.[AttributeId] = @QueryParamAttributeId
		    AND Q.[entityid] = T.[EntityId]
	    WHERE T.[AttributeId] = @TemplateAttributeId
	    AND ( T.[Value] = '{% include ''~~/Assets/Lava/AdDetails.lava'' %}' OR	T.[Value] = '{% include ''~~/Assets/Lava/BlogItemDetail.lava'' %}' )

	    INSERT INTO [AttributeValue] ( [IsSystem],[AttributeId],[EntityId],[VAlue],[Guid] )
	    SELECT 
		    1,
		    @QueryParamAttributeId,
		    T.[EntityId],
		    'True',
		    NEWID()
	    FROM [AttributeValue] T
	    LEFT OUTER JOIN [AttributeValue] Q
		    ON Q.[AttributeId] = @QueryParamAttributeId
		    AND Q.[entityid] = T.[EntityId]
	    WHERE T.[AttributeId] = @TemplateAttributeId
	    AND ( T.[Value] = '{% include ''~~/Assets/Lava/AdDetails.lava'' %}' OR	T.[Value] = '{% include ''~~/Assets/Lava/BlogItemDetail.lava'' %}' )
	    AND Q.[Id] IS NULL

    END
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // DT: Add new Location setting page
            // Attrib for BlockType: Family Members:Location Detail Page
            RockMigrationHelper.DeleteAttribute( "A1C5EAB7-B507-4DB7-916D-64A58EEF8691" );
            // Remove Block: Location Detail, from Page: Location Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "27982E23-332F-4285-AD5C-F0237C0F0BF1" );
            RockMigrationHelper.DeletePage( "4CE2A5DA-15F3-454C-8172-D146D938E203" ); //  Page: Location Settings, Layout: Full Width, Site: Rock RMS
            
            // DT: Add new page settings to group member list/detail blocks
            // Attrib for BlockType: Group Member Detail:Registration Page
            RockMigrationHelper.DeleteAttribute( "2EDA5282-EA3E-446F-9CD6-5B3F323FC245" );
            // Attrib for BlockType: Group Member List:Registration Page
            RockMigrationHelper.DeleteAttribute( "EDF79295-04A4-42B4-B382-DDEF5888D565" );
        }
    }
}
