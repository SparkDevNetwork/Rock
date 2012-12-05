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
    public partial class RockModel : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            @Sql( @"
delete from EntityType where Name like 'Rock.Group%' and id not in (select EntityTypeId from Attribute)

update EntityType set Name = Replace(Name, 'Rock.Cms', 'Rock.Model')
update EntityType set Name = Replace(Name, 'Rock.Core', 'Rock.Model')
update EntityType set Name = Replace(Name, 'Rock.Crm', 'Rock.Model')
update EntityType set Name = Replace(Name, 'Rock.Financial', 'Rock.Model')
update EntityType set Name = Replace(Name, 'Rock.Groups', 'Rock.Model')
update EntityType set Name = Replace(Name, 'Rock.Util', 'Rock.Model')

update AttributeValue set Value = Replace(Value, 'Rock.Cms', 'Rock.Model')
update AttributeValue set Value = Replace(Value, 'Rock.Core', 'Rock.Model')
update AttributeValue set Value = Replace(Value, 'Rock.Crm', 'Rock.Model')
update AttributeValue set Value = Replace(Value, 'Rock.Financial', 'Rock.Model')
update AttributeValue set Value = Replace(Value, 'Rock.Groups', 'Rock.Model')
update AttributeValue set Value = Replace(Value, 'Rock.Util', 'Rock.Model')

update PageContext set Entity = Replace(Entity, 'Rock.Cms', 'Rock.Model')
update PageContext set Entity = Replace(Entity, 'Rock.Core', 'Rock.Model')
update PageContext set Entity = Replace(Entity, 'Rock.Crm', 'Rock.Model')
update PageContext set Entity = Replace(Entity, 'Rock.Financial', 'Rock.Model')
update PageContext set Entity = Replace(Entity, 'Rock.Groups', 'Rock.Model')
update PageContext set Entity = Replace(Entity, 'Rock.Util', 'Rock.Model')
                " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // There really isn't a foolproof Down() for this one.  You'll probably have to start a fresh database to get back to the previous migration 
        }
    }
}
