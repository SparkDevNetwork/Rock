namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAcronymCasing : DbMigration
    {
        public override void Up()
        {
            Sql(@"
UPDATE [cmsAuth] SET [EntityType] = REPLACE([EntityType], 'CMS', 'Cms') WHERE [EntityType] LIKE '%CMS%'
UPDATE [cmsAuth] SET [EntityType] = REPLACE([EntityType], 'CRM', 'Crm') WHERE [EntityType] LIKE '%CRM%'
UPDATE [cmsPageContext] SET [Entity] = REPLACE([Entity], 'CMS', 'Cms') WHERE [Entity] LIKE '%CMS%'
UPDATE [cmsPageContext] SET [Entity] = REPLACE([Entity], 'CRM', 'Crm') WHERE [Entity] LIKE '%CRM%'
UPDATE [cmsBlock] SET [Path] = REPLACE([Path], 'CMS', 'Cms') WHERE [Path] LIKE '%CMS%'
UPDATE [cmsBlock] SET [Path] = REPLACE([Path], 'CRM', 'Crm') WHERE [Path] LIKE '%CRM%'
UPDATE [coreAttribute] SET [Entity] = REPLACE([Entity], 'CMS', 'Cms') WHERE [Entity] LIKE '%CMS%'
UPDATE [coreAttribute] SET [Entity] = REPLACE([Entity], 'CRM', 'Crm') WHERE [Entity] LIKE '%CRM%'
UPDATE [coreAttributeValue] SET [Value] = REPLACE([Value], 'CMS', 'Cms') WHERE [Value] LIKE '%CMS%'
UPDATE [coreAttributeValue] SET [Value] = REPLACE([Value], 'CRM', 'Crm') WHERE [Value] LIKE '%CRM%'
");
        }
        
        public override void Down()
        {
            Sql(@"
UPDATE [cmsAuth] SET [EntityType] = REPLACE([EntityType], 'Cms', 'CMS') WHERE [EntityType] LIKE '%Cms%'
UPDATE [cmsAuth] SET [EntityType] = REPLACE([EntityType], 'Crm', 'CRM') WHERE [EntityType] LIKE '%Crm%'
UPDATE [cmsPageContext] SET [Entity] = REPLACE([Entity], 'Cms', 'CMS') WHERE [Entity] LIKE '%Cms%'
UPDATE [cmsPageContext] SET [Entity] = REPLACE([Entity], 'Crm', 'CRM') WHERE [Entity] LIKE '%Crm%'
UPDATE [cmsBlock] SET [Path] = REPLACE([Path], 'Cms', 'CMS') WHERE [Path] LIKE '%Cms%'
UPDATE [cmsBlock] SET [Path] = REPLACE([Path], 'Crm', 'CRM') WHERE [Path] LIKE '%Crm%'
UPDATE [coreAttribute] SET [Entity] = REPLACE([Entity], 'Cms', 'CMS') WHERE [Entity] LIKE '%Cms%'
UPDATE [coreAttribute] SET [Entity] = REPLACE([Entity], 'Crm', 'CRM') WHERE [Entity] LIKE '%Crm%'
UPDATE [coreAttributeValue] SET [Value] = REPLACE([Value], 'Cms', 'CMS') WHERE [Value] LIKE '%Cms%'
UPDATE [coreAttributeValue] SET [Value] = REPLACE([Value], 'Crm', 'CRM') WHERE [Value] LIKE '%Crm%'
");
        }
    }
}
