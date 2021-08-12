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
    public partial class AddPersonalLink : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                 "dbo.PersonalLink",
                 c => new
                 {
                     Id = c.Int( nullable: false, identity: true ),
                     PersonAliasId = c.Int(),
                     Name = c.String( nullable: false, maxLength: 100 ),
                     Url = c.String( maxLength: 2048 ),
                     SectionId = c.Int( nullable: false ),
                     Order = c.Int( nullable: false ),
                     CreatedDateTime = c.DateTime(),
                     ModifiedDateTime = c.DateTime(),
                     CreatedByPersonAliasId = c.Int(),
                     ModifiedByPersonAliasId = c.Int(),
                     Guid = c.Guid( nullable: false ),
                     ForeignId = c.Int(),
                     ForeignGuid = c.Guid(),
                     ForeignKey = c.String( maxLength: 100 ),
                 } )
                 .PrimaryKey( t => t.Id )
                 .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                 .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                 .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                 .ForeignKey( "dbo.PersonalLinkSection", t => t.SectionId, cascadeDelete: true )
                 .Index( t => t.PersonAliasId )
                 .Index( t => t.SectionId )
                 .Index( t => t.CreatedByPersonAliasId )
                 .Index( t => t.ModifiedByPersonAliasId )
                 .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.PersonalLinkSection",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonAliasId = c.Int(),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    IsShared = c.Boolean( nullable: false ),
                    IconCssClass = c.String( maxLength: 100 ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.PersonalLinkSectionOrder",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonAliasId = c.Int( nullable: false ),
                    SectionId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .ForeignKey( "dbo.PersonalLinkSection", t => t.SectionId, cascadeDelete: true )
                .Index( t => t.PersonAliasId )
                .Index( t => t.SectionId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Add Page Shared Links to Site:Rock RMS (Under Admin Tools > CMS Configuration)
            RockMigrationHelper.AddPage( true, "B4A24AB7-9369-4055-883F-4F4892C39AE3", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Shared Links", "", "C206A96E-6926-4EB9-A30F-E5FCE559D180", "fa fa-bookmark" );
            // Add Page Section Detail to Site:Rock RMS           
            RockMigrationHelper.AddPage( true, "C206A96E-6926-4EB9-A30F-E5FCE559D180", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Section Detail", "", "776704B9-17F8-467E-AABC-B4E19FF28960", "" );
            // Add Page Personal Links to Site:Rock RMS (Under My Settings)
            RockMigrationHelper.AddPage( true, "CF54E680-2E02-4F16-B54B-A2F2D29CD932", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Personal Links", "", "ED1B85B7-C76A-4624-B644-ABC1CD4BDEAE", "fa fa-bookmark" );
            // Add Page Section Detail to Site:Rock RMS          
            RockMigrationHelper.AddPage( true, "ED1B85B7-C76A-4624-B644-ABC1CD4BDEAE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Section Detail", "", "B0866B52-290B-4623-A123-2AD913BB905C", "" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = 'B0866B52-290B-4623-A123-2AD913BB905C'" );
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '776704B9-17F8-467E-AABC-B4E19FF28960'" );
            // Add/Update BlockType Personal Link List       
            RockMigrationHelper.UpdateBlockType( "Personal Link List", "Lists personal link in the system.", "~/Blocks/Cms/PersonalLinkList.ascx", "CMS", "E7546752-C3DC-4B96-88D9-A431F2D1C989" );
            // Add/Update BlockType Personal Link Section Detail 
            RockMigrationHelper.UpdateBlockType( "Personal Link Section Detail", "Edit details of a Personal Link Section", "~/Blocks/Cms/PersonalLinkSectionDetail.ascx", "CMS", "CFE5C556-9E46-4A51-849D-FF5F4A899930" );
            // Add/Update BlockType Personal Link Section List    
            RockMigrationHelper.UpdateBlockType( "Personal Link Section List", "Lists personal link section in the system.", "~/Blocks/Cms/PersonalLinkSectionList.ascx", "CMS", "0BFD74A8-1888-4407-9102-D3FCEABF3095" );
            // Add Block Personal Link Section List to Page: Shared Links, Site: Rock RMS         
            RockMigrationHelper.AddBlock( true, "C206A96E-6926-4EB9-A30F-E5FCE559D180".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0BFD74A8-1888-4407-9102-D3FCEABF3095".AsGuid(), "Personal Link Section List", "Main", @"", @"", 0, "047459F6-27BE-484C-9192-F711593F2525" );
            // Add Block Personal Link Section Detail to Page: Section Detail, Site: Rock RMS      
            RockMigrationHelper.AddBlock( true, "776704B9-17F8-467E-AABC-B4E19FF28960".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CFE5C556-9E46-4A51-849D-FF5F4A899930".AsGuid(), "Personal Link Section Detail", "Main", @"", @"", 0, "29F0A3E8-1953-443F-98DA-2E38646F15C6" );
            // Add Block Personal Link List to Page: Section Detail, Site: Rock RMS             
            RockMigrationHelper.AddBlock( true, "776704B9-17F8-467E-AABC-B4E19FF28960".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E7546752-C3DC-4B96-88D9-A431F2D1C989".AsGuid(), "Personal Link List", "Main", @"", @"", 1, "08926EE5-F5FA-495A-889F-ADF5340FF96F" );
            // Add Block Personal Link Section List to Page: Personal Links, Site: Rock RMS   
            RockMigrationHelper.AddBlock( true, "ED1B85B7-C76A-4624-B644-ABC1CD4BDEAE".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0BFD74A8-1888-4407-9102-D3FCEABF3095".AsGuid(), "Personal Link Section List", "Main", @"", @"", 0, "E77F9A2B-C714-429A-B7C6-A364FEDC81CA" );
            // Add Block Personal Link Section Detail to Page: Section Detail, Site: Rock RMS   
            RockMigrationHelper.AddBlock( true, "B0866B52-290B-4623-A123-2AD913BB905C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "CFE5C556-9E46-4A51-849D-FF5F4A899930".AsGuid(), "Personal Link Section Detail", "Main", @"", @"", 0, "DAB97C12-D88B-4ACE-AC02-52C99B81C375" );
            // Add Block Personal Link List to Page: Section Detail, Site: Rock RMS    
            RockMigrationHelper.AddBlock( true, "B0866B52-290B-4623-A123-2AD913BB905C".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "E7546752-C3DC-4B96-88D9-A431F2D1C989".AsGuid(), "Personal Link List", "Main", @"", @"", 1, "4DA54C7E-EBEA-426C-90ED-6A4F5913B148" );
            // update block order for pages with new blocks if the page,zone has multiple blocks
            // Update Order for Page: Section Detail,  Zone: Main,  Block: Personal Link List         
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '08926EE5-F5FA-495A-889F-ADF5340FF96F'" );
            // Update Order for Page: Section Detail,  Zone: Main,  Block: Personal Link List         
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '4DA54C7E-EBEA-426C-90ED-6A4F5913B148'" );
            // Update Order for Page: Section Detail,  Zone: Main,  Block: Personal Link Section Detail     
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '29F0A3E8-1953-443F-98DA-2E38646F15C6'" );
            // Update Order for Page: Section Detail,  Zone: Main,  Block: Personal Link Section Detail      
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'DAB97C12-D88B-4ACE-AC02-52C99B81C375'" );
            // Attribute for BlockType: Personal Link Section Detail:Shared Section            
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "CFE5C556-9E46-4A51-849D-FF5F4A899930", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Shared Section", "SharedSection", "Shared Section", @"When enabled, any section created will be shared and non-personal.", 0, @"False", "C153B361-D17D-4C84-B9AD-3E80CBC0D2D5" );
            // Attribute for BlockType: Personal Link Section List:Shared Section       
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0BFD74A8-1888-4407-9102-D3FCEABF3095", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Shared Section", "SharedSection", "Shared Section", @"When enabled, only shared sections will be displayed.", 1, @"False", "CCCAB143-FF4E-4751-9A8B-01BE3E058099" );
            // Attribute for BlockType: Personal Link Section List:Detail Page          
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0BFD74A8-1888-4407-9102-D3FCEABF3095", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "Detail Page", @"", 0, @"", "15F245C3-D64E-420C-84EB-204661F94126" );
            RockMigrationHelper.AddBlockAttributeValue( "047459F6-27BE-484C-9192-F711593F2525", "15F245C3-D64E-420C-84EB-204661F94126", @"776704b9-17f8-467e-aabc-b4e19ff28960" );
            RockMigrationHelper.AddBlockAttributeValue( "047459F6-27BE-484C-9192-F711593F2525", "D836A274-AB49-4A65-8C11-5CE85C0FC6DB", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "047459F6-27BE-484C-9192-F711593F2525", "8CCA3F5B-A5AB-49E7-9A7E-E7B3302F0F93", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "29F0A3E8-1953-443F-98DA-2E38646F15C6", "C153B361-D17D-4C84-B9AD-3E80CBC0D2D5", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "E77F9A2B-C714-429A-B7C6-A364FEDC81CA", "8CCA3F5B-A5AB-49E7-9A7E-E7B3302F0F93", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "E77F9A2B-C714-429A-B7C6-A364FEDC81CA", "15F245C3-D64E-420C-84EB-204661F94126", @"b0866b52-290b-4623-a123-2ad913bb905c" );
            RockMigrationHelper.AddBlockAttributeValue( "E77F9A2B-C714-429A-B7C6-A364FEDC81CA", "D836A274-AB49-4A65-8C11-5CE85C0FC6DB", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "DAB97C12-D88B-4ACE-AC02-52C99B81C375", "C153B361-D17D-4C84-B9AD-3E80CBC0D2D5", @"False" );
            RockMigrationHelper.AddBlockAttributeValue( "047459F6-27BE-484C-9192-F711593F2525", "CCCAB143-FF4E-4751-9A8B-01BE3E058099", @"True" );

            // Add/Update BlockType Personal Links    
            RockMigrationHelper.UpdateBlockType( "Personal Links", "This block is used to show both personal and shared bookmarks as well as 'Quick Return' links.", "~/Blocks/Cms/PersonalLinks.ascx", "CMS", "4D42DF90-97A3-470B-A7D4-A6FD00673761" );
            // Add Block Personal Links to  Site: Rock RMS        
            RockMigrationHelper.AddBlock( true, null, null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "4D42DF90-97A3-470B-A7D4-A6FD00673761".AsGuid(), "Personal Links", "Header", @"", @"", 0, "6AD476B5-77BB-4DAF-B55D-AD2CF599F8B2" );
            // Attribute for BlockType: Personal Links:Manage Link Page   
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D42DF90-97A3-470B-A7D4-A6FD00673761", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage Link Page", "ManageLinksPage", "Manage Link Page", @"Link to the manage link page", 0, @"", "43C43878-DDA8-4517-9630-84F16291E5CE" );
            RockMigrationHelper.AddBlockAttributeValue( "6AD476B5-77BB-4DAF-B55D-AD2CF599F8B2", "43C43878-DDA8-4517-9630-84F16291E5CE", @"ed1b85b7-c76a-4624-b644-abc1cd4bdeae" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Personal Link Section List  
            RockMigrationHelper.DeleteAttribute( "8CCA3F5B-A5AB-49E7-9A7E-E7B3302F0F93" );
            // core.CustomActionsConfigs Attribute for BlockType: Personal Link Section List       
            RockMigrationHelper.DeleteAttribute( "A1848623-A868-4AAD-8D3B-37F1FB88AE67" );
            // Detail Page Attribute for BlockType: Personal Link Section List            
            RockMigrationHelper.DeleteAttribute( "15F245C3-D64E-420C-84EB-204661F94126" );
            // Shared Section Attribute for BlockType: Personal Link Section Detail       
            RockMigrationHelper.DeleteAttribute( "C153B361-D17D-4C84-B9AD-3E80CBC0D2D5" );
            // core.EnableDefaultWorkflowLauncher Attribute for BlockType: Personal Link List 
            RockMigrationHelper.DeleteAttribute( "9AC67074-F35E-4A43-BDDD-8EA1813572E2" );
            // core.CustomActionsConfigs Attribute for BlockType: Personal Link List        
            RockMigrationHelper.DeleteAttribute( "2233A415-008D-4944-B81E-E71B35F57F23" );
            // Remove Block: Personal Link List, from Page: Section Detail, Site: Rock RMS   
            RockMigrationHelper.DeleteBlock( "4DA54C7E-EBEA-426C-90ED-6A4F5913B148" );
            // Remove Block: Personal Link Section Detail, from Page: Section Detail, Site: Rock RMS   
            RockMigrationHelper.DeleteBlock( "DAB97C12-D88B-4ACE-AC02-52C99B81C375" );
            // Remove Block: Personal Link Section List, from Page: Personal Links, Site: Rock RMS     
            RockMigrationHelper.DeleteBlock( "E77F9A2B-C714-429A-B7C6-A364FEDC81CA" );
            // Remove Block: Personal Link List, from Page: Section Detail, Site: Rock RMS     
            RockMigrationHelper.DeleteBlock( "08926EE5-F5FA-495A-889F-ADF5340FF96F" );
            // Remove Block: Personal Link Section Detail, from Page: Section Detail, Site: Rock RMS      
            RockMigrationHelper.DeleteBlock( "29F0A3E8-1953-443F-98DA-2E38646F15C6" );
            // Remove Block: Personal Link Section List, from Page: Personal Links, Site: Rock RMS        
            RockMigrationHelper.DeleteBlock( "047459F6-27BE-484C-9192-F711593F2525" );
            // Delete BlockType Personal Link Section List    
            RockMigrationHelper.DeleteBlockType( "0BFD74A8-1888-4407-9102-D3FCEABF3095" ); // Personal Link Section List  
            // Delete BlockType Personal Link Section Detail   
            RockMigrationHelper.DeleteBlockType( "CFE5C556-9E46-4A51-849D-FF5F4A899930" ); // Personal Link Section Detail  
            // Delete BlockType Personal Link List             
            RockMigrationHelper.DeleteBlockType( "E7546752-C3DC-4B96-88D9-A431F2D1C989" ); // Personal Link List  
            // Delete Page Section Detail from Site:Rock RMS          
            RockMigrationHelper.DeletePage( "B0866B52-290B-4623-A123-2AD913BB905C" ); //  Page: Section Detail, Layout: Full Width, Site: Rock RMS  
            // Delete Page Personal Links from Site:Rock RMS          
            RockMigrationHelper.DeletePage( "ED1B85B7-C76A-4624-B644-ABC1CD4BDEAE" ); //  Page: Personal Links, Layout: Full Width, Site: Rock RMS  
            // Delete Page Section Detail from Site:Rock RMS         
            RockMigrationHelper.DeletePage( "776704B9-17F8-467E-AABC-B4E19FF28960" ); //  Page: Section Detail, Layout: Full Width, Site: Rock RMS  
            // Delete Page Shared Links from Site:Rock RMS            
            RockMigrationHelper.DeletePage( "C206A96E-6926-4EB9-A30F-E5FCE559D180" ); //  Page: Shared Links, Layout: Full Width, Site: Rock RMS  

            DropForeignKey( "dbo.PersonalLink", "SectionId", "dbo.PersonalLinkSection" );
            DropForeignKey( "dbo.PersonalLinkSectionOrder", "SectionId", "dbo.PersonalLinkSection" );
            DropForeignKey( "dbo.PersonalLinkSectionOrder", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLinkSectionOrder", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLinkSectionOrder", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLinkSection", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLinkSection", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLinkSection", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLink", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLink", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalLink", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.PersonalLinkSectionOrder", new[] { "Guid" } );
            DropIndex( "dbo.PersonalLinkSectionOrder", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonalLinkSectionOrder", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonalLinkSectionOrder", new[] { "SectionId" } );
            DropIndex( "dbo.PersonalLinkSectionOrder", new[] { "PersonAliasId" } );
            DropIndex( "dbo.PersonalLinkSection", new[] { "Guid" } );
            DropIndex( "dbo.PersonalLinkSection", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonalLinkSection", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonalLinkSection", new[] { "PersonAliasId" } );
            DropIndex( "dbo.PersonalLink", new[] { "Guid" } );
            DropIndex( "dbo.PersonalLink", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonalLink", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonalLink", new[] { "SectionId" } );
            DropIndex( "dbo.PersonalLink", new[] { "PersonAliasId" } );
            DropTable( "dbo.PersonalLinkSectionOrder" );
            DropTable( "dbo.PersonalLinkSection" );
            DropTable( "dbo.PersonalLink" );
        }
    }
}
