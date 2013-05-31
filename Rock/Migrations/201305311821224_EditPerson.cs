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
    public partial class EditPerson : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateEntityType( "Rock.PersonProfile.Badge.Campus", "Campus", "Rock.PersonProfile.Badge.Campus, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "D4B2BA9B-4F2C-47CB-A5BB-F3FF53A68F39" );
            UpdateEntityType( "Rock.PersonProfile.Badge.DiscScore", "Disc Score", "Rock.PersonProfile.Badge.DiscScore, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "208A2FE8-9CC8-4608-891A-A62CE29BE05B" );
            UpdateEntityType( "Rock.PersonProfile.Badge.eRA", "e RA", "Rock.PersonProfile.Badge.eRA, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "5EE996CB-EA8A-4EEF-88D2-F11510AEE5CD" );
            UpdateEntityType( "Rock.PersonProfile.Badge.eraAttendanceAttendance", "era Attendance Attendance", "Rock.PersonProfile.Badge.eraAttendanceAttendance, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "D046A808-06A2-4131-88AF-A97166840E97" );
            UpdateEntityType( "Rock.PersonProfile.Badge.FamilyAttendance", "Family Attendance", "Rock.PersonProfile.Badge.FamilyAttendance, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "78F5527E-0E90-4AC9-AAAB-F8F2F061BDFB" );
            UpdateEntityType( "Rock.PersonProfile.Badge.NextSteps", "Next Steps", "Rock.PersonProfile.Badge.NextSteps, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "13E3B42C-7E7F-41A6-940B-FA0DDC5E647F" );
            UpdateEntityType( "Rock.PersonProfile.Badge.PersonStatus", "Person Status", "Rock.PersonProfile.Badge.PersonStatus, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "3D093330-D547-454B-8956-B76D8F9B8536" );
            UpdateEntityType( "Rock.PersonProfile.Badge.RecordStatus", "Record Status", "Rock.PersonProfile.Badge.RecordStatus, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "09E8CD24-BE34-4DC0-8B43-A7ACE0549CE0" );
            UpdateEntityType( "Rock.PersonProfile.Badge.Serving", "Serving", "Rock.PersonProfile.Badge.Serving, Rock, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", false, true, "7ADBA2D4-663F-4039-AA04-E0AEC81B5A21" );

            AddPage( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "EditPerson", "", "Default", "AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC", "" );
            AddBlockType( "CRM - Person Detail - Edit Person", "", "~/Blocks/CRM/PersonDetail/EditPerson.ascx", "0A15F28C-4828-4B38-AF66-58AC5BDE48E0" ); 
            AddBlock( "AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC", "0A15F28C-4828-4B38-AF66-58AC5BDE48E0", "Edit Person", "", "Content", 0, "59C7EA79-2073-4EA9-B439-7E74F06E8F5B" );

            AddPageRoute( "AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC", "Person/{PersonId}/Edit" );
            AddPageContext( "AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC", "Rock.Model.Person", "PersonId" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "59C7EA79-2073-4EA9-B439-7E74F06E8F5B" ); // Edit Person
            DeleteBlockType( "0A15F28C-4828-4B38-AF66-58AC5BDE48E0" ); // CRM - Person Detail - Edit Person
            DeletePage( "AD899394-13AD-4CAB-BCB3-CFFD79C9ADCC" ); // EditPerson
        }
    }
}
