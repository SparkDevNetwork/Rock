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
    public partial class RenameBlockProperties : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                update Attribute set [Key] = 'EventPage'            where [key] = 'EventPageGuid'
                update Attribute set [Key] = 'Domain'               where [key] = 'EmailServer'
                update Attribute set [Key] = 'Port'                 where [key] = 'EmailServerPort'
                update Attribute set [Key] = 'EZ-LocateService'     where [key] = 'EZLocateService'
                update Attribute set [Key] = 'AllowSettingofValues' where [key] = 'SetValues'
                update Attribute set [Key] = 'EnableFacebookLogin'  where [key] = 'FacebookEnabled'
                update Attribute set [Key] = 'CheckforDuplicates'   where [key] = 'Duplicates'
");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                update Attribute set [Key] = 'EventPageGuid'   where [key] = 'EventPage'            
                update Attribute set [Key] = 'EmailServer'     where [key] = 'Domain'               
                update Attribute set [Key] = 'EmailServerPort' where [key] = 'Port'                 
                update Attribute set [Key] = 'EZLocateService' where [key] = 'EZ-LocateService'     
                update Attribute set [Key] = 'SetValues'       where [key] = 'AllowSettingofValues' 
                update Attribute set [Key] = 'FacebookEnabled' where [key] = 'EnableFacebookLogin'  
                update Attribute set [Key] = 'Duplicates'      where [key] = 'CheckforDuplicates'   
");
        }
    }
}
