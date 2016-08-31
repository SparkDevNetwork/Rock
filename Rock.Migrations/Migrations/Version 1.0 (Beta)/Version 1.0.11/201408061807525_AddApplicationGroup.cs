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
    public partial class AddApplicationGroup : Rock.Migrations.RockMigration
    {
        /// Add the new Application Group grouptype and PhotoRequest group, and corresponding pages.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateGroupType( "Application Group", "A generic group type used by specific features in Rock. Groups of this type are not show in nav but do show up in lists and have a single 'Member' role.  For example, the PhotoRequest group is of this type. ", "group", "member", null, false, true, false, "fa fa-gears", 0, null, 0, null, Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP );
            RockMigrationHelper.UpdateGroupTypeRole( Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP, "Member", "Member of a group", 0, null, null, "09B14358-FA17-4D65-A8E9-03FA7312CD62", isDefaultGroupTypeRole: true );
            RockMigrationHelper.UpdateGroup( null, Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP, "Photo Request", "People who have given their photo via the Photo Request system (pending or active) and/or people who have 'opted out' (members who are inactive). Pending members are people whose photo has not yet been approved.", null, 0, "2108EF9C-10DC-4466-973D-D25AAB7818BE" );

            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Application Groups", "Special application type groups.", "BA078BB8-7205-46F4-9530-B2FB9EAD3E57", "fa fa-gears" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "BA078BB8-7205-46F4-9530-B2FB9EAD3E57", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Application Group Detail", "", "E9737442-E6A9-47D5-A842-11C1AE1CF43F", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "E9737442-E6A9-47D5-A842-11C1AE1CF43F", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Detail", "", "C920AA8F-A8CA-4984-95EC-58B7309E670E", "fa fa-user" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Photo Requests", "", "325B50D6-545D-461A-9CB7-72B001E82F21", "fa fa-camera" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "325B50D6-545D-461A-9CB7-72B001E82F21", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Send Requests", "", "B64D0429-488C-430E-8C32-5C7F32589F73", "fa fa-send" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "325B50D6-545D-461A-9CB7-72B001E82F21", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Verify Photos", "View submitted photos for verification.", "07E4BA19-614A-42D0-9D75-DFB31374844D", "fa fa-inbox" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "325B50D6-545D-461A-9CB7-72B001E82F21", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "View Request Details", "", "2B941997-0C2C-4153-8E1E-ACC874DCC7DB", "fa fa-list-alt" ); // Site:Rock RMS

            // Add Block to Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlock( "BA078BB8-7205-46F4-9530-B2FB9EAD3E57", "", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Application Groups", "Main", "", "", 0, "69A9B634-7A59-4F89-AF07-0C628E05BDC4" );

            // Add Block to Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "E9737442-E6A9-47D5-A842-11C1AE1CF43F", "", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Application Group Detail", "Main", "", "", 0, "64EEA884-8D04-44A3-9C75-5523A9EB9175" );

            // Add Block to Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "E9737442-E6A9-47D5-A842-11C1AE1CF43F", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "Main", "", "", 1, "AB47ABE2-B9BB-4C89-B35A-ABCECA2098C6" );

            // Add Block to Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C920AA8F-A8CA-4984-95EC-58B7309E670E", "", "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "Group Member Detail", "Main", "", "", 0, "DE7DC339-C919-4C9D-9BCA-B5257A4CF799" );

            // Add Block to Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlock( "325B50D6-545D-461A-9CB7-72B001E82F21", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "F382361C-BEE3-45FC-9A24-66E5338EAB66" );

            // Add Block to Page: View Request Details, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2B941997-0C2C-4153-8E1E-ACC874DCC7DB", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Member List", "Main", "", "", 0, "DEC91774-50DD-46F4-9441-2A35B6FAC843" );

            // Add Block to Page: Verify Photos, Site: Rock RMS
            RockMigrationHelper.AddBlock( "07E4BA19-614A-42D0-9D75-DFB31374844D", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "Main", "", "", 0, "995E30ED-CB90-4A3F-B626-2C565BDB3FDE" );

            // Attrib Value for Block:Application Groups, Attribute:Display System Column Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "766A4BFA-D2D1-4744-B30D-637A7E3B9D8F", @"True" );

            // Attrib Value for Block:Application Groups, Attribute:Display Active Status Column Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "FCB5F8B3-9C0E-46A8-974A-15353447FCD7", @"False" );

            // Attrib Value for Block:Application Groups, Attribute:Entity Type Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "A32C7EFE-2E5F-4E99-9867-DD562407B72E", @"" );

            // Attrib Value for Block:Application Groups, Attribute:Display Filter Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "7E0EDF09-9374-4AC4-8591-30C08D7F1E1F", @"False" );

            // Attrib Value for Block:Application Groups, Attribute:Include Group Types Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "5164FF88-A53B-4982-BE50-D56F1FE13FC6", @"3981cf6d-7d15-4b57-aace-c0e25d28bd49" );

            // Attrib Value for Block:Application Groups, Attribute:Exclude Group Types Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "0901CBFE-1980-4A1C-8AF0-4A8BD0FC46E9", @"" );

            // Attrib Value for Block:Application Groups, Attribute:Display Group Type Column Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "951D268A-B2A8-42A2-B1C1-3B854070DDF9", @"True" );

            // Attrib Value for Block:Application Groups, Attribute:Display Description Column Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "A0E1B2A4-9D86-4F57-B608-FC7CC498EAC3", @"True" );

            // Attrib Value for Block:Application Groups, Attribute:Limit to Security Role Groups Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "1DAD66E3-8859-487E-8200-483C98DE2E07", @"False" );

            // Attrib Value for Block:Application Groups, Attribute:Detail Page Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69A9B634-7A59-4F89-AF07-0C628E05BDC4", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", @"e9737442-e6a9-47d5-a842-11c1ae1cf43f" );

            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AB47ABE2-B9BB-4C89-B35A-ABCECA2098C6", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"c920aa8f-a8ca-4984-95ec-58b7309e670e" );

            // Attrib Value for Block:Group Member List, Attribute:Group Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "AB47ABE2-B9BB-4C89-B35A-ABCECA2098C6", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A", @"0" );

            // Attrib Value for Block:Page Menu, Attribute:CSS File Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current Parameters Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Number of Levels Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" );

            // Attrib Value for Block:Page Menu, Attribute:Include Current QueryString Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Template Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsBlocks' %}" );

            // Attrib Value for Block:Page Menu, Attribute:Root Page Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"" );

            // Attrib Value for Block:Page Menu, Attribute:Is Secondary Block Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Page Menu, Attribute:Enable Debug Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F382361C-BEE3-45FC-9A24-66E5338EAB66", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Group Member List, Attribute:Group Page: Verify Photos, Site: Rock RMS
            AddBlockAttributeValueFromGroupGuid( "995E30ED-CB90-4A3F-B626-2C565BDB3FDE", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A", "2108EF9C-10DC-4466-973D-D25AAB7818BE" );

            // Add 'Transaction Matching Page' LinkedPage attribute to Batch Detail block
            RockMigrationHelper.AddBlockTypeAttribute( "CE34CE43-2CCF-4568-9AEB-3BE203DB3470", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Transaction Matching Page", "TransactionMatchingPage", "", "Page used to match transactions for a batch.", 0, @"", "A628A153-1318-4686-83AD-13ED975D2F23" );
            RockMigrationHelper.AddBlockAttributeValue( "E7C8C398-0E1D-4BCE-BC54-A02957228514", "A628A153-1318-4686-83AD-13ED975D2F23", @"cd18fe52-8d6a-49c9-81bf-df97c5ba0302" ); // Transaction Matching Page

            // Add 'Scheduled Transaction Detail Page' LinkedPage attribute to the Transaction Detail block
            RockMigrationHelper.AddBlockTypeAttribute( "1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Scheduled Transaction Detail Page", "ScheduledTransactionDetailPage", "", "Page used to view scheduled transaction detail", 0, @"", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9" );
            RockMigrationHelper.AddBlockAttributeValue( "F125E7EB-DA78-4840-9D00-4C8DD0DD4A27", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9", @"f1c3bbd3-ee91-4ddd-8880-1542ebcd8041" ); // Scheduled Transaction Detail Page
            RockMigrationHelper.AddBlockAttributeValue( "25F645D5-50B9-4DCC-951F-C780C49CD913", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9", @"f1c3bbd3-ee91-4ddd-8880-1542ebcd8041" ); // Scheduled Transaction Detail Page

            // Add 'Batch Detail Page' LinkedPage attribute to the scheduled transaction download block
            RockMigrationHelper.AddBlockTypeAttribute( "71FF09C3-3E50-4E97-9329-3CD57AACCA53", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Batch Detail Page", "BatchDetailPage", "", "The page used to display details of a batch.", 1, @"", "5A8FD662-BF0B-44BD-BAB3-A40A77AF3903" );
            RockMigrationHelper.AddBlockAttributeValue( "A55A9614-9D89-4D56-A022-D15BD6472C62", "5A8FD662-BF0B-44BD-BAB3-A40A77AF3903", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" ); // Batch Detail Page

            // Update the icon on the transaction detail page to be the credit card instead of dollar sign
            Sql( "UPDATE [Page] SET [IconCssClass] = 'fa fa-credit-card' WHERE [Guid] = '97716641-D003-4663-9EA2-D9BB94E7955B'" );
     
        }

        /// <summary>
        /// Delete the Application Group grouptype and PhotoRequest group and related pages.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "5A8FD662-BF0B-44BD-BAB3-A40A77AF390" );
            RockMigrationHelper.DeleteAttribute( "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9" );
            RockMigrationHelper.DeleteAttribute( "A628A153-1318-4686-83AD-13ED975D2F23" );

            // Remove Block: Group Member List, from Page: Verify Photos, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "995E30ED-CB90-4A3F-B626-2C565BDB3FDE" );
            // Remove Block: Member List, from Page: View Request Details, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DEC91774-50DD-46F4-9441-2A35B6FAC843" );
            // Remove Block: Page Menu, from Page: Photo Requests, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "F382361C-BEE3-45FC-9A24-66E5338EAB66" );
            // Remove Block: Group Member Detail, from Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DE7DC339-C919-4C9D-9BCA-B5257A4CF799" );
            // Remove Block: Group Member List, from Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "AB47ABE2-B9BB-4C89-B35A-ABCECA2098C6" );
            // Remove Block: Application Group Detail, from Page: Application Group Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "64EEA884-8D04-44A3-9C75-5523A9EB9175" );
            // Remove Block: Application Groups, from Page: Application Groups, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "69A9B634-7A59-4F89-AF07-0C628E05BDC4" );

            RockMigrationHelper.DeletePage( "2B941997-0C2C-4153-8E1E-ACC874DCC7DB" ); //  Page: View Request Details, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "07E4BA19-614A-42D0-9D75-DFB31374844D" ); //  Page: Verify Photos, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "B64D0429-488C-430E-8C32-5C7F32589F73" ); //  Page: Send Requests, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "325B50D6-545D-461A-9CB7-72B001E82F21" ); //  Page: Photo Requests, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "C920AA8F-A8CA-4984-95EC-58B7309E670E" ); //  Page: Group Member Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "E9737442-E6A9-47D5-A842-11C1AE1CF43F" ); //  Page: Application Group Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "BA078BB8-7205-46F4-9530-B2FB9EAD3E57" ); //  Page: Application Groups, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeleteGroup( "2108EF9C-10DC-4466-973D-D25AAB7818BE" );
            RockMigrationHelper.DeleteGroupTypeRole( "09B14358-FA17-4D65-A8E9-03FA7312CD62" );
            RockMigrationHelper.DeleteGroupType( Rock.SystemGuid.GroupType.GROUPTYPE_APPLICATION_GROUP );
        }

        /// <summary>
        /// Adds a new block attribute value (a group id) for the given block guid and attribute guid
        /// using the given group guid.  It also delets any previously existing attribute value first.
        /// </summary>
        /// <param name="blockGuid">The block GUID.</param>
        /// <param name="attributeGuid">The attribute GUID.</param>
        /// <param name="groupGuid">The guid of the group</param>
        public void AddBlockAttributeValueFromGroupGuid( string blockGuid, string attributeGuid, string groupGuid )
        {
            Sql( string.Format( @"
                
                DECLARE @BlockId int = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')
                DECLARE @GroupId int = (SELECT [Id] FROM [Group] WHERE [Guid] = '{2}')
                
                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @BlockId

                INSERT INTO [AttributeValue] (
                    [IsSystem]
                    ,[AttributeId]
                    ,[EntityId]
                    ,[Order]
                    ,[Value]
                    ,[Guid])
                VALUES(
                    1
                    ,@AttributeId
                    ,@BlockId
                    ,0
                    ,CAST(@GroupId AS VARCHAR)
                    ,NEWID())
",
                    blockGuid,
                    attributeGuid,
                    groupGuid
                )
            );
        }
    }
}
