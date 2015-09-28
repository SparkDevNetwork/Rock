using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 5, "1.3.4" )]
    public class LifeGroupAddHasChildren : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            var typeId = new Rock.Model.DefinedTypeService( new Rock.Data.RockContext() ).GetByGuid( new Guid( "512F355E-9441-4C47-BE47-7FFE19209496" ) ).Id.ToString();
            Sql( String.Format( @"
UPDATE [AttributeQualifier]
SET Value = '{0}'
WHERE Guid = '9AF45EBE-3154-402D-B5C5-03BA338170AC'
", typeId ) );

            var blockTypeId = new Rock.Model.AttributeService( new Rock.Data.RockContext() ).Get( new Guid( "D9D915AB-D013-4285-9727-645A054C6832" ) ).EntityTypeQualifierValue;
            if ( blockTypeId == null )
            {
                RockMigrationHelper.UpdateBlockType( "Group Registration", "Allows a person to register for a group.", "~/Blocks/Groups/GroupRegistration.ascx", "Groups", "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7" );

                // Attrib for BlockType: Group Registration:Enable Debug
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Shows the fields available to merge in lava.", 5, @"False", "FBF13ACE-F9DC-4A28-87B3-2FA3D36FF55A" );

                // Attrib for BlockType: Group Registration:Workflow
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 4, @"", "C4287E3F-D2D8-413E-A3AE-F9A3EE7A5021" );

                // Attrib for BlockType: Group Registration:Lava Template
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The lava template to use to format the group details.", 6, @"
", "8E37CB4A-AF69-4671-9EC6-2ED72380B749" );

                // Attrib for BlockType: Group Registration:Result Page
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Result Page", "ResultPage", "", "An optional page to redirect user to after they have been registered for the group.", 7, @"", "BAD40ACB-CC0B-4CE4-A3B8-E7C5134AE0E2" );

                // Attrib for BlockType: Group Registration:Result Lava Template
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Result Lava Template", "ResultLavaTemplate", "", "The lava template to use to format result message after user has been registered. Will only display if user is not redirected to a Result Page ( previous setting ).", 8, @"
", "C58B22F0-1CAC-436D-AFFE-5FC616F36DB1" );

                // Attrib for BlockType: Group Registration:Mode
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Mode", "Mode", "", "The mode to use when displaying registration details.", 0, @"Simple", "EDEC82F3-C9E1-4B26-862D-E896F6C26376" );

                // Attrib for BlockType: Group Registration:Group Member Status
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "", "The group member status to use when adding person to group (default: 'Pending'.)", 1, @"2", "B61174DE-6E7D-4171-9067-6A7981F888E8" );

                // Attrib for BlockType: Group Registration:Connection Status
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 2, @"368DD475-242C-49C4-A42C-7278BE690CC2", "E3544F7A-0E7A-421B-9142-AE858E9CCFBB" );

                // Attrib for BlockType: Group Registration:Record Status
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 3, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "BCDB7126-5F5E-486D-84CB-C6D7E8F265FC" );


                RockMigrationHelper.DeleteAttribute( "19D5EB3B-9921-440E-9075-6617203FEE39" );
                RockMigrationHelper.DeleteAttribute( "D9D915AB-D013-4285-9727-645A054C6832" );

                // Attrib for BlockType: Group Registration:Register Button Alt Text
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Register Button Alt Text", "RegisterButtonAltText", "", "Alternate text to use for the Register button (default is 'Register').", 10, @"", "19D5EB3B-9921-440E-9075-6617203FEE39" );

                // Attrib for BlockType: Group Registration:Auto Fill Form
                RockMigrationHelper.AddBlockTypeAttribute( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Auto Fill Form", "AutoFillForm", "", "If set to FALSE then the form will not load the context of the logged in user (default: 'True'.)", 9, @"true", "D9D915AB-D013-4285-9727-645A054C6832" );

            }

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
