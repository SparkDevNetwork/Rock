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
    public partial class UpdatePersonDetail : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    -- Update PageXslt page attribute to use guid instead of id
    UPDATE AV SET [Value] = P.[Guid]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId]
    INNER JOIN [Page] P ON P.[Id] = AV.[Value]
    WHERE A.[Guid] = 'DD516FA7-966E-4C80-8523-BEAC91C8EEDA'
    AND AV.[Value] <> ''
   
    -- Update icons for relationship group types
    UPDATE [GroupType] SET [IconCssClass] = 'icon-pushpin' WHERE [Guid] = '8C0E5852-F08F-4327-9AA5-87800A6AB53E'
    UPDATE [GroupType] SET [IconCssClass] = 'icon-exchange' WHERE [Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'

    -- Move person pages to root
    UPDATE [Page] SET 
         [ParentPageId] = null 
        ,[DisplayInNavWhen] = 0
    WHERE [Guid] = 'BF04BB7E-BE3A-4A38-A37C-386B55496303'

    DECLARE @PageId int
    SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '08DBD8A5-2C35-4146-B4A8-0F7652348B25')
    DELETE [Block] WHERE [PageId] = @PageId
" );

            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Extended Attributes", "", "PersonDetail", "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Groups", "", "PersonDetail", "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Staff Details", "", "PersonDetail", "0E56F56E-FB32-4827-A69A-B90D43CB47F5" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "Contributions", "", "PersonDetail", "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892" );
            AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "History", "", "PersonDetail", "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418" );

            AddPageRoute( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "Person/{PersonId}/ExtendedAttributes" );
            AddPageRoute( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D", "Person/{PersonId}/Groups" );
            AddPageRoute( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "Person/{PersonId}/StaffDetails" );
            AddPageRoute( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "Person/{PersonId}/Contributions" );
            AddPageRoute( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "Person/{PersonId}/History" );

            AddPageContext( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE", "Rock.Model.Person", "PersonId" );
            AddPageContext( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D", "Rock.Model.Person", "PersonId"  );
            AddPageContext( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", "Rock.Model.Person", "PersonId"  );
            AddPageContext( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "Rock.Model.Person", "PersonId" );
            AddPageContext( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "Rock.Model.Person", "PersonId" );

            AddBlockType( "CRM - Person Detail - Family Members", "", "~/Blocks/CRM/PersonDetail/FamilyMembers.ascx", "FC137BDA-4F05-4ECE-9899-A249C90D11FC" );
            AddBlockType( "CRM - Person Detail - Relationships", "", "~/Blocks/CRM/PersonDetail/Relationships.ascx", "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD" );

            // Family Members Block Type
            AddBlockTypeAttribute( "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Xslt File", "XsltFile", "", "XSLT File to use.", 0, "PersonDetail/FamilyMembers.xslt", "4B85F5F3-755E-4B14-91AD-56089BCD15B9" );

            // Sub Page Menu Block Type
            AddBlockTypeAttribute( "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current Parameters", "IncludeCurrentParameters", "", "Flag indicating if current page's parameters should be used when building url for child pages", 0, "False", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7" );

            // Notes Block Type
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Context Entity Type", "ContextEntityType", "Filter", "Context Entity Type", 0, "", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" );
            AddBlockTypeAttribute( "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Note Type", "NoteType", "", "The note type name associated with the context entity to use (If it doesn't exist it will be created).", 0, "Notes", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6" );

            // Relationship Block type
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Group Role Filter", "GroupRoleFilter", "", "Delimited list of group role id's that if entered, will only show groups where selected person is one of the roles.", 0, "", "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B" );
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "The type of group to display.  Any group of this type that person belongs to will be displayed", 0, "", "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E" );
            AddBlockTypeAttribute( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Role", "ShowRole", "", "Should the member's role be displayed with their name", 0, "False", "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6" );

            AddBlock( "", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "PersonDetail", "HeaderZone", 0, "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" );
            AddBlock( "", "FC137BDA-4F05-4ECE-9899-A249C90D11FC", "Family Members", "PersonDetail", "FamilyZone", 0, "4CC50BE8-72ED-43E0-8D11-7E2A590453CC" );
            AddBlock( "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Sub Page Menu", "PersonDetail", "TabsZone", 0, "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "TimeLine", "", "ContentZoneLeft", 0, "0B2B550C-B0C9-420E-9CF3-BEC8979108F2" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "Relationships", "", "ContentZoneRight", 0, "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136" );
            AddBlock( "08DBD8A5-2C35-4146-B4A8-0F7652348B25", "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD", "Peer Network", "", "ContentZoneRight", 1, "32847AAF-15F5-4F8B-9F84-92D6AE827857" );

            // Attrib Value for Sub Page Menu:Number of Levels
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );
            // Attrib Value for Sub Page Menu:Root Page
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "BF04BB7E-BE3A-4A38-A37C-386B55496303" );
            // Attrib Value for Sub Page Menu:XSLT File
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/PersonDetail/SubPageNav.xslt" );
            // Attrib Value for Sub Page Menu:Include Current Parameters
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

            // Attrib Value for TimeLine:Context Entity Type
            AddBlockAttributeValue( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", "Rock.Model.Person" );
            // Attrib Value for TimeLine:Note Type
            AddBlockAttributeValue( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6", "TimeLine" );

            // Attrib Value for Relationships:Group Type
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E", "E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF" );
            // Attrib Value for Relationships:Group Role Filter
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B", "7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42" );
            // Attrib Value for Relationships:Show Role
            AddBlockAttributeValue( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136", "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6", "True" );

            // Attrib Value for Peer Network:Group Type
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E", "8C0E5852-F08F-4327-9AA5-87800A6AB53E" );
            // Attrib Value for Peer Network:Group Role Filter
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B", "CB9A0E14-6FCF-4C07-A49A-D7873F45E196" );
            // Attrib Value for Peer Network:Show Role
            AddBlockAttributeValue( "32847AAF-15F5-4F8B-9F84-92D6AE827857", "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6", "False" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Relationships Block Type
            DeleteAttribute( "FE82ED76-2F25-442C-8AC1-A5532A1EBD9B" ); // Group Role Filter
            DeleteAttribute( "AC4C7B54-9CAA-4623-BE1F-2545985B2A8E" ); // Group Type
            DeleteAttribute( "D0D8CFBF-5A66-42EA-AE78-A8BF3FAE45E6" ); // Show Role

            // Notes
            DeleteAttribute( "F1BCF615-FBCA-4BC2-A912-C35C0DC04174" ); // Context Entity Type
            DeleteAttribute( "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6" ); // Note Type

            // Subpage
            DeleteAttribute( "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7" ); // Include Current Parameters

            // Family Members Block Type
            DeleteAttribute( "4B85F5F3-755E-4B14-91AD-56089BCD15B9" ); // Xslt File

            DeleteBlock( "D5CD2A87-D34E-42E0-A58A-5FFC9B72F136" ); // Relationships
            DeleteBlock( "32847AAF-15F5-4F8B-9F84-92D6AE827857" ); // Peer Network
            DeleteBlock( "0B2B550C-B0C9-420E-9CF3-BEC8979108F2" ); // Timeline
            DeleteBlock( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" ); // Sub Page Menu
            DeleteBlock( "4CC50BE8-72ED-43E0-8D11-7E2A590453CC" ); // Family Members
            DeleteBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" ); // Bio

            DeleteBlockType( "FC137BDA-4F05-4ECE-9899-A249C90D11FC" ); // CRM - Person Detail - Family Members
            DeleteBlockType( "77E409D4-11CD-4009-B4CD-4B75DF2CC9FD" ); // CRM - Person Detail - Relationships

            DeletePage( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE" ); // Extended Attributes
            DeletePage( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D" ); // Groups
            DeletePage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5" ); // Staff Details
            DeletePage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892" ); // Contributions
            DeletePage( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418" ); // History

            Sql( @"
    -- Update PageXslt page attribute to use id instead of guid
    UPDATE AV SET [Value] = P.[Id]
    FROM [AttributeValue] AV
    INNER JOIN [Attribute] A ON A.[Id] = AV.[AttributeId] 
    INNER JOIN [Page] P ON P.[Guid] = AV.[Value]
    WHERE A.[Guid] = 'DD516FA7-966E-4C80-8523-BEAC91C8EEDA'
    AND AV.[Value] <> ''
" );
    
        }
    }
}
