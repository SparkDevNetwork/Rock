using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    public class LifeGroupFinderPages : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Page: Small Groups
            // Delete current Group Finder
            RockMigrationHelper.DeleteAttribute( "4D326BF5-EF92-455E-9B15-C4D5094D76FB" );
            RockMigrationHelper.DeleteAttribute( "7437E1B6-CCF4-4A00-8004-B6074F79C107" );
            RockMigrationHelper.DeleteAttribute( "9EFF53E3-256D-4251-8A7A-629AAAEB856D" );
            RockMigrationHelper.DeleteAttribute( "6A113765-5FEA-4714-93A1-6675AD5DF8FE" );
            RockMigrationHelper.DeleteAttribute( "71140493-AB2A-4040-8724-EA69F95FE264" );
            RockMigrationHelper.DeleteAttribute( "AFB2F4FD-772C-4238-9805-13897A600DCC" );
            RockMigrationHelper.DeleteAttribute( "1846B075-E3B2-4220-A458-8F74B67272B7" );
            RockMigrationHelper.DeleteAttribute( "9DA54BB6-986C-4723-8FE1-E3EF53119C6A" );
            RockMigrationHelper.DeleteAttribute( "16CC2B4B-3BA5-4B76-8601-8D5B91637B06" );
            RockMigrationHelper.DeleteAttribute( "2F71D27F-8EC1-4A13-A349-59229173F88B" );
            RockMigrationHelper.DeleteAttribute( "3CE83B17-1E68-4FC2-BDC3-33920C0BC99C" );
            RockMigrationHelper.DeleteAttribute( "FA863C93-132D-4888-BB51-E46603585199" );
            RockMigrationHelper.DeleteAttribute( "CCDE5B4C-D195-4E9C-9212-36068E8406C8" );
            RockMigrationHelper.DeleteAttribute( "6748E6B3-45B0-4FA1-8405-3FBEDF514BB1" );
            RockMigrationHelper.DeleteAttribute( "B6F965A8-0EF8-48DC-B599-D9131EC640CA" );
            RockMigrationHelper.DeleteAttribute( "1097A3DD-5A58-414B-A17C-DF679FBF12D6" );
            RockMigrationHelper.DeleteAttribute( "DACB3F11-AA38-4470-9D81-4BB0F7D43AA8" );
            RockMigrationHelper.DeleteAttribute( "ED4809CD-1F8A-491D-91F3-ED4C04E36F96" );
            RockMigrationHelper.DeleteAttribute( "CCF6E7CA-F3EA-490F-B910-DD1049B75B5A" );
            RockMigrationHelper.DeleteAttribute( "380D0978-AF4F-4EEC-B872-56F0FB9F91E4" );
            RockMigrationHelper.DeleteBlock( "7332DD26-D5F5-4736-BFCF-FC4AD97DD571" );

            //Add custom Group Finder
            RockMigrationHelper.UpdateBlockType( "Life Group Search", "Central custom group search block.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupSearch.ascx", "Groups", "205531A1-C1BC-494C-911E-EE88D29969FB" );
            RockMigrationHelper.AddBlock( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "", "205531A1-C1BC-494C-911E-EE88D29969FB", "Life Group Finder", "Main", "", "", 1, "6E830C1F-E5BB-4BCB-AA59-807204134E3E" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Information Security Page", "InformationSecurityPage", "", "The page to navigate to for group details.", 0, @"", "97717892-A27D-4709-81FF-DF834DD3B730" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Life Group List Page", "LifeGroupListPage", "", "The page to navigate to for group details.", 0, @"", "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B" );
            RockMigrationHelper.AddBlockAttributeValue( "6E830C1F-E5BB-4BCB-AA59-807204134E3E", "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B", @"218263dc-0877-4956-9610-25e3b70a10f0" ); // Life Group List Page
            
            // Page: Group Search List
            RockMigrationHelper.AddPage( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Group Search List", "", "218263DC-0877-4956-9610-25E3B70A10F0", "" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Life Group List", "Lists all groups for the configured group types.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupList.ascx", "com_centralaz > Groups", "57D90EE5-8425-448A-82F6-292D35CEAEAE" );
            RockMigrationHelper.AddBlock( "218263DC-0877-4956-9610-25E3B70A10F0", "", "57D90EE5-8425-448A-82F6-292D35CEAEAE", "Life Group List", "Main", "", "", 0, "48DBE5C1-14FF-4E69-AEC5-247EE43E6068" );

            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "C54408D6-AF4E-4E01-A23C-D6E307C23206" );

            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Limit to Active Status", "LimittoActiveStatus", "", "Select which groups to show, based on active status. Select [All] to let the user filter by active status.", 10, @"all", "18146156-9AAE-4255-92B1-2CFE183CAF79" );

            // Page: Life Group Detail
            RockMigrationHelper.DeleteAttribute( "BCDB7126-5F5E-486D-84CB-C6D7E8F265FC" );
            RockMigrationHelper.DeleteAttribute( "E3544F7A-0E7A-421B-9142-AE858E9CCFBB" );
            RockMigrationHelper.DeleteAttribute( "B61174DE-6E7D-4171-9067-6A7981F888E8" );
            RockMigrationHelper.DeleteAttribute( "EDEC82F3-C9E1-4B26-862D-E896F6C26376" );
            RockMigrationHelper.DeleteAttribute( "C58B22F0-1CAC-436D-AFFE-5FC616F36DB1" );
            RockMigrationHelper.DeleteAttribute( "BAD40ACB-CC0B-4CE4-A3B8-E7C5134AE0E2" );
            RockMigrationHelper.DeleteAttribute( "8E37CB4A-AF69-4671-9EC6-2ED72380B749" );
            RockMigrationHelper.DeleteAttribute( "C4287E3F-D2D8-413E-A3AE-F9A3EE7A5021" );
            RockMigrationHelper.DeleteAttribute( "FBF13ACE-F9DC-4A28-87B3-2FA3D36FF55A" );
            RockMigrationHelper.DeleteBlock( "91782D7C-9DCE-49F8-99AB-DEC58BF9ACA1" );
            RockMigrationHelper.DeleteBlockType( "9D0EF3AC-D0F7-4FA7-9C64-E7B0855648C7" );
            RockMigrationHelper.DeletePage( "7D24FE9A-710C-4B25-B1C7-76161ED78DB8" ); //  Page: Group Registration

            RockMigrationHelper.AddPage( "EA515FD1-7D71-4E24-A09D-EA9EC34BEC71", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Life Group Detail", "", "803A7786-6A83-43DF-A247-BB4DFF50AAE8", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "803A7786-6A83-43DF-A247-BB4DFF50AAE8", "LifeGroup/{groupId}" );
            RockMigrationHelper.UpdateBlockType( "Life Group Detail", "Allows a person to register for a group.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupDetail.ascx", "com_centralaz > Groups", "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2" );
            RockMigrationHelper.AddBlock( "803A7786-6A83-43DF-A247-BB4DFF50AAE8", "", "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "Life Group Detail", "Main", "", "", 0, "18DC3025-3791-43C8-9805-113AF17D5942" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Group Member Status", "GroupMemberStatus", "", "The group member status to use when adding person to group (default: 'Pending'.)", 1, @"2", "A6C8D708-AB6F-4CED-AA0A-1923DDB5CF87" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Connection Status", "ConnectionStatus", "", "The connection status to use for new individuals (default: 'Web Prospect'.)", 2, @"368DD475-242C-49C4-A42C-7278BE690CC2", "0E9F8D12-C389-43F2-90C6-AC449996F925" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Record Status", "RecordStatus", "", "The record status to use for new individuals (default: 'Pending'.)", 3, @"283999EC-7346-42E3-B807-BCE9B2BABB49", "0E40AB39-FF5C-41FC-BE93-95594FF9AFB2" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow", "Workflow", "", "An optional workflow to start when registration is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 4, @"", "42D95717-2494-4E76-B837-C78AC6E8B139" );
            
            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Email Workflow", "EmailWorkflow", "", "An optional workflow to start when an email request is created. The GroupMember will set as the workflow 'Entity' when processing is started.", 4, @"", "C3537C7E-7105-4E3C-8FF2-6FD956D5EC40" );

            RockMigrationHelper.AddBlockTypeAttribute( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Result Page", "ResultPage", "", "An optional page to redirect user to after they have been registered for the group.", 7, @"", "00C4D211-78F0-424E-BD3F-4F73998C8CD4" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "C3537C7E-7105-4E3C-8FF2-6FD956D5EC40" );
            RockMigrationHelper.DeleteAttribute( "A6C8D708-AB6F-4CED-AA0A-1923DDB5CF87" );
            RockMigrationHelper.DeleteAttribute( "42D95717-2494-4E76-B837-C78AC6E8B139" );
            RockMigrationHelper.DeleteAttribute( "00C4D211-78F0-424E-BD3F-4F73998C8CD4" );
            RockMigrationHelper.DeleteAttribute( "0E9F8D12-C389-43F2-90C6-AC449996F925" );
            RockMigrationHelper.DeleteAttribute( "0E40AB39-FF5C-41FC-BE93-95594FF9AFB2" );
            RockMigrationHelper.DeleteBlock( "18DC3025-3791-43C8-9805-113AF17D5942" );
            RockMigrationHelper.DeleteBlockType( "DAE64D60-18EE-4DC8-A0A0-3280E4201AF2" );
            RockMigrationHelper.DeletePage( "803A7786-6A83-43DF-A247-BB4DFF50AAE8" ); //  Page: Life Group Detail

            RockMigrationHelper.DeleteAttribute( "18146156-9AAE-4255-92B1-2CFE183CAF79" );
            RockMigrationHelper.DeleteAttribute( "C54408D6-AF4E-4E01-A23C-D6E307C23206" );
            RockMigrationHelper.DeleteAttribute( "39E42561-BEDB-450E-A026-98E617B6E70D" );
            RockMigrationHelper.DeleteBlock( "48DBE5C1-14FF-4E69-AEC5-247EE43E6068" );
            RockMigrationHelper.DeleteBlockType( "57D90EE5-8425-448A-82F6-292D35CEAEAE" );
            RockMigrationHelper.DeletePage( "218263DC-0877-4956-9610-25E3B70A10F0" ); //  Page: Group Search List

            RockMigrationHelper.DeleteAttribute( "B4463438-776A-4F4B-9AC5-6D993151004D" );
            RockMigrationHelper.DeleteAttribute( "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B" );
            RockMigrationHelper.DeleteAttribute( "97717892-A27D-4709-81FF-DF834DD3B730" );
            RockMigrationHelper.DeleteBlock( "6E830C1F-E5BB-4BCB-AA59-807204134E3E" );
            RockMigrationHelper.DeleteBlockType( "205531A1-C1BC-494C-911E-EE88D29969FB" ); //Small groups
        }
    }
}
