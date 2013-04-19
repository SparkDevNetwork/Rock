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
    public partial class AttendedCheckinPages : RockMigration
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

            AddBlock( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "2C51230E-BA2E-4646-BB10-817B26C16218", "Attended Search", "", "Content", 0, "9F8731AB-07DB-406F-A344-45E31D0DE301" );

            AddBlock( "8F618315-F554-4751-AB7F-00CC5658120A", "645D3F2F-0901-44FE-93E9-446DBC8A1680", "Attended Search", "", "Content", 0, "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" );

            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "4D48B5F0-F0B2-4C10-8498-DAF690761A80", "Attended Family Select", "", "Content", 0, "82929409-8551-413C-972A-98EDBC23F420" );
            AddBlock( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "0DF27F26-691D-41F8-B0F7-987E4FEC375C", "Idle Redirect", "", "Content", 1, "BDD502FF-40D2-42E6-845E-95C49C3505B3" );

            // Page Routes for Attended Checkin
            AddPageRoute( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "attendedcheckin" );
            AddPageRoute( "771E3CF1-63BD-4880-BC43-AC29B4CCE963", "attendedcheckin/admin" );
            AddPageRoute( "8F618315-F554-4751-AB7F-00CC5658120A", "attendedcheckin/search" );
            AddPageRoute( "AF83D0B2-2995-4E46-B0DF-1A4763637A68", "attendedcheckin/family" );
            AddPageRoute( "C87916FE-417E-4A11-8831-5CFA7678A228", "attendedcheckin/activity" );
            AddPageRoute( "BE996C9B-3DFE-407F-BD53-D6F58D85A035", "attendedcheckin/confirm" );

            AddDefinedValue( "1EBCDB30-A89A-4C14-8580-8289EC2C7742", "Name", "Search for family based on name", "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteDefinedValue( "071D6DAA-3063-463A-B8A1-7D9A1BE1BB31" ); // DefinedValue search by name

            DeleteBlock( "9F8731AB-07DB-406F-A344-45E31D0DE301" ); // Attended Admin
            DeleteBlock( "182C9AA0-E76F-4AAF-9F61-5418EE5A0CDB" ); // Attended Search
            DeleteBlock( "82929409-8551-413C-972A-98EDBC23F420" ); // Attended Family Select
            DeleteBlock( "BDD502FF-40D2-42E6-845E-95C49C3505B3" ); // Idle Redirect
            DeleteBlockType( "2C51230E-BA2E-4646-BB10-817B26C16218" ); // Attended Check In - Admin
            DeleteBlockType( "645D3F2F-0901-44FE-93E9-446DBC8A1680" ); // Attended Check In - Search
            DeleteBlockType( "4D48B5F0-F0B2-4C10-8498-DAF690761A80" ); // Attended Check In - Family Select
            DeletePage( "771E3CF1-63BD-4880-BC43-AC29B4CCE963" ); // Admin
            DeletePage( "8F618315-F554-4751-AB7F-00CC5658120A" ); // Search
            DeletePage( "AF83D0B2-2995-4E46-B0DF-1A4763637A68" ); // Family Select
            DeletePage( "C87916FE-417E-4A11-8831-5CFA7678A228" ); // Activity Select
            DeletePage( "BE996C9B-3DFE-407F-BD53-D6F58D85A035" ); // Confirmation
            DeletePage( "32A132A6-63A2-4840-B4A5-23D80994CCBD" ); // Attended Checkin
        }
    }
}
