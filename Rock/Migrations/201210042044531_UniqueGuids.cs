//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public partial class UniqueGuids : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex( "cmsAuth", "Guid", true );
            CreateIndex( "cmsBlock", "Guid", true );
            CreateIndex( "cmsBlockType", "Guid", true );
            CreateIndex( "cmsFile", "Guid", true );
            CreateIndex( "cmsHtmlContent", "Guid", true );
            CreateIndex( "cmsPage", "Guid", true );
            CreateIndex( "cmsPageContext", "Guid", true );
            // CreateIndex( "cmsPageRoute", "Guid", true ); already done in prior migration
            CreateIndex( "cmsSite", "Guid", true );
            CreateIndex( "cmsSiteDomain", "Guid", true );
            CreateIndex( "cmsUser", "Guid", true );
            CreateIndex( "coreAttribute", "Guid", true );
            CreateIndex( "coreAttributeQualifier", "Guid", true );
            CreateIndex( "coreAttributeValue", "Guid", true );
            //CreateIndex( "coreDefinedType", "Guid", true );
            //CreateIndex( "coreDefinedValue", "Guid", true );
            CreateIndex( "coreEntityChange", "Guid", true );
            CreateIndex( "coreExceptionLog", "Guid", true );
            CreateIndex( "coreFieldType", "Guid", true );
            CreateIndex( "coreMetric", "Guid", true );
            CreateIndex( "coreMetricValue", "Guid", true );
            CreateIndex( "coreServiceLog", "Guid", true );
            CreateIndex( "coreTag", "Guid", true );
            CreateIndex( "coreTaggedItem", "Guid", true );
            CreateIndex( "crmCampus", "Guid", true );
            //CreateIndex( "crmEmailTemplate", "Guid", true );
            CreateIndex( "crmLocation", "Guid", true );
            CreateIndex( "crmPerson", "Guid", true );
            CreateIndex( "crmPersonMerged", "Guid", true );
            CreateIndex( "crmPersonViewed", "Guid", true );
            CreateIndex( "crmPhoneNumber", "Guid", true );
            CreateIndex( "financialBatch", "Guid", true );
            CreateIndex( "financialFund", "Guid", true );
            CreateIndex( "financialGateway", "Guid", true );
            CreateIndex( "financialPersonAccountLookup", "Guid", true );
            CreateIndex( "financialPledge", "Guid", true );
            CreateIndex( "financialTransaction", "Guid", true );
            CreateIndex( "financialTransactionDetail", "Guid", true );
            //CreateIndex( "groupsGroup", "Guid", true );
            CreateIndex( "groupsGroupRole", "Guid", true );
            CreateIndex( "groupsGroupType", "Guid", true );
            CreateIndex( "groupsMember", "Guid", true );
            CreateIndex( "utilJob", "Guid", true );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "cmsAuth", new string[] { "Guid" } );
            DropIndex( "cmsBlock", new string[] { "Guid" } );
            DropIndex( "cmsBlockType", new string[] { "Guid" } );
            DropIndex( "cmsFile", new string[] { "Guid" } );
            DropIndex( "cmsHtmlContent", new string[] { "Guid" } );
            DropIndex( "cmsPage", new string[] { "Guid" } );
            DropIndex( "cmsPageContext", new string[] { "Guid" } );
            // DropIndex( "cmsPageRoute", new string[] { "Guid" } ); already done in prior migration
            DropIndex( "cmsSite", new string[] { "Guid" } );
            DropIndex( "cmsSiteDomain", new string[] { "Guid" } );
            DropIndex( "cmsUser", new string[] { "Guid" } );
            DropIndex( "coreAttribute", new string[] { "Guid" } );
            DropIndex( "coreAttributeQualifier", new string[] { "Guid" } );
            DropIndex( "coreAttributeValue", new string[] { "Guid" } );
            //DropIndex( "coreDefinedType", new string[] { "Guid" } );
            //DropIndex( "coreDefinedValue", new string[] { "Guid" } );
            DropIndex( "coreEntityChange", new string[] { "Guid" } );
            DropIndex( "coreExceptionLog", new string[] { "Guid" } );
            DropIndex( "coreFieldType", new string[] { "Guid" } );
            DropIndex( "coreMetric", new string[] { "Guid" } );
            DropIndex( "coreMetricValue", new string[] { "Guid" } );
            DropIndex( "coreServiceLog", new string[] { "Guid" } );
            DropIndex( "coreTag", new string[] { "Guid" } );
            DropIndex( "coreTaggedItem", new string[] { "Guid" } );
            DropIndex( "crmCampus", new string[] { "Guid" } );
            //DropIndex( "crmEmailTemplate", new string[] { "Guid" } );
            DropIndex( "crmLocation", new string[] { "Guid" } );
            DropIndex( "crmPerson", new string[] { "Guid" } );
            DropIndex( "crmPersonMerged", new string[] { "Guid" } );
            DropIndex( "crmPersonViewed", new string[] { "Guid" } );
            DropIndex( "crmPhoneNumber", new string[] { "Guid" } );
            DropIndex( "financialBatch", new string[] { "Guid" } );
            DropIndex( "financialFund", new string[] { "Guid" } );
            DropIndex( "financialGateway", new string[] { "Guid" } );
            DropIndex( "financialPersonAccountLookup", new string[] { "Guid" } );
            DropIndex( "financialPledge", new string[] { "Guid" } );
            DropIndex( "financialTransaction", new string[] { "Guid" } );
            DropIndex( "financialTransactionDetail", new string[] { "Guid" } );
            //DropIndex( "groupsGroup", new string[] { "Guid" } );
            DropIndex( "groupsGroupRole", new string[] { "Guid" } );
            DropIndex( "groupsGroupType", new string[] { "Guid" } );
            DropIndex( "groupsMember", new string[] { "Guid" } );
            DropIndex( "utilJob", new string[] { "Guid" } );
        }
    }
}
