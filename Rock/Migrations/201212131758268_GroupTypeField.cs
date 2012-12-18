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
    public partial class GroupTypeField : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( "update FieldType set Class = 'Rock.Field.Types.GroupTypesField' where Guid = 'F725B854-A15E-46AE-9D4C-0608D4154F1E'" );
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
           ,'Group'
           ,'Group'
           ,'Rock'
           ,'Rock.Field.Types.GroupField'
           ,'F4399CEF-827B-48B2-A735-F7806FCFE8E8'
           )
          " );

            AddPage( "91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F", "Group Viewer", "Group Viewer", "TwoColumnLeft", "4E237286-B715-4109-A578-C1445EC02707" );

            AddBlockType( "Person Details", "~/Blocks/PersonDetails.ascx", "~/Blocks/PersonDetails.ascx", "CB337944-CAC7-4040-BD67-44F44AD3D6D4" );
            AddBlockType( "Notes", "~/Blocks/Core/Notes.ascx", "~/Blocks/Core/Notes.ascx", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3" );
            AddBlockType( "Group Tree View", "~/Blocks/Crm/GroupTreeView.ascx", "~/Blocks/Crm/GroupTreeView.ascx", "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" );
            AddBlockType( "Disc", "~/Blocks/Crm/DiscAssessment/Disc.ascx", "~/Blocks/Crm/DiscAssessment/Disc.ascx", "A161D12D-FEA7-422F-B00E-A689629680E4" );

            AddBlock( "4E237286-B715-4109-A578-C1445EC02707", "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "GroupViewLeft", "LeftContent", "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", 0 );
            AddBlock( "4E237286-B715-4109-A578-C1445EC02707", "582BEEA1-5B27-444D-BC0A-F60CEB053981", "GroupDetailRight", "RightContent", "88344FE3-737E-4741-A38D-D2D3A1653818", 0 );

            AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Limit to Security Role Groups", "LimittoSecurityRoleGroups", "", "", 3, "False", "1688837B-73CF-46C3-8880-74C46605807C" );
            AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "F725B854-A15E-46AE-9D4C-0608D4154F1E", "Group Types", "GroupTypes", "", "Select group types to show in this block.  Leave all unchecked to show all group types.", 1, "", "12557F76-B0AF-4327-8884-C664B08453AE" );
            AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Group", "Group", "", "Select the root group to show in this block.", 2, "", "0E1768CD-87CC-4361-8BCD-01981FBFE24B" );
            AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page Guid", "DetailPageGuid", "Advanced", "", 0, "", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2" );
            AddBlockTypeAttribute( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Treeview Title", "TreeviewTitle", "", "", 0, "Group Tree View", "D1583306-2504-48D2-98EE-3DE55C2806C7" );

            // Attrib Value for GroupViewLeft:Group Types
            AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "12557F76-B0AF-4327-8884-C664B08453AE", "" );
            // Attrib Value for GroupViewLeft:Detail Page Guid
            AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2", "4e237286-b715-4109-a578-c1445ec02707" );
            // Attrib Value for GroupViewLeft:Group
            AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "0E1768CD-87CC-4361-8BCD-01981FBFE24B", "" );
            // Attrib Value for GroupViewLeft:Limit to Security Role Groups
            AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "1688837B-73CF-46C3-8880-74C46605807C", "False" );
            // Attrib Value for GroupViewLeft:Treeview Title
            AddBlockAttributeValue( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD", "D1583306-2504-48D2-98EE-3DE55C2806C7", "All Groups" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "1688837B-73CF-46C3-8880-74C46605807C" );
            DeleteAttribute( "12557F76-B0AF-4327-8884-C664B08453AE" );
            DeleteAttribute( "0E1768CD-87CC-4361-8BCD-01981FBFE24B" );
            DeleteAttribute( "ADCC4391-8D8B-4A28-80AF-24CD6D3F77E2" );
            DeleteAttribute( "D1583306-2504-48D2-98EE-3DE55C2806C7" );
            DeleteBlock( "95612FCE-C40B-4CBB-AE26-800B52BE5FCD" );
            DeleteBlock( "88344FE3-737E-4741-A38D-D2D3A1653818" );
            DeleteBlockType( "CB337944-CAC7-4040-BD67-44F44AD3D6D4" );
            DeleteBlockType( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3" );
            DeleteBlockType( "2D26A2C4-62DC-4680-8219-A52EB2BC0F65" );
            DeleteBlockType( "A161D12D-FEA7-422F-B00E-A689629680E4" );
            DeletePage( "4E237286-B715-4109-A578-C1445EC02707" );

            Sql( "DELETE FROM [FieldType] where Guid = 'F4399CEF-827B-48B2-A735-F7806FCFE8E8'" );
            Sql( "update FieldType set Name = 'Group Type', Class = 'Rock.Field.Types.GroupTypeField' where Guid = 'F725B854-A15E-46AE-9D4C-0608D4154F1E'" );
        }
    }
}
