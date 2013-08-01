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
    public partial class PageSettingUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddBlock("03C49950-9C4C-4668-9C65-9A0DF43D1B33", "19B61D65-37E3-459F-A44F-DEF0089118A3", "Warning Message", "", "Content", 0, "6EC0CC5D-C727-4687-A86D-317CEE8C22DB");
            Sql(@"
                Update [Page] set [IconCssClass] = 'icon-warning-sign' where [Guid] = '03C49950-9C4C-4668-9C65-9A0DF43D1B33' --SQL Command
                Update [Page] set [IconCssClass] = 'icon-list-ul', name = 'Person Attributes', Title = 'Person Attributes' where [Guid] = '7BA1FAF4-B63C-4423-A818-CC794DDB14E3' --Person Properties / Attributes
                Update [Page] set [IconCssClass] = 'icon-user' where [Guid] = 'BBD61BB9-7BE0-4F16-9615-91D38F3AE9C9' --Group Roles
                Update [Page] set [IconCssClass] = 'icon-list-ul' where [Guid] = 'A2753E03-96B1-4C83-AA11-FCD68C631571' --Global attributes
                Update [Page] set [IconCssClass] = 'icon-lock' where [Guid] = 'CE2170A9-2C8E-40B1-A42E-DFA73762D01D' --Authenication services
                Update [Page] set [IconCssClass] = 'icon-bug' where [Guid] = '21DA6141-0A03-4F00-B0A8-3B110FBE2438' --Exception List
                Update [Page] set [IconCssClass] = 'icon-bug' where [Guid] = 'F1F58172-E03E-4299-910A-ED34F857DAFB' --Exception Details
                Update [Page] set [IconCssClass] = 'icon-list-ul' where [Guid] = '23507C90-3F78-40D4-B847-6FE8941FCD32' --Entity attributes

                
                Update [Page] set [ParentPageId] = (Select id from page where guid = '7F1F4130-CB98-473B-9DE1-7A886D2283ED') where [Guid] = 'F7F41856-F7EA-49A8-9D9B-917AC1964602' --Move entity admin to power tools

                Update [Page] set [ParentPageId] = (Select id from page where guid = '4E237286-B715-4109-A578-C1445EC02707') where [Guid] = '54F6365A-4E4C-4E2A-9498-70B09E062C69' --Move group details under GroupViewer
                Delete from [Page] where [Guid] = '4D7624FB-A9AE-40BD-82CB-84C22F64343E' --Group List

                insert into HtmlContent (BlockId, Version, Content, IsApproved, ApprovedByPersonId, ApprovedDateTime, Guid)
                    (Select Id, 0,  '<div class=""alert alert-danger"">
                        <strong>Warning!</strong>

                        <p>Running SQL commands directly against the database while powerful, can be extremely dangerous. The difference is all in your hands.
<p/>

<p>If you are unsure of the SQL you are about to run <strong>DO NOT</strong> proceed.</p>

</div>', 'True', 1, getdate(), NEWID()
From Block where Guid = '6EC0CC5D-C727-4687-A86D-317CEE8C22DB')
            ");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlock("6EC0CC5D-C727-4687-A86D-317CEE8C22DB");
        }
    }
}
