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
    public partial class MetricSchedules : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateEntityType( "Rock.Model.MetricCategory", "Metric Category", "Rock.Model.MetricCategory, Rock, Version=1.0.5.0, Culture=neutral, PublicKeyToken=null", true, true, "3D35C859-DF37-433F-A20A-0FFD0FCB9862" );
            
            AlterColumn( "dbo.Schedule", "Name", c => c.String( maxLength: 50 ) );

            // Add Schedule Category for Metrics
            Sql( @"
    DECLARE @MetricEntityTypeId INT
    SET @MetricEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Schedule')

    INSERT INTO [Category] ([IsSystem], [EntityTypeId], [Name], [Guid], [Order])
    VALUES (1, @MetricEntityTypeId, 'Metrics', '5A794741-5444-43F0-90D7-48E47276D426', 0)
" );

            AddDefinedType( "Metric", "Source Value Type", "The source of the data for a metric", "D6F323FF-6EF2-4DA7-A82C-61399AC1D798" );

            AddDefinedValue( "D6F323FF-6EF2-4DA7-A82C-61399AC1D798", "SQL", "The Metric Values are populated from custom SQL", "6A1E1A1B-A636-4E12-B90C-D7FD1BDAE764" );
            AddDefinedValue( "D6F323FF-6EF2-4DA7-A82C-61399AC1D798", "DataView", "The Metric Values are from a dataview", "2EC60BCF-EF63-4CCC-A970-F152292765D0" );
            AddDefinedValue( "D6F323FF-6EF2-4DA7-A82C-61399AC1D798", "Manual", "The Metric Values are manually entered", "1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E" );



            AddPage( "BB0ACD18-24FB-42BA-B89A-2FFD80472F5B", "6AC471A3-9B0E-459B-ADA2-F6E18F970803", "Metrics", "", "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53", "" ); // Site:Rock RMS
            AddPage( "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Metric Value Detail", "", "64E16878-D5AE-40A5-94FE-C2E8BE62DF61", "" ); // Site:Rock RMS
            UpdateBlockType( "Group Mapper", "Displays groups on a map.", "~/Blocks/Groups/GroupMapper.ascx", "Groups", "2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF" );
            UpdateBlockType( "Metric Detail", "Displays the details of the given metric.", "~/Blocks/Reporting/MetricDetail.ascx", "Reporting", "D77341B9-BA38-4693-884E-E5C1D908CEC4" );
            UpdateBlockType( "Metric Value List", "Displays a list of metric values.", "~/Blocks/Reporting/MetricValueList.ascx", "Reporting", "E40A1526-04D0-42A0-B275-D1AE161E2E57" );
            UpdateBlockType( "MetricValue Detail", "Displays the details of a particular metric value.", "~/Blocks/Reporting/MetricValueDetail.ascx", "Reporting", "508DA252-F94C-4641-8579-458D8FCE14B2" );

            // Add Block to Page: Metrics, Site: Rock RMS
            AddBlock( "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53", "", "ADE003C7-649B-466A-872B-B8AC952E7841", "Metrics Tree View", "Sidebar1", "", "", 0, "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF" );

            // Add Block to Page: Metrics, Site: Rock RMS
            AddBlock( "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53", "", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "Main", "", "", 0, "8DD6BBCD-518B-4492-9E44-721DEBC70884" );

            // Add Block to Page: Metrics, Site: Rock RMS
            AddBlock( "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53", "", "D77341B9-BA38-4693-884E-E5C1D908CEC4", "Metric Detail", "Main", "", "", 1, "F85FE71D-927D-45AF-B419-02A8909C6E72" );

            // Add Block to Page: Metrics, Site: Rock RMS
            AddBlock( "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53", "", "E40A1526-04D0-42A0-B275-D1AE161E2E57", "Metric Value List", "Main", "", "", 2, "6D58C677-B46A-48E1-9624-AB06A3D5AF1A" );

            // Add Block to Page: Metric Value Detail, Site: Rock RMS
            AddBlock( "64E16878-D5AE-40A5-94FE-C2E8BE62DF61", "", "508DA252-F94C-4641-8579-458D8FCE14B2", "Metric Value Detail", "Main", "", "", 0, "85CD3B5F-83B7-4786-B6CB-FC5F1887CA1A" );

            // Attrib for BlockType: Category Tree View:Entity Type Friendly Name
            AddBlockTypeAttribute( "ADE003C7-649B-466A-872B-B8AC952E7841", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Entity Type Friendly Name", "EntityTypeFriendlyName", "", "The text to show for the entity type name. Leave blank to get it from the specified Entity Type", 0, @"", "07213E2C-C239-47CA-A781-F7A908756DC2" );

            // Attrib for BlockType: Metric Value List:Detail Page
            AddBlockTypeAttribute( "E40A1526-04D0-42A0-B275-D1AE161E2E57", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "E789EF36-D2B6-417C-91BB-D42BE6645A36" );

            // Attrib Value for Block:Metrics Tree View, Attribute:Detail Page Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"78d84825-eb1a-43c6-9ad5-5f0f84cc9a53" );

            // Attrib Value for Block:Metrics Tree View, Attribute:Entity Type Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"3d35c859-df37-433f-a20a-0ffd0fcb9862" );

            // Attrib Value for Block:Metrics Tree View, Attribute:Entity Type Qualifier Property Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF", "2D5FF74A-D316-4924-BCD2-6AA338D8DAAC", @"" );

            // Attrib Value for Block:Metrics Tree View, Attribute:Entity type Qualifier Value Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF", "F76C5EEF-FD45-4BD6-A903-ED5AB53BB928", @"" );

            // Attrib Value for Block:Metrics Tree View, Attribute:Page Parameter Key Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF", "AA057D3E-00CC-42BD-9998-600873356EDB", @"MetricCategoryId" );

            // Attrib Value for Block:Metrics Tree View, Attribute:Entity Type Friendly Name Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF", "07213E2C-C239-47CA-A781-F7A908756DC2", @"Metric" );

            // Attrib Value for Block:Metric Value List, Attribute:Detail Page Page: Metrics, Site: Rock RMS
            AddBlockAttributeValue( "6D58C677-B46A-48E1-9624-AB06A3D5AF1A", "E789EF36-D2B6-417C-91BB-D42BE6645A36", @"64e16878-d5ae-40a5-94fe-c2e8be62df61" );

            // EntityType for metric category detail
            AddBlockAttributeValue( "8DD6BBCD-518B-4492-9E44-721DEBC70884", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", "3d35c859-df37-433f-a20a-0ffd0fcb9862" );

            /* Catch up Migrations */

            UpdateFieldType( "Day Of Week", "", "Rock", "Rock.Field.Types.DayOfWeekFieldType", "7EDFA2DE-FDD3-4AC1-B356-1F5BFC231DAE" );

            UpdateFieldType( "Days Of Week", "", "Rock", "Rock.Field.Types.DaysOfWeekFieldType", "08943FF9-F2A8-4DB4-A72A-31938B200C8C" );

            UpdateFieldType( "Remote Auths", "", "Rock", "Rock.Field.Types.RemoteAuthsFieldType", "ECA90666-E7A0-4406-8559-0153DCB908FD" );

            // Attrib for BlockType: Rock Solid Church Sample Data:Enable Stopwatch
            AddBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Stopwatch", "EnableStopwatch", "", "If true, a stopwatch will be used to time each of the major operations.", 3, @"False", "C12E5A2D-1813-4FC6-ACB4-9D5DD17116F3" );

            // Attrib for BlockType: Rock Solid Church Sample Data:XML Document URL
            AddBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "XML Document URL", "XMLDocumentURL", "", "The URL for the input sample data XML document.", 1, @"http://storage.rockrms.com/sampledata/sampledata.xml", "5E26439E-4E98-45B1-B19B-D5B2F3405963" );

            // Attrib for BlockType: Rock Solid Church Sample Data:Fabricate Attendance
            AddBlockTypeAttribute( "A42E0031-B2B9-403A-845B-9C968D7716A6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Fabricate Attendance", "FabricateAttendance", "", "If true, then fake attendance data will be fabricated (if the right parameters are in the xml)", 2, @"True", "D984C884-DA3D-47E9-846C-3AE6285152A3" );

            // Attrib for BlockType: Login:Remote Authorization Types
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "ECA90666-E7A0-4406-8559-0153DCB908FD", "Remote Authorization Types", "RemoteAuthorizationTypes", "", "Which of the active remote authorization types should be displayed as an option for user to use for authentication.", 7, @"", "8A09E6E2-3A9C-4D70-B03D-43D8FCB77D78" );

            // Attrib for BlockType: Defined Type Detail:Defined Type
            AddBlockTypeAttribute( "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "If a Defined Type is set, only details for it will be displayed (regardless of the querystring parameters).", 0, @"", "0305EF98-C791-4626-9996-F189B9BB674C" );

            // Attrib for BlockType: Defined Value List:Defined Type
            AddBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "If a Defined Type is set, only its Defined Values will be displayed (regardless of the querystring parameters).", 0, @"", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637" );

            // Set all existing HtmlEditors to UseCodeEditor=true, since that is the new default value
            Sql( @"
UPDATE [AttributeValue]
SET
	[Value] = 'True'
WHERE
	[AttributeId] IN 
	(
		SELECT [Id] FROM [Attribute] WHERE [Guid] = '0673E015-F8DD-4A52-B380-C758011331B2'
	)
" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.Schedule", new string[] { "Name" } );
            AlterColumn( "dbo.Schedule", "Name", c => c.String( nullable: false, maxLength: 50 ) );
            CreateIndex( "dbo.Schedule", new string[] { "Name" } );

            Sql( "DELETE FROM [Metric]" );
            
            Sql( "DELETE FROM [Schedule] where [CategoryId] in (SELECT [Id] FROM [Category] where [Guid] = '5A794741-5444-43F0-90D7-48E47276D426')" );

            Sql( "DELETE FROM [Category] where [Guid] = '5A794741-5444-43F0-90D7-48E47276D426'" );

            DeleteDefinedType( "D6F323FF-6EF2-4DA7-A82C-61399AC1D798" );

            // Attrib for BlockType: Metric Value List:Detail Page
            DeleteAttribute( "E789EF36-D2B6-417C-91BB-D42BE6645A36" );
            // Attrib for BlockType: Category Tree View:Entity Type Friendly Name
            DeleteAttribute( "07213E2C-C239-47CA-A781-F7A908756DC2" );


            // Remove Block: Metric Value Detail, from Page: Metric Value Detail, Site: Rock RMS
            DeleteBlock( "85CD3B5F-83B7-4786-B6CB-FC5F1887CA1A" );
            // Remove Block: Metric Value List, from Page: Metrics, Site: Rock RMS
            DeleteBlock( "6D58C677-B46A-48E1-9624-AB06A3D5AF1A" );
            // Remove Block: Metric Detail, from Page: Metrics, Site: Rock RMS
            DeleteBlock( "F85FE71D-927D-45AF-B419-02A8909C6E72" );
            // Remove Block: Category Detail, from Page: Metrics, Site: Rock RMS
            DeleteBlock( "8DD6BBCD-518B-4492-9E44-721DEBC70884" );
            // Remove Block: Metrics Tree View, from Page: Metrics, Site: Rock RMS
            DeleteBlock( "EAAB692C-8009-4A0E-AA44-E0DFF827EAAF" );

            DeleteBlockType( "508DA252-F94C-4641-8579-458D8FCE14B2" ); // MetricValue Detail
            DeleteBlockType( "E40A1526-04D0-42A0-B275-D1AE161E2E57" ); // Metric Value List
            DeleteBlockType( "D77341B9-BA38-4693-884E-E5C1D908CEC4" ); // Metric Detail

            DeletePage( "64E16878-D5AE-40A5-94FE-C2E8BE62DF61" ); // Page: Metric Value DetailLayout: Full Width, Site: Rock RMS
            DeletePage( "78D84825-EB1A-43C6-9AD5-5F0F84CC9A53" ); // Page: MetricsLayout: Left Sidebar Panel, Site: Rock RMS


            /* Catch up migrations */
            // Attrib for BlockType: Defined Value List:Defined Type
            DeleteAttribute( "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637" );
            // Attrib for BlockType: Defined Type Detail:Defined Type
            DeleteAttribute( "0305EF98-C791-4626-9996-F189B9BB674C" );
            // Attrib for BlockType: Login:Remote Authorization Types
            DeleteAttribute( "8A09E6E2-3A9C-4D70-B03D-43D8FCB77D78" );
            // Attrib for BlockType: Rock Solid Church Sample Data:Fabricate Attendance
            DeleteAttribute( "D984C884-DA3D-47E9-846C-3AE6285152A3" );
            // Attrib for BlockType: Rock Solid Church Sample Data:XML Document URL
            DeleteAttribute( "5E26439E-4E98-45B1-B19B-D5B2F3405963" );
            // Attrib for BlockType: Rock Solid Church Sample Data:Enable Stopwatch
            DeleteAttribute( "C12E5A2D-1813-4FC6-ACB4-9D5DD17116F3" );
        }
    }
}
