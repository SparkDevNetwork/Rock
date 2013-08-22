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
    public partial class CheckinAdminPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage( "550A898C-EDEA-48B5-9C58-B20EC13AF13B", "Check-in", "", "Default", "FB0A7D8A-F9F4-4081-B15B-7970D20698E3", "" );
            AddPage( "FB0A7D8A-F9F4-4081-B15B-7970D20698E3", "Schedule Builder", "", "Default", "F9B48E2A-7D49-45B6-AA88-D731AD887B0F", "icon-calendar" );

            AddBlockType( "Administration - Check-in Schedule Builder", "", "~/Blocks/Administration/CheckinScheduleBuilder.ascx", "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4" );

            AddBlock( "F9B48E2A-7D49-45B6-AA88-D731AD887B0F", "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4", "Check-in Schedule Builder", "", "Content", 0, "282B34B6-354F-41F3-97A2-16DEC1B657E0" );
            AddBlock( "FB0A7D8A-F9F4-4081-B15B-7970D20698E3", "F49AD5F8-1E45-41E7-A88E-8CD285815BD9", "Page Xslt Transformation", "", "Content", 0, "77D37F89-F305-4E0E-950C-AA1F0F926580" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Root Page, Page:Check-in
            AddBlockAttributeValue( "77D37F89-F305-4E0E-950C-AA1F0F926580", "DD516FA7-966E-4C80-8523-BEAC91C8EEDA", "00000000-0000-0000-0000-000000000000" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:XSLT File, Page:Check-in
            AddBlockAttributeValue( "77D37F89-F305-4E0E-950C-AA1F0F926580", "D8A029F8-83BE-454A-99D3-94D879EBF87C", "~/Assets/XSLT/PageListAsBlocks.xslt" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Number of Levels, Page:Check-in
            AddBlockAttributeValue( "77D37F89-F305-4E0E-950C-AA1F0F926580", "9909E07F-0E68-43B8-A151-24D03C795093", "1" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current Parameters, Page:Check-in
            AddBlockAttributeValue( "77D37F89-F305-4E0E-950C-AA1F0F926580", "A0B1F15A-8735-48CA-AFF5-10ED7DD24EA7", "False" );

            // Attrib Value for Block:Page Xslt Transformation, Attribute:Include Current QueryString, Page:Check-in
            AddBlockAttributeValue( "77D37F89-F305-4E0E-950C-AA1F0F926580", "09D9AB6B-B2E6-4E9C-AAFE-25758BE2754B", "False" );

            // Make Check-in 2nd in the Menu and shift the others 
            Sql( @"
Update [Page] set [Order] = 0 where [Guid] = '0B213645-FA4E-44A5-8E4C-B2D8EF054985' -- General Settings
Update [Page] set [Order] = 1 where [Guid] = 'FB0A7D8A-F9F4-4081-B15B-7970D20698E3' -- Check-in
Update [Page] set [Order] = 2 where [Guid] = 'B4A24AB7-9369-4055-883F-4F4892C39AE3' -- CMS Configuration
Update [Page] set [Order] = 3 where [Guid] = '91CCB1C9-5F9F-44F5-8BE2-9EC3A3CFD46F' -- Person Settings
Update [Page] set [Order] = 4 where [Guid] = '199DC522-F4D6-4D82-AF44-3C16EE9D2CDA' -- Communication Settings                                                                                                                                                                                           
Update [Page] set [Order] = 5 where [Guid] = '7F1F4130-CB98-473B-9DE1-7A886D2283ED' -- Power Tools
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock( "77D37F89-F305-4E0E-950C-AA1F0F926580" ); // Page Xslt Transformation
            DeleteBlock( "282B34B6-354F-41F3-97A2-16DEC1B657E0" ); // Check-in Schedule Builder

            DeleteBlockType( "8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4" ); // Administration - Check-in Schedule Builder

            DeletePage( "F9B48E2A-7D49-45B6-AA88-D731AD887B0F" ); // Schedule Builder
            DeletePage( "FB0A7D8A-F9F4-4081-B15B-7970D20698E3" ); // Check-in
        }
    }
}
