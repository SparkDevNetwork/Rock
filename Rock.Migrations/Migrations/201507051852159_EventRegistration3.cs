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
    public partial class EventRegistration3 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex( "dbo.FinancialTransactionDetail", new string[] { "EntityTypeId", "EntityId" } );

            AddColumn( "dbo.Registration", "DiscountCode", c => c.String( maxLength: 100 ) );
            AddColumn("dbo.Registration", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Registration", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.RegistrationRegistrantFee", "Option", c => c.String());
            AlterColumn("dbo.RegistrationTemplateDiscount", "DiscountPercentage", c => c.Decimal(nullable: false, precision: 18, scale: 2));

            RockMigrationHelper.AddPage( "2E6FED28-683F-4726-8CF1-2822E8E73B03", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Registration", "", "F7CA6E0F-C319-47AB-9A6D-247C5716D846", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "F7CA6E0F-C319-47AB-9A6D-247C5716D846", "Registration/{Slug}" );

            RockMigrationHelper.UpdateBlockType( "Registration Entry", "Block used to register for registration instance.", "~/Blocks/Event/RegistrationEntry.ascx", "Event", "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD" );
            // Add Block to Page: Registration, Site: External Website
            RockMigrationHelper.AddBlock( "F7CA6E0F-C319-47AB-9A6D-247C5716D846", "", "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "Registration Entry", "Main", "", "", 0, "2AD7E973-5F01-4B2F-95AB-E80CA557CE9F" );

            // Attrib for BlockType: Registration Entry:Batch Name Prefix
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Batch Name Prefix", "BatchNamePrefix", "", "The batch prefix name to use when creating a new batch", 3, @"Event Registration", "589260FC-E44F-48AA-B119-C94E1C7A595B" );
            // Attrib for BlockType: Registration Entry:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 0, @"368DD475-242C-49C4-A42C-7278BE690CC2", "A2FF6FA6-3A37-48AA-867E-FFA2AE0DBE45" );
            // Attrib for BlockType: Registration Entry:Record Status
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 1, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "CE8EDF18-7A68-4752-99C1-CEFFBF06632C" );
            // Attrib for BlockType: Registration Entry:Source
            RockMigrationHelper.AddBlockTypeAttribute( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Source", "Source", "", "The Financial Source Type to use when creating transactions", 2, @"7D705CE7-7B11-4342-A58E-53617C5B4E69", "7290B82A-14A8-402D-8A97-1A1F730F7ECE" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Registration Entry:Batch Name Prefix
            RockMigrationHelper.DeleteAttribute( "589260FC-E44F-48AA-B119-C94E1C7A595B" );
            // Attrib for BlockType: Registration Entry:Source
            RockMigrationHelper.DeleteAttribute( "7290B82A-14A8-402D-8A97-1A1F730F7ECE" );
            // Attrib for BlockType: Registration Entry:Record Status
            RockMigrationHelper.DeleteAttribute( "CE8EDF18-7A68-4752-99C1-CEFFBF06632C" );
            // Attrib for BlockType: Registration Entry:Connection Status
            RockMigrationHelper.DeleteAttribute( "A2FF6FA6-3A37-48AA-867E-FFA2AE0DBE45" );

            // Remove Block: Registration Entry, from Page: Registration, Site: External Website
            RockMigrationHelper.DeleteBlock( "2AD7E973-5F01-4B2F-95AB-E80CA557CE9F" );

            RockMigrationHelper.DeleteBlockType( "CABD2BFB-DFFF-42CD-BF1A-14F3BEE583DD" ); // Registration Entry
            RockMigrationHelper.DeletePage( "F7CA6E0F-C319-47AB-9A6D-247C5716D846" ); //  Page: Registration, Layout: FullWidth, Site: External Website
            
            AlterColumn("dbo.RegistrationTemplateDiscount", "DiscountPercentage", c => c.Double(nullable: false));
            DropColumn("dbo.RegistrationRegistrantFee", "Option");
            DropColumn("dbo.Registration", "DiscountAmount");
            DropColumn("dbo.Registration", "DiscountPercentage");
            DropColumn("dbo.Registration", "DiscountCode");

            DropIndex( "dbo.FinancialTransactionDetail", new string[] { "EntityTypeId", "EntityId" } );


        }
    }
}
