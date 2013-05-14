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

            AddBlock( "", "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "Bio", "PersonDetail", "HeaderZone", 0, "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" );
            AddBlock( "", "3E14B410-22CB-49CC-8A1F-C30ECD0E816A", "Families", "PersonDetail", "FamilyZone", 0, "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750" );
            AddBlock( "", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Sub Page Menu", "PersonDetail", "TabsZone", 0, "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" );

            AddBlockTypeAttribute( "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Current Parameters", "IncludeCurrentParameters", "", "Flag indicating if current page's parameters should be used when building url for child pages", 0, "False", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7" );

            // Attrib Value for Families:Xslt File
            AddBlockAttributeValue( "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750", "69A88BCD-02DA-4600-AE9B-ADF30D41EE58", "PersonDetail/FamilyMembers.xslt" );
            // Attrib Value for Families:Include Self
            AddBlockAttributeValue( "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750", "BD82A9CA-BB0C-47B4-90FD-3A8D4FDBDCEA", "False" );
            // Attrib Value for Families:Include Locations
            AddBlockAttributeValue( "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750", "0504FB69-C7EE-432F-B232-A705AACD4858", "True" );
            // Attrib Value for Families:Group Type
            AddBlockAttributeValue( "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750", "B84EB1CB-E719-4444-B739-B0112AA20BBA", "" );
            // Attrib Value for Families:Group Role Filter
            AddBlockAttributeValue( "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750", "19EAAFBB-0669-4BC7-B69C-4DADB904BA8B", "" );
            // Attrib Value for Sub Page Menu:Number of Levels
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );
            // Attrib Value for Sub Page Menu:Root Page
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "92" );
            // Attrib Value for Sub Page Menu:XSLT File
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Themes/RockChMS/Assets/Xslt/PersonDetail/SubPageNav.xslt" );
            // Attrib Value for Sub Page Menu:Include Current Parameters
            AddBlockAttributeValue( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "True" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteAttribute( "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7" ); // Include Current Parameters

            DeleteBlock( "B5C1FDB6-0224-43E4-8E26-6B2EAF86253A" ); // Bio
            DeleteBlock( "018BA800-94DF-4BE5-A6E3-CBDC8FAEF750" ); // Families
            DeleteBlock( "F82E5FF2-F412-405C-9CC5-BF6E0401EB38" ); // Sub Page Menu

            DeletePage( "1C737278-4CBA-404B-B6B3-E3F0E05AB5FE" ); // Extended Attributes
            DeletePage( "183B7B7E-105A-4C9A-A4BC-06CD26B7FE6D" ); // Groups
            DeletePage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5" ); // Staff Details
            DeletePage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892" ); // Contributions
            DeletePage( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418" ); // History
        }
    }
}
