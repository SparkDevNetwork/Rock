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
    public partial class FixInactiveSecurityGroups : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
update [Group] set [IsActive] = 1 where [Guid] in (
'628C51A8-4613-43ED-A18D-4A6FB999273E',
'2C112948-FF4C-46E7-981A-0257681EADF4',
'1918E74F-C00D-4DDD-94C4-2E7209CE12C3',
'CDF68207-2795-42DE-B060-FE01C33BEAEA',
'6246A7EF-B7A3-4C8C-B1E4-3FF114B84559',
'2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9')
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
update [Group] set [IsActive] = 0 where [Guid] in (
'628C51A8-4613-43ED-A18D-4A6FB999273E',
'2C112948-FF4C-46E7-981A-0257681EADF4',
'1918E74F-C00D-4DDD-94C4-2E7209CE12C3',
'CDF68207-2795-42DE-B060-FE01C33BEAEA',
'6246A7EF-B7A3-4C8C-B1E4-3FF114B84559',
'2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9')
" );
        }
    }
}
