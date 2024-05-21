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
    public partial class Rollup_20220809 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CSVImportPageUp();
            PersonalizationSegmentResultsUp();
            FixRecordStatusBadge();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CSVImportPageDown();
            PersonalizationSegmentResultsDown();
        }

        /// <summary>
        /// PA: Migration to add new CSV Import page/block
        /// </summary>
        private void CSVImportPageUp()
        {
            // Add Page 
            //  Internal Name: CSV Import
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "7F1F4130-CB98-473B-9DE1-7A886D2283ED", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "CSV Import", "", "4033514D-23D6-493A-8558-591F6EB98D56", "fa fa-file-csv" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add Page Route
            //   Page:CSV Import
            //   Route:admin/power-tools/csv-import
            RockMigrationHelper.AddPageRoute( "4033514D-23D6-493A-8558-591F6EB98D56", "admin/power-tools/csv-import", "36C8FD66-00CF-4388-966A-47A52C6CD69B" );
#pragma warning restore CS0618 // Type or member is obsolete

            // Add/Update BlockType 
            //   Name: CSV Import
            //   Category: CSV Import
            //   Path: ~/Blocks/BulkImport/CsvImport.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "CSV Import", "Block to import data into Rock using the CSV files.", "~/Blocks/BulkImport/CsvImport.ascx", "CSV Import", "362C679C-9A7F-4A2B-9BB0-8683824BE892" );

            // Add Block 
            //  Block Name: CSV Import
            //  Page Name: CSV Import
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4033514D-23D6-493A-8558-591F6EB98D56".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "362C679C-9A7F-4A2B-9BB0-8683824BE892".AsGuid(), "CSV Import", "Main", @"", @"", 0, "12C294CA-E043-41EA-9641-605B9D051FF7" );
        }

        /// <summary>
        /// PA: Migration to add new CSV Import page/block
        /// </summary>
        private void CSVImportPageDown()
        {
            // Remove Block
            //  Name: CSV Import, from Page: CSV Import, Site: Rock RMS
            //  from Page: CSV Import, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "12C294CA-E043-41EA-9641-605B9D051FF7" );

            // Delete BlockType 
            //   Name: CSV Import
            //   Category: CSV Import
            //   Path: ~/Blocks/BulkImport/CsvImport.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "362C679C-9A7F-4A2B-9BB0-8683824BE892" );

            // Delete Page 
            //  Internal Name: CSV Import
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "4033514D-23D6-493A-8558-591F6EB98D56" );
        }
    
        /// <summary>
        /// SK: Add Personalization Segment Results block
        /// </summary>
        private void PersonalizationSegmentResultsUp()
        {
            RockMigrationHelper.AddPage( true, "905F6132-AE1C-4C85-9752-18D22E604C3A","D65F783D-87A9-4CC9-8110-E83466A0EADB","Personalization Segment Results","","08C61FD2-B495-4EC2-8B35-C0D9427E4F46","");
#pragma warning disable CS0618 // Type or member is obsolete
            RockMigrationHelper.AddPageRoute("08C61FD2-B495-4EC2-8B35-C0D9427E4F46","admin/cms/personalization-segments/{PersonalizationSegmentGuid}","5C1BE6E2-4255-4F7C-A015-2D418EDE8606");
#pragma warning restore CS0618 // Type or member is obsolete
            RockMigrationHelper.UpdateBlockType("Personalization Segment Results","Block that lists existing Personalization Segments result.","~/Blocks/Cms/PersonalizationSegmentResults.ascx","Cms","438432E3-22A8-43D9-9F06-179C3B65D298");
            RockMigrationHelper.AddBlock( true, "08C61FD2-B495-4EC2-8B35-C0D9427E4F46".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"438432E3-22A8-43D9-9F06-179C3B65D298".AsGuid(), "Personalization Segment Results","Main",@"",@"",0,"550F8870-9A8E-4072-BF07-36B0621F037C"); 
        }

        /// <summary>
        /// SK: Add Personalization Segment Results block
        /// </summary>
        private void PersonalizationSegmentResultsDown()
        {
            RockMigrationHelper.DeleteBlock( "550F8870-9A8E-4072-BF07-36B0621F037C" );
            RockMigrationHelper.DeleteBlockType( "438432E3-22A8-43D9-9F06-179C3B65D298" );
            RockMigrationHelper.DeletePage( "08C61FD2-B495-4EC2-8B35-C0D9427E4F46" );
        }

        /// <summary>
        /// GJ: Fix Record Status Badge
        /// </summary>
        private void FixRecordStatusBadge()
        {
            Sql( @"
                UPDATE [AttributeValue]
                SET [Value] = N'{% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Inactive"" -%}
    <div class=""rockbadge rockbadge-label"">
        <span class=""label label-danger"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
    </div>
{% elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Pending"" -%}
    <div class=""rockbadge rockbadge-label"">
        <span class=""label label-warning"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
    </div>
{% endif -%}'
                WHERE ([Guid]='B434C492-D5AA-4F81-BEBA-C50F4B82263A')" );
        }
    }
}
