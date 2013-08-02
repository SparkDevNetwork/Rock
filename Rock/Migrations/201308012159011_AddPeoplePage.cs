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
    public partial class AddPeoplePage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPage("20F97A93-7949-4C2A-8A5E-C756FE8585CA", "People", "menu holder for functions related to people", "Default", "97ECDC48-6DF6-492E-8C72-161F76AE111B", "icon-user");

            AddPage("97ECDC48-6DF6-492E-8C72-161F76AE111B", "Attendees", "menu holder for functions related to people", "Default", "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "icon-user");
            AddPage("97ECDC48-6DF6-492E-8C72-161F76AE111B", "Tools", "menu holder for functions related to people", "Default", "03761EFD-35C2-4EF3-9567-B934D9395152", "icon-user");

            // move add family
            Sql(@"Update [Page] set [Order] = 1, [ParentPageId] = (Select id from page where guid = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701') where [Guid] = '6A11A13D-05AB-4982-A4C2-67A8B1950C74'");

            // move group list
            Sql(@"Update [Page] set [Order] = 2, [ParentPageId] = (Select id from page where guid = 'B0F4B33D-DD11-4CCC-B79D-9342831B8701') where [Guid] = '4E237286-B715-4109-A578-C1445EC02707'");

            // reorder pages under the home
            Sql(@"Update [Page] set [Order] = 1 where [Guid] = '97ECDC48-6DF6-492E-8C72-161F76AE111B'");
            Sql(@"Update [Page] set [Order] = 2 where [Guid] = '7BEB7569-C485-40A0-A609-B0678F6F7240'");
            Sql(@"Update [Page] set [Order] = 3 where [Guid] = '98163C8B-5C91-4A68-BB79-6AD948A604CE'");
            Sql(@"Update [Page] set [Order] = 4 where [Guid] = '84E12152-E456-478E-AF68-BA640E9CE65B'");
            Sql(@"Update [Page] set [Order] = 5 where [Guid] = 'E7BD353C-91A6-4C15-A6C8-F44D0B16D16E'");

            // hide in breadcrumb
            Sql(@"Update [Page] set [BreadCrumbDisplayName] = 'False' where [Guid] in ('97ECDC48-6DF6-492E-8C72-161F76AE111B', 'B0F4B33D-DD11-4CCC-B79D-9342831B8701', '03761EFD-35C2-4EF3-9567-B934D9395152')");
        
            // add icon to add family
            Sql(@"Update [Page] set [IconCssClass] = 'icon-group' where [Guid] = '6A11A13D-05AB-4982-A4C2-67A8B1950C74'");

            // move accounts page under finance -> administration
            Sql(@"Update [Page] set [Order] = 0, [ParentPageId] = (Select id from page where guid = '18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C') where [Guid] = '2B630A3B-E081-4204-A3E4-17BB3A5F063D'");

            // move financial batches to functions and rename it to batches
            Sql(@"Update [Page] set [Order] = 0, [ParentPageId] = (Select id from page where guid = '142627AE-6590-48E3-BFCA-3669260B8CF2') where [Guid] = 'EF65EFF2-99AC-4081-8E09-32A04518683A'");
            Sql(@"Update [Page] set [Name] = 'Batches', [Title] = 'Batches', [Order] = 4 where [Guid] = 'EF65EFF2-99AC-4081-8E09-32A04518683A'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // null out parent page
            Sql(@"Update [Page] set [ParentPageId] = null where [Guid] = '4E237286-B715-4109-A578-C1445EC02707'");
            Sql(@"Update [Page] set [ParentPageId] = null where [Guid] = '4E237286-B715-4109-A578-C1445EC02707'");

            DeletePage("B0F4B33D-DD11-4CCC-B79D-9342831B8701"); // Atendees
            DeletePage("03761EFD-35C2-4EF3-9567-B934D9395152"); // Tools

            DeletePage("9C9CAD94-095E-4CC9-BC29-24BDE30492B2"); // People
        }
    }
}
