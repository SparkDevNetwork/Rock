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
    public partial class WorkflowNotes2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Workflow", "Workflow Note Type", "The type of notes that can be associated with a workflow.", "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782" );
            RockMigrationHelper.AddDefinedTypeAttribute( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Icon Class Name", "IconClass", "The class name to use when rendering an icon for notes of this type", 0, "", "629CFBFF-3A95-4294-B13C-37F4FED04FE7" );

            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "User Note", "User entered note", "534489FB-E239-4C51-8F5D-9ECF85E9CDE2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "534489FB-E239-4C51-8F5D-9ECF85E9CDE2", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-comment" );

            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "System Note", "System entered note", "414E9F98-4709-4895-AEBA-E41773BB7EB8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "414E9F98-4709-4895-AEBA-E41773BB7EB8", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-file-text" );

            Sql( @"
    DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'FDC7A191-717E-4CA6-9DCF-A2B5BB09C782' )
    DECLARE @WorkflowEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '3540E9A7-FE30-43A9-8B0A-A372B63DFC93' )
    IF @DefinedTypeId IS NOT NULL AND @WorkflowEntityTypeId IS NOT NULL 
    BEGIN
	    IF EXISTS ( SELECT [Id] FROM [NoteType] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND ( [Name] = 'WorkflowNote' OR [Name] = 'Workflow Note' ) )
	    BEGIN
		    UPDATE [NoteType] SET 
    			[IsSystem] = 1,
                [Name] = 'Workflow Note',
			    [SourcesTypeId] = @DefinedTypeId,
			    [Guid] = 'A6CE445C-3B49-4401-82E6-312BF7946A6B'
		    WHERE [EntityTypeId] = @WorkflowEntityTypeId 
		    AND ( [Name] = 'WorkflowNote' OR [Name] = 'Workflow Note' )
	    END
	    ELSE
	    BEGIN
		    INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [Name], [SourcesTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Guid] )
		    VALUES ( 1, @WorkflowEntityTypeId, 'Workflow Note', @DefinedTypeId, '', '', 'A6CE445C-3B49-4401-82E6-312BF7946A6B')
	    END
    END
" );

            Sql( @"
    UPDATE Q 
	    SET [Value] = 'General^General Inquiry,
Login^Login / Username / Password Assistance,
Website^Feedback about the web site,
Finance^Contributions / Finance,
Missions^Missions / Global Trips,
Pastor^Talk to a Pastor'
    FROM [Attribute] A
    INNER JOIN [AttributeQualifier] Q 
	    ON Q.[AttributeId] = A.[Id]
	    AND Q.[Key] = 'values'
    WHERE A.[Guid] = 'DA61CA95-0106-49EE-962B-F70042E1464E'
    AND Q.[Value] = 'General:General Inquiry,
Login:Login / Username / Password Assistance,
Website:Feedback about the web site,
Finance:Contributions / Finance,
Missions:Missions / Global Trips,
Pastor:Talk to a Pastor'
" );

            // Add Block to Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "BA547EED-5537-49CF-BD4E-C583D760788C", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 1, "3A289F81-3048-419B-8A78-2B15967CC42B" );
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-comment" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"3540e9a7-fe30-43a9-8b0a-a372b63dfc93" );
            // Attrib Value for Block:Notes, Attribute:Note Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6", @"Workflow Note" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );

            // Add/Update PageContext for Page:Workflow Detail, Entity: Rock.Model.Workflow, Parameter: workflowid
            RockMigrationHelper.UpdatePageContext( "BA547EED-5537-49CF-BD4E-C583D760788C", "Rock.Model.Workflow", "workflowid", "55B1F94F-6498-4616-A1EC-4891E3FF2299" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Notes, from Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3A289F81-3048-419B-8A78-2B15967CC42B" );

            // Delete PageContext for Page:Workflow Detail, Entity: Rock.Model.Workflow, Parameter: workflowid
            RockMigrationHelper.DeletePageContext( "55B1F94F-6498-4616-A1EC-4891E3FF2299" );

        }
    }
}
