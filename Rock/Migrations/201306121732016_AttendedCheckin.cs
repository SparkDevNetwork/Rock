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
    public partial class AttendedCheckin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "Attended Checkin", "Screens for managing Attended Checkin", "32A132A6-63A2-4840-B4A5-23D80994CCBD" );
            Sql( " UPDATE [Page] SET [ParentPageId] = NULL WHERE [Guid] = '32A132A6-63A2-4840-B4A5-23D80994CCBD' " );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Admin", "Admin screen for Attended Checkin", "Checkin", "771E3CF1-63BD-4880-BC43-AC29B4CCE963" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Search", "Search screen for Attended Checkin", "Checkin", "8F618315-F554-4751-AB7F-00CC5658120A" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Family Select", "Family select for Attended Checkin", "Checkin", "AF83D0B2-2995-4E46-B0DF-1A4763637A68" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Activity Select", "Activity select for Attended Checkin", "Checkin", "C87916FE-417E-4A11-8831-5CFA7678A228" );
            AddPage( "32A132A6-63A2-4840-B4A5-23D80994CCBD", "Confirmation", "Confirmation screen for Attended Checkin", "Checkin", "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );

            AddBlockType( "Attended Check In - Admin", "Admin screen for Attended Checkin", "~/Blocks/CheckIn/Attended/Admin.ascx", "2C51230E-BA2E-4646-BB10-817B26C16218" );
            AddBlockType( "Attended Check In - Search", "Search screen for Attended Checkin", "~/Blocks/CheckIn/Attended/Search.ascx", "645D3F2F-0901-44FE-93E9-446DBC8A1680" );
            AddBlockType( "Attended Check In - Family Select", "Family select for Attended Checkin", "~/Blocks/CheckIn/Attended/FamilySelect.ascx", "4D48B5F0-F0B2-4C10-8498-DAF690761A80" );
            AddBlockType( "Attended Check In - Activity Select", "Activity select for Attended Checkin", "~/Blocks/CheckIn/Attended/ActivitySelect.ascx", "78E2AB4A-FDF7-4864-92F7-F052050BC4BB" );
            AddBlockType( "Attended Check In - Confirmation", "Confirmation for Attended Checkin", "~/Blocks/CheckIn/Attended/Confirm.ascx", "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F" );

            AddBlock( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "2C51230E-BA2E-4646-BB10-817B26C16218", "Admin", "", "Content", 0, "9F8731AB-07DB-406F-A344-45E31D0DE301" );
            AddBlock( "8F618315-F554-4751-AB7F-00CC5658120A", "645D3F2F-0901-44FE-93E9-446DBC8A1680", "Search", "", "Content", 0, "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" );
            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "Family Select", "", "Content", 0, "82929409-8551-413C-972A-98EDBC23F420" );
            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "BDD502FF-40D2-42E6-845E-95C49C3505B3" );
            AddBlock( "C87916FE-417E-4A11-8831-5CFA7678A228", "78E2AB4A-FDF7-4864-92F7-F052050BC4BB", "Activity Select", "", "Content", 0, "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4" );
            AddBlock( "BE996C9B-3DFE-407F-BD53-D6F58D85A035", "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F", "Confirmation", "", "Content", 0, "7CC68DD4-A6EF-4B67-9FEA-A144C479E058" );

            // Page Routes for Attended Checkin
            AddPageRoute( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "attendedcheckin" );
            AddPageRoute( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "attendedcheckin/admin" );
            AddPageRoute( "8F618315-F554-4751-AB7F-00CC5658120A", "attendedcheckin/search" );
            AddPageRoute( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "attendedcheckin/family" );
            AddPageRoute( "C87916FE-417E-4A11-8831-5CFA7678A228", "attendedcheckin/activity" );
            AddPageRoute( "BE996C9B-3DFE-407F-BD53-D6F58D85A035", "attendedcheckin/confirm" );

            // DefinedValue for name searches
            AddDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Name", "Search for family based on name", "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" );

            // Add Block Attributes
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "B196160E-4397-4C6F-8C5A-317CAD3C118F" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "7332D1F1-A1A5-48AE-BAB9-91C3AF085DB0" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "18864DE7-F075-437D-BA72-A6054C209FA5" );
            AddBlockTypeAttribute( "2C51230E-BA2E-4646-BB10-817B26C16218", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "40F39C36-3092-4B87-81F8-A9B1C6B261B2" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Admin Page", "AdminPage", "", "", 0, "", "BBB93FF9-C021-4E82-8C03-55942FA4141E" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "72E40960-2072-4F08-8EA8-5A766B49A2E0" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "BF8AAB12-57A2-4F50-992C-428C5DDCB89B" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "C4E992EA-62AE-4211-BE5A-9EEF5131235C" );
            AddBlockTypeAttribute( "645D3F2F-0901-44FE-93E9-446DBC8A1680", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "EBE397EF-07FF-4B97-BFF3-152D139F9B80" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "DD9F93C9-009B-4FA5-8FF9-B186E4969ACB" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "81A02B6F-F760-4110-839C-4507CF285A7E" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "338CAD91-3272-465B-B768-0AC2F07A0B40" );
            AddBlockTypeAttribute( "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "2DF1D39B-DFC7-4FB2-B638-3D99C3C4F4DF" );
            AddBlockTypeAttribute( "78E2AB4A-FDF7-4864-92F7-F052050BC4BB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "6048A23D-6544-441A-A8B3-5782CAF5B468" );
            AddBlockTypeAttribute( "78E2AB4A-FDF7-4864-92F7-F052050BC4BB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "39008E18-48C9-445F-B9D7-78334B76A7EE" );
            AddBlockTypeAttribute( "78E2AB4A-FDF7-4864-92F7-F052050BC4BB", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "BEC10B87-4B19-4CD5-8952-A4D59DDA3E9C" );
            AddBlockTypeAttribute( "78E2AB4A-FDF7-4864-92F7-F052050BC4BB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "5046A353-D901-45BB-9981-9CC1B33550C6" );
            AddBlockTypeAttribute( "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Previous Page", "PreviousPage", "", "", 0, "", "E45D2B10-D1B1-4CBE-9C7A-3098B1D95F47" );
            AddBlockTypeAttribute( "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Next Page", "NextPage", "", "", 0, "", "48813610-DD26-4E72-9D19-817535802C49" );
            AddBlockTypeAttribute( "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Workflow Type Id", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in", 0, "0", "2A71729F-E7CA-4ACD-9996-A6A661A069FD" );
            AddBlockTypeAttribute( "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Home Page", "HomePage", "", "", 0, "", "DEB23724-94F9-4164-BFAB-AD2DDE1F90ED" );

            // Attrib Value for Admin:Previous Page -> There is no previous page
            AddBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "B196160E-4397-4C6F-8C5A-317CAD3C118F", "00000000-0000-0000-0000-000000000000" );

            // Attrib Value for Admin:Next Page -> Goes to search page
            AddBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "7332D1F1-A1A5-48AE-BAB9-91C3AF085DB0", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Admin:Workflow Type Id
            AddBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "18864DE7-F075-437D-BA72-A6054C209FA5", "0" );

            // Attrib Value for Admin:Home Page -> Goes to search page
            AddBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "40F39C36-3092-4B87-81F8-A9B1C6B261B2", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Search:Admin Page
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "BBB93FF9-C021-4E82-8C03-55942FA4141E", "771e3cf1-63bd-4880-bc43-ac29b4cce963" );

            // Attrib Value for Search:Previous Page -> Goes to confirmation page
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "72E40960-2072-4F08-8EA8-5A766B49A2E0", "be996c9b-3dfe-407f-bd53-d6f58d85a035" );

            // Attrib Value for Search:Next Page -> Goes to family select page
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "BF8AAB12-57A2-4F50-992C-428C5DDCB89B", "af83d0b2-2995-4e46-b0df-1a4763637a68" );

            // Attrib Value for Search:Workflow Type Id
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "C4E992EA-62AE-4211-BE5A-9EEF5131235C", "0" );

            // Attrib Value for Search:Home Page -> Goes to search page
            AddBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "EBE397EF-07FF-4B97-BFF3-152D139F9B80", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Family Select:Previous Page -> Goes to search page
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "DD9F93C9-009B-4FA5-8FF9-B186E4969ACB", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Family Select:Next Page -> Goes to confirmation page
            //AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "81A02B6F-F760-4110-839C-4507CF285A7E", "c87916fe-417e-4a11-8831-5cfa7678a228" );
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "81A02B6F-F760-4110-839C-4507CF285A7E", "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );

            // Attrib Value for Family Select:Workflow Type Id
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "338CAD91-3272-465B-B768-0AC2F07A0B40", "0" );

            // Attrib Value for Family Select:Home Page -> Goes to search page
            AddBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "2DF1D39B-DFC7-4FB2-B638-3D99C3C4F4DF", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Family Select:Redirect Page -> Goes to search page
            AddBlockAttributeValue( "BDD502FF-40D2-42E6-845E-95C49C3505B3", "C4204D6E-715E-4E3A-BA1B-949D20D26487", "search" );

            // Attrib Value for Family Select:Timeout Value
            AddBlockAttributeValue( "BDD502FF-40D2-42E6-845E-95C49C3505B3", "A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD", "120" );

            // Attrib Value for Activity Select:Previous Page -> Goes to confirmation page
            //AddBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "6048A23D-6544-441A-A8B3-5782CAF5B468", "AF83D0B2-2995-4E46-B0DF-1A4763637A68" );
            AddBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "6048A23D-6544-441A-A8B3-5782CAF5B468", "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );

            // Attrib Value for Activity Select:Next Page -> Goes to confirmation page
            AddBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "39008E18-48C9-445F-B9D7-78334B76A7EE", "BE996C9B-3DFE-407F-BD53-D6F58D85A035" );

            // Attrib Value for Activity Select:Workflow Type Id
            AddBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "BEC10B87-4B19-4CD5-8952-A4D59DDA3E9C", "0" );

            // Attrib Value for Activity Select:Home Page -> Goes to search page
            AddBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "5046A353-D901-45BB-9981-9CC1B33550C6", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Confirmation:Previous Page -> Goes to family select page
            //AddBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "E45D2B10-D1B1-4CBE-9C7A-3098B1D95F47", "C87916FE-417E-4A11-8831-5CFA7678A228" );
            AddBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "E45D2B10-D1B1-4CBE-9C7A-3098B1D95F47", "AF83D0B2-2995-4E46-B0DF-1A4763637A68" );
            
            // Attrib Value for Confirmation:Next Page -> Goes to search page
            AddBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "48813610-DD26-4E72-9D19-817535802C49", "8f618315-f554-4751-ab7f-00cc5658120a" );

            // Attrib Value for Confirmation:Workflow Type Id
            AddBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "2A71729F-E7CA-4ACD-9996-A6A661A069FD", "0" );

            // Attrib Value for Confirmation:Home Page -> Goes to search page
            AddBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "DEB23724-94F9-4164-BFAB-AD2DDE1F90ED", "8f618315-f554-4751-ab7f-00cc5658120a" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block Attributes
            DeleteBlockAttributeValue( "BDD502FF-40D2-42E6-845E-95C49C3505B3", "A7F99980-BED4-4A80-AB83-DDAB5C7D7AAD" );
            DeleteBlockAttributeValue( "BDD502FF-40D2-42E6-845E-95C49C3505B3", "C4204D6E-715E-4E3A-BA1B-949D20D26487" );
            DeleteBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "2DF1D39B-DFC7-4FB2-B638-3D99C3C4F4DF" );
            DeleteBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "338CAD91-3272-465B-B768-0AC2F07A0B40" );
            DeleteBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "81A02B6F-F760-4110-839C-4507CF285A7E" );
            DeleteBlockAttributeValue( "82929409-8551-413C-972A-98EDBC23F420", "DD9F93C9-009B-4FA5-8FF9-B186E4969ACB" );
            DeleteBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "BEC10B87-4B19-4CD5-8952-A4D59DDA3E9C" );
            DeleteBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "C4E992EA-62AE-4211-BE5A-9EEF5131235C" );
            DeleteBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "BF8AAB12-57A2-4F50-992C-428C5DDCB89B" );
            DeleteBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "72E40960-2072-4F08-8EA8-5A766B49A2E0" );
            DeleteBlockAttributeValue( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB", "BBB93FF9-C021-4E82-8C03-55942FA4141E" );
            DeleteBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "40F39C36-3092-4B87-81F8-A9B1C6B261B2" );
            DeleteBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "18864DE7-F075-437D-BA72-A6054C209FA5" );
            DeleteBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "7332D1F1-A1A5-48AE-BAB9-91C3AF085DB0" );
            DeleteBlockAttributeValue( "9F8731AB-07DB-406F-A344-45E31D0DE301", "B196160E-4397-4C6F-8C5A-317CAD3C118F" );
            DeleteBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "5046A353-D901-45BB-9981-9CC1B33550C6" );
            DeleteBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "BEC10B87-4B19-4CD5-8952-A4D59DDA3E9C" );
            DeleteBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "39008E18-48C9-445F-B9D7-78334B76A7EE" );
            DeleteBlockAttributeValue( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4", "6048A23D-6544-441A-A8B3-5782CAF5B468" );
            DeleteBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "DEB23724-94F9-4164-BFAB-AD2DDE1F90ED" );
            DeleteBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "2A71729F-E7CA-4ACD-9996-A6A661A069FD" );
            DeleteBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "48813610-DD26-4E72-9D19-817535802C49" );
            DeleteBlockAttributeValue( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058", "E45D2B10-D1B1-4CBE-9C7A-3098B1D95F47" );

            DeleteAttribute( "B196160E-4397-4C6F-8C5A-317CAD3C118F" ); // Previous Page
            DeleteAttribute( "7332D1F1-A1A5-48AE-BAB9-91C3AF085DB0" ); // Next Page
            DeleteAttribute( "18864DE7-F075-437D-BA72-A6054C209FA5" ); // Workflow Type Id
            DeleteAttribute( "40F39C36-3092-4B87-81F8-A9B1C6B261B2" ); // Home Page
            DeleteAttribute( "BBB93FF9-C021-4E82-8C03-55942FA4141E" ); // Admin Page
            DeleteAttribute( "72E40960-2072-4F08-8EA8-5A766B49A2E0" ); // Previous Page
            DeleteAttribute( "BF8AAB12-57A2-4F50-992C-428C5DDCB89B" ); // Next Page
            DeleteAttribute( "C4E992EA-62AE-4211-BE5A-9EEF5131235C" ); // Workflow Type Id
            DeleteAttribute( "EBE397EF-07FF-4B97-BFF3-152D139F9B80" ); // Home Page
            DeleteAttribute( "DD9F93C9-009B-4FA5-8FF9-B186E4969ACB" ); // Previous Page
            DeleteAttribute( "81A02B6F-F760-4110-839C-4507CF285A7E" ); // Next Page
            DeleteAttribute( "338CAD91-3272-465B-B768-0AC2F07A0B40" ); // Workflow Type Id
            DeleteAttribute( "2DF1D39B-DFC7-4FB2-B638-3D99C3C4F4DF" ); // Home Page
            DeleteAttribute( "6048A23D-6544-441A-A8B3-5782CAF5B468" ); // Previous Page
            DeleteAttribute( "39008E18-48C9-445F-B9D7-78334B76A7EE" ); // Next Page
            DeleteAttribute( "BEC10B87-4B19-4CD5-8952-A4D59DDA3E9C" ); // Workflow Type Id
            DeleteAttribute( "5046A353-D901-45BB-9981-9CC1B33550C6" ); // Home Page
            DeleteAttribute( "E45D2B10-D1B1-4CBE-9C7A-3098B1D95F47" ); // Previous Page
            DeleteAttribute( "48813610-DD26-4E72-9D19-817535802C49" ); // Next Page
            DeleteAttribute( "2A71729F-E7CA-4ACD-9996-A6A661A069FD" ); // Workflow Type Id
            DeleteAttribute( "DEB23724-94F9-4164-BFAB-AD2DDE1F90ED" ); // Home Page

            DeleteDefinedValue( "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" ); // DefinedValue search by name

            DeleteBlock( "9F8731AB-07DB-406F-A344-45E31D0DE301" ); // Attended Admin
            DeleteBlock( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" ); // Attended Search
            DeleteBlock( "82929409-8551-413C-972A-98EDBC23F420" ); // Attended Family Select
            DeleteBlock( "BDD502FF-40D2-42E6-845E-95C49C3505B3" ); // Idle Redirect
            DeleteBlock( "8C8CBBE9-2502-4FEC-804D-C0DA13C07FA4" ); // Attended Activity Select
            DeleteBlock( "7CC68DD4-A6EF-4B67-9FEA-A144C479E058" ); // Attended Confirmation
            DeleteBlockType( "2C51230E-BA2E-4646-BB10-817B26C16218" ); // Attended Check In - Admin
            DeleteBlockType( "645D3F2F-0901-44FE-93E9-446DBC8A1680" ); // Attended Check In - Search
            DeleteBlockType( "4D48B5F0-F0B2-4C10-8498-DAF690761A80" ); // Attended Check In - Family Select
            DeleteBlockType( "78E2AB4A-FDF7-4864-92F7-F052050BC4BB" ); // Attended Check In - Activity Select
            DeleteBlockType( "5B1D4187-9B34-4AB6-AC57-7E2CF67B266F" ); // Attended Check In - Confirmation
            DeletePage( "771E3CF1-63BD-4880-BC43-AC29B4CCE963" ); // Admin
            DeletePage( "8F618315-F554-4751-AB7F-00CC5658120A" ); // Search
            DeletePage( "AF83D0B2-2995-4E46-B0DF-1A4763637A68" ); // Family Select
            DeletePage( "C87916FE-417E-4A11-8831-5CFA7678A228" ); // Activity Select
            DeletePage( "BE996C9B-3DFE-407F-BD53-D6F58D85A035" ); // Confirmation
            DeletePage( "32A132A6-63A2-4840-B4A5-23D80994CCBD" ); // Attended Checkin
        }
    }
}
