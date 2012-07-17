using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Rock.Migrations
{
    public partial class CreateDatabase : DbMigration
    {
        /// <summary>
        /// Creates additional indexes.  A call to this method should be added at the end of the Up() 
        /// method in the initial migration
        /// </summary>
        public void CreateIndexes()
        {
            CreateIndex( "cmsAuth", new string[] { "EntityType", "EntityId" } );
            CreateIndex( "cmsBlock", "Name" );
            CreateIndex( "cmsBlock", "Path" );
            CreateIndex( "cmsBlockInstance", "Layout" );
            CreateIndex( "cmsBlockInstance", new string[] { "Layout", "PageId", "Zone" } );
            CreateIndex( "cmsHtmlContent", new string[] { "BlockId", "EntityValue", "Version" }, true );
            CreateIndex( "cmsSiteDomain", "Domain", true );
            CreateIndex( "cmsSiteDomain", new string[] { "SiteId", "Domain" } );
            CreateIndex( "cmsUser", "UserName", true );
            CreateIndex( "cmsUser", "ApiKey" );
            CreateIndex( "coreAttribute", "Entity" );
            CreateIndex( "coreAttribute", new string[] { "Entity", "EntityQualifierColumn", "EntityQualifierValue", "Key" }, true );
            CreateIndex( "coreAttributeValue", new string[] { "AttributeId" , "EntityId" } );
            CreateIndex( "coreAttributeValue", "EntityId" );
            CreateIndex( "coreDefinedType", "Guid", true );
            CreateIndex( "coreDefinedValue", "Guid", true );
            CreateIndex( "coreEntityChange", "ChangeSet" );
            CreateIndex( "coreExceptionLog", "ParentId" );
            CreateIndex( "coreExceptionLog", "SiteId" );
            CreateIndex( "coreFieldType", "Name" );
            CreateIndex( "crmAddress", new string[] { "Street1", "Street2", "City", "State", "Zip" }, true );
            CreateIndex( "crmAddress", "Raw", true );
            CreateIndex( "crmEmailTemplate", "Guid", true );
            CreateIndex( "crmPerson", "Email" );
            CreateIndex( "crmPersonTrail", "CurrentId" );
            CreateIndex( "groupsGroup", "Guid", true );
            CreateIndex( "groupsGroup", "IsSecurityRole" );
            CreateIndex( "groupsGroup", new string[] { "ParentGroupId", "Name" } );
            CreateIndex( "groupsGroupRole", "Order" );
            CreateIndex( "groupsMember", new string[] { "GroupId", "PersonId", "GroupRoleId" }, true );
        }

        /// <summary>
        /// Drops additional indexes.  A call to this method should be added at the beginning of the Down()
        /// method in the initial migration
        /// </summary>
        public void DropIndexes()
        {
            DropIndex( "groupsMember", new[] { "GroupId", "PersonId", "GroupRoleId" } );
            DropIndex( "groupsGroupRole", new[] { "Order" } );
            DropIndex( "groupsGroup", new[] { "ParentGroupId", "Name" } );
            DropIndex( "groupsGroup", new[] { "IsSecurityRole" } );
            DropIndex( "groupsGroup", new[] { "Guid" } );
            DropIndex( "crmPersonTrail", new[] { "CurrentId" } );
            DropIndex( "crmPerson", new[] { "Email" } );
            DropIndex( "crmEmailTemplate", new[] { "Guid" } );
            DropIndex( "crmAddress", new[] { "Raw" } );
            DropIndex( "crmAddress", new[] { "Street1", "Street2", "City", "State", "Zip" } );
            DropIndex( "coreFieldType", new[] { "Name" } );
            DropIndex( "coreExceptionLog", new[] { "SiteId" } );
            DropIndex( "coreExceptionLog", new[] { "ParentId" } );
            DropIndex( "coreEntityChange", new[] { "ChangeSet" } );
            DropIndex( "coreDefinedValue", new[] { "Guid" } );
            DropIndex( "coreDefinedType", new[] { "Guid" } );
            DropIndex( "coreAttributeValue", new[] { "EntityId" } );
            DropIndex( "coreAttributeValue", new[] { "AttributeId", "EntityId" } );
            DropIndex( "coreAttribute", new[] { "Entity", "EntityQualifierColumn", "EntityQualifierValue", "Key" } );
            DropIndex( "coreAttribute", new[] { "Entity" } );
            DropIndex( "cmsUser", new[] { "ApiKey" } );
            DropIndex( "cmsUser", new[] { "UserName" } );
            DropIndex( "cmsSiteDomain", new[] { "SiteId", "Domain" } );
            DropIndex( "cmsSiteDomain", new[] { "Domain" } );
            DropIndex( "cmsHtmlContent", new[] { "BlockId", "EntityValue", "Version" } );
            DropIndex( "cmsBlockInstance", new[] { "Layout", "PageId", "Zone" } );
            DropIndex( "cmsBlockInstance", new[] { "Layout" } );
            DropIndex( "cmsBlock", new[] { "Path" } );
            DropIndex( "cmsBlock", new[] { "Name" } );
            DropIndex( "cmsAuth", new[] { "EntityType", "EntityId" } );
        }
    }
}