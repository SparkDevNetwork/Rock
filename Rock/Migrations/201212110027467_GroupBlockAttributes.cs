//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupBlockAttributes : RockMigration_2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // fix up Blocks, BlockTypes and Pages that should have had IsSystem = 1
            Sql( @"
Update [Block] set [IsSystem] = 1 where [Guid] = '6E189D68-C4EC-443F-B409-1EEC0F12D427'
Update [Block] set [IsSystem] = 1 where [Guid] = '71477D00-6A2B-4736-9F7D-2001B76DA638'
Update [Block] set [IsSystem] = 1 where [Guid] = '803CE253-3ADA-4C2A-B62F-EC4D5B7B7257'
Update [Block] set [IsSystem] = 1 where [Guid] = '60DED2D5-0675-452A-B82B-781B044BB856'
Update [Block] set [IsSystem] = 1 where [Guid] = '845100E9-30D6-45A8-9B80-052395735982'
Update [Block] set [IsSystem] = 1 where [Guid] = 'D5D2048C-52C6-4827-A817-9B84E0525510'
Update [Block] set [IsSystem] = 1 where [Guid] = 'BDE5E15D-0CC7-4164-837F-91ADA64A15D9'
Update [Block] set [IsSystem] = 1 where [Guid] = '091BDFEC-F76B-416B-B8F3-0A9DB93AF606'
Update [Block] set [IsSystem] = 1 where [Guid] = '8AFEAEDE-187A-4EE6-BB0E-702C582E8E02'
Update [Block] set [IsSystem] = 1 where [Guid] = 'BEDFF750-3EB8-4EE7-A8B4-23863FB0315D'
Update [Block] set [IsSystem] = 1 where [Guid] = '261EA4CF-F7CD-47DC-BC69-3B2D6EB87DD5'
Update [Block] set [IsSystem] = 1 where [Guid] = '718C516F-0A1D-4DBC-A939-1D9777208FEC'
Update [Block] set [IsSystem] = 1 where [Guid] = 'B8224C72-4168-40F0-96BE-38F2AFD525F5'
Update [Block] set [IsSystem] = 1 where [Guid] = '6C805618-75E7-470F-AC3E-390529ED94F1'
Update [Block] set [IsSystem] = 1 where [Guid] = 'B447AB11-3A19-4527-921A-2266A6B4E181'
Update [Block] set [IsSystem] = 1 where [Guid] = 'B8D83A2C-31F2-48C6-BEBC-753BCDC2A30C'
Update [Block] set [IsSystem] = 1 where [Guid] = '9126CFA2-9B26-4FBB-BB87-F76514221DBE'

Update [BlockType] set [IsSystem] = 1 where [Guid] in ('8C46B8AC-7962-410E-AC37-C1602E9675DD','90533F7D-6841-45E4-9141-1504C49A028D','69B695A4-AD43-4A73-83C0-4C13BE142ECA')

Update [Page] set [IsSystem] = 1 where [Guid] = '18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C'
Update [Page] set [IsSystem] = 1 where [Guid] = '7CA317B5-5C47-465D-B407-7D614F2A568F'
Update [Page] set [IsSystem] = 1 where [Guid] = 'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40'
Update [Page] set [IsSystem] = 1 where [Guid] = '84DB9BA0-2725-40A5-A3CA-9A1C043C31B0'
" );

            Sql( @"
INSERT INTO [dbo].[FieldType]
           ([IsSystem]
           ,[Name]
           ,[Description]
           ,[Assembly]
           ,[Class]
           ,[Guid])
     VALUES
           (1
           ,'Group Type'
           ,'List of Group Types'
           ,'Rock'
           ,'Rock.Field.Types.GroupTypeField'
           ,'F725B854-A15E-46AE-9D4C-0608D4154F1E')
" );

            Sql( "update [BlockType] set [Path] = '~/Blocks/Crm/GroupList.ascx', [Name] = 'Group List', [Description] = 'Group List' where [Guid] = '3D7FB6BE-6BBD-49F7-96B4-96310AF3048A'" );

            Sql( "update [Page] set [Name] = 'Group List', [Title] = 'Group List', [Description] = 'Group List' where [Guid] = '4D7624FB-A9AE-40BD-82CB-84C22F64343E'" );

            // Delete Roles.aspx blocktype
            DeleteBlockType( "EA5F81B5-9086-4D34-8339-46D26B5F775E" );

            // Delete old Security Roles page (delete will cascade and delete Block too)
            DeletePage( "BED5E775-E630-42E0-8A92-0806E513EAA2" );

            //Update Groups block into GroupList/GroupDetail
            Sql( "update [BlockType] set [Path] = '~/Blocks/Crm/GroupList.ascx' where [Guid] = '3D7FB6BE-6BBD-49F7-96B4-96310AF3048A'" );

            // Update Person PageList Block to only show 1 level deep
            Sql( @"
update [AttributeValue] set [Value] = 1 
where [AttributeId] = (select [Id] from [Attribute] where [Guid] = '9909E07F-0E68-43B8-A151-24D03C795093')
and EntityId = (select Id from Block where Guid = '8AFEAEDE-187A-4EE6-BB0E-702C582E8E02')" );

            AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "Security Roles", "Manage Security Roles", "D9678FEF-C086-4232-972C-5DBAC14BFEE6" );
            AddPage( "D9678FEF-C086-4232-972C-5DBAC14BFEE6", "Security Roles Detail", "", "48AAD428-A9C9-4BBB-A80F-B85F28D31240" );
            AddPage( "4D7624FB-A9AE-40BD-82CB-84C22F64343E", "Group Detail", "", "54F6365A-4E4C-4E2A-9498-70B09E062C69" );

            AddBlockType( "Group Detail", "~/Blocks/Crm/GroupDetail.ascx", "~/Blocks/Crm/GroupDetail.ascx", "582BEEA1-5B27-444D-BC0A-F60CEB053981" );

            AddBlock( "D9678FEF-C086-4232-972C-5DBAC14BFEE6", "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "Security Roles List", "Content", "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", 0 );
            AddBlock( "48AAD428-A9C9-4BBB-A80F-B85F28D31240", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Security Roles Detail", "Content", "B58919B6-0947-4FE6-A9AE-FB28194643E7", 0 );
            AddBlock( "54F6365A-4E4C-4E2A-9498-70B09E062C69", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "Group Detail", "Content", "D025306E-6820-42A6-8BF6-8606582D3DF5", 0 );

            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "", 6, "False", "1DAD66E3-8859-487E-8200-483C98DE2E07" );
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "", "Select group types to show in this block.  Leave all unchecked to show all group types.", 0, "", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324" );
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4" );
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show User Count", "ShowUserCount", "", "", 1, "True", "D7A5D717-6B3F-4033-B707-B92D81D402C2" );
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Description", "ShowDescription", "", "", 2, "True", "99AF141C-8F5F-4FB8-8748-837A4BFCFB94" );
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Edit", "ShowEdit", "", "", 3, "True", "0EC725C5-F6F7-47DC-ABC2-8A59B6033F45" );
            AddBlockTypeAttribute( "3D7FB6BE-6BBD-49F7-96B4-96310AF3048A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Notification", "ShowNotification", "", "", 5, "False", "D5B9A3DB-DD94-4B7C-A784-28BA691181E0" );
            AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Edit", "ShowEdit", "", "", 3, "True", "50C7E223-459E-4A1C-AE3C-2892CBD40D22" );
            AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "", "Select group types to show in this block.  Leave all unchecked to show all group types.", 0, "", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A" );
            AddBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "", 6, "False", "12295C7E-08F4-4AC5-8A34-C829620FC0B1" );

            // Attrib Value for Groups:Group Types
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324", "" );

            // Attrib Value for Groups:Show User Count
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "D7A5D717-6B3F-4033-B707-B92D81D402C2", "True" );

            // Attrib Value for Groups:Show Description
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "99AF141C-8F5F-4FB8-8748-837A4BFCFB94", "True" );

            // Attrib Value for Groups:Show Edit
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "0EC725C5-F6F7-47DC-ABC2-8A59B6033F45", "True" );

            // Attrib Value for Groups:Show Notification
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "D5B9A3DB-DD94-4B7C-A784-28BA691181E0", "False" );

            // Attrib Value for Groups:Limit to Security Role Groups
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "1DAD66E3-8859-487E-8200-483C98DE2E07", "False" );

            // Attrib Value for Security Roles List:Group Types
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324", "" );

            // Attrib Value for Security Roles List:Show User Count
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "D7A5D717-6B3F-4033-B707-B92D81D402C2", "True" );

            // Attrib Value for Security Roles List:Show Description
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "99AF141C-8F5F-4FB8-8748-837A4BFCFB94", "True" );

            // Attrib Value for Security Roles List:Show Edit
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "0EC725C5-F6F7-47DC-ABC2-8A59B6033F45", "True" );

            // Attrib Value for Security Roles List:Show Notification
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "D5B9A3DB-DD94-4B7C-A784-28BA691181E0", "False" );

            // Attrib Value for Security Roles List:Limit to Security Role Groups
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "1DAD66E3-8859-487E-8200-483C98DE2E07", "True" );

            // Attrib Value for Security Roles Detail:Group Types
            AddBlockAttributeValue( "B58919B6-0947-4FE6-A9AE-FB28194643E7", "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A", "" );

            // Attrib Value for Security Roles Detail:Show Edit
            AddBlockAttributeValue( "B58919B6-0947-4FE6-A9AE-FB28194643E7", "50C7E223-459E-4A1C-AE3C-2892CBD40D22", "True" );

            // Attrib Value for Security Roles Detail:Limit to Security Role Groups
            AddBlockAttributeValue( "B58919B6-0947-4FE6-A9AE-FB28194643E7", "12295C7E-08F4-4AC5-8A34-C829620FC0B1", "True" );

            // Attrib Value for Groups:Detail Page Guid
            AddBlockAttributeValue( "52B774FE-9ABF-4852-9496-6FAD4646F949", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", "54f6365a-4e4c-4e2a-9498-70b09e062c69" );

            // Attrib Value for Security Roles List:Detail Page Guid
            AddBlockAttributeValue( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4", "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4", "48aad428-a9c9-4bbb-a80f-b85f28d31240" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // unfix up Blocks, BlockTypes and Pages that should have had IsSystem = 1
            Sql( @"
Update [Block] set [IsSystem] = 0 where [Guid] = '6E189D68-C4EC-443F-B409-1EEC0F12D427'
Update [Block] set [IsSystem] = 0 where [Guid] = '71477D00-6A2B-4736-9F7D-2001B76DA638'
Update [Block] set [IsSystem] = 0 where [Guid] = '803CE253-3ADA-4C2A-B62F-EC4D5B7B7257'
Update [Block] set [IsSystem] = 0 where [Guid] = '60DED2D5-0675-452A-B82B-781B044BB856'
Update [Block] set [IsSystem] = 0 where [Guid] = '845100E9-30D6-45A8-9B80-052395735982'
Update [Block] set [IsSystem] = 0 where [Guid] = 'D5D2048C-52C6-4827-A817-9B84E0525510'
Update [Block] set [IsSystem] = 0 where [Guid] = 'BDE5E15D-0CC7-4164-837F-91ADA64A15D9'
Update [Block] set [IsSystem] = 0 where [Guid] = '091BDFEC-F76B-416B-B8F3-0A9DB93AF606'
Update [Block] set [IsSystem] = 0 where [Guid] = '8AFEAEDE-187A-4EE6-BB0E-702C582E8E02'
Update [Block] set [IsSystem] = 0 where [Guid] = 'BEDFF750-3EB8-4EE7-A8B4-23863FB0315D'
Update [Block] set [IsSystem] = 0 where [Guid] = '261EA4CF-F7CD-47DC-BC69-3B2D6EB87DD5'
Update [Block] set [IsSystem] = 0 where [Guid] = '718C516F-0A1D-4DBC-A939-1D9777208FEC'
Update [Block] set [IsSystem] = 0 where [Guid] = 'B8224C72-4168-40F0-96BE-38F2AFD525F5'
Update [Block] set [IsSystem] = 0 where [Guid] = '6C805618-75E7-470F-AC3E-390529ED94F1'
Update [Block] set [IsSystem] = 0 where [Guid] = 'B447AB11-3A19-4527-921A-2266A6B4E181'
Update [Block] set [IsSystem] = 0 where [Guid] = 'B8D83A2C-31F2-48C6-BEBC-753BCDC2A30C'
Update [Block] set [IsSystem] = 0 where [Guid] = '9126CFA2-9B26-4FBB-BB87-F76514221DBE'

Update [BlockType] set [IsSystem] = 0 where [Guid] in ('8C46B8AC-7962-410E-AC37-C1602E9675DD','90533F7D-6841-45E4-9141-1504C49A028D','69B695A4-AD43-4A73-83C0-4C13BE142ECA')

Update [Page] set [IsSystem] = 0 where [Guid] = '18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C'
Update [Page] set [IsSystem] = 0 where [Guid] = '7CA317B5-5C47-465D-B407-7D614F2A568F'
Update [Page] set [IsSystem] = 0 where [Guid] = 'E0E1DE66-B825-4BFB-A0B3-6E069AA9AA40'
Update [Page] set [IsSystem] = 0 where [Guid] = '84DB9BA0-2725-40A5-A3CA-9A1C043C31B0'
" );

            Sql( "DELETE FROM [dbo].[Attribute] WHERE [Key] like 'GroupType%'" );
            Sql( "DELETE FROM [dbo].[FieldType] WHERE [Guid] = 'F725B854-A15E-46AE-9D4C-0608D4154F1E'" );

            DeleteAttribute( "1DAD66E3-8859-487E-8200-483C98DE2E07" );
            DeleteAttribute( "C3FD6CE3-D37F-4A53-B0D7-AB1B1F252324" );
            DeleteAttribute( "1762F39E-7EE4-43AF-8F86-7D7B48D7424A" );
            DeleteAttribute( "D7A5D717-6B3F-4033-B707-B92D81D402C2" );
            DeleteAttribute( "99AF141C-8F5F-4FB8-8748-837A4BFCFB94" );
            DeleteAttribute( "0EC725C5-F6F7-47DC-ABC2-8A59B6033F45" );
            DeleteAttribute( "D5B9A3DB-DD94-4B7C-A784-28BA691181E0" );
            DeleteAttribute( "50C7E223-459E-4A1C-AE3C-2892CBD40D22" );
            DeleteAttribute( "15AC7A62-7BF2-44B7-93CD-EA8F96BF529A" );
            DeleteAttribute( "12295C7E-08F4-4AC5-8A34-C829620FC0B1" );
            DeleteAttribute( "8E57EC42-ABEE-4D35-B7FA-D8513880E8E4" );


            DeleteBlock( "3205122B-3EA8-4FEE-A516-2C64CA0F35F4" );
            DeleteBlock( "B58919B6-0947-4FE6-A9AE-FB28194643E7" );
            DeleteBlock( "D025306E-6820-42A6-8BF6-8606582D3DF5" );

            DeleteBlockType( "582BEEA1-5B27-444D-BC0A-F60CEB053981" );

            DeletePage( "54F6365A-4E4C-4E2A-9498-70B09E062C69" );
            DeletePage( "48AAD428-A9C9-4BBB-A80F-B85F28D31240" );
            DeletePage( "D9678FEF-C086-4232-972C-5DBAC14BFEE6" );
        }
    }
}
