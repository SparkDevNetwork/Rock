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
    public partial class DateTimeOffset : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AlterColumn("dbo.crmPerson", "AnniversaryDate", c => c.DateTimeOffset());
            AlterColumn("dbo.crmPerson", "GraduationDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsUser", "LastActivityDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsUser", "LastLoginDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsUser", "LastPasswordChangedDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsUser", "CreationDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsUser", "LastLockedOutDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsUser", "FailedPasswordAttemptWindowStart", c => c.DateTimeOffset());
            AlterColumn("dbo.financialPledge", "StartDate", c => c.DateTimeOffset(nullable: false));
            AlterColumn("dbo.financialPledge", "EndDate", c => c.DateTimeOffset(nullable: false));
            AlterColumn("dbo.financialFund", "StartDate", c => c.DateTimeOffset());
            AlterColumn("dbo.financialFund", "EndDate", c => c.DateTimeOffset());
            AlterColumn("dbo.financialTransaction", "TransactionDate", c => c.DateTimeOffset());
            AlterColumn("dbo.financialBatch", "BatchDate", c => c.DateTimeOffset());
            AlterColumn("dbo.crmLocation", "StandardizeAttempt", c => c.DateTimeOffset());
            AlterColumn("dbo.crmLocation", "StandardizeDate", c => c.DateTimeOffset());
            AlterColumn("dbo.crmLocation", "GeocodeAttempt", c => c.DateTimeOffset());
            AlterColumn("dbo.crmLocation", "GeocodeDate", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsHtmlContent", "ApprovedDateTime", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsHtmlContent", "StartDateTime", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsHtmlContent", "ExpireDateTime", c => c.DateTimeOffset());
            AlterColumn("dbo.cmsPageContext", "CreatedDateTime", c => c.DateTimeOffset());
            AlterColumn("dbo.coreEntityChange", "CreatedDateTime", c => c.DateTimeOffset());
            AlterColumn("dbo.coreExceptionLog", "ExceptionDate", c => c.DateTimeOffset(nullable: false));
            AlterColumn("dbo.coreMetric", "LastCollected", c => c.DateTimeOffset());
            AlterColumn("dbo.coreServiceLog", "Time", c => c.DateTimeOffset());
            AlterColumn("dbo.crmPersonViewed", "ViewDateTime", c => c.DateTimeOffset());
            AlterColumn("dbo.utilJob", "LastSuccessfulRun", c => c.DateTimeOffset());
            AlterColumn("dbo.utilJob", "LastRunDate", c => c.DateTimeOffset());
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AlterColumn("dbo.utilJob", "LastRunDate", c => c.DateTime());
            AlterColumn("dbo.utilJob", "LastSuccessfulRun", c => c.DateTime());
            AlterColumn("dbo.crmPersonViewed", "ViewDateTime", c => c.DateTime());
            AlterColumn("dbo.coreServiceLog", "Time", c => c.DateTime());
            AlterColumn("dbo.coreMetric", "LastCollected", c => c.DateTime());
            AlterColumn("dbo.coreExceptionLog", "ExceptionDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.coreEntityChange", "CreatedDateTime", c => c.DateTime());
            AlterColumn("dbo.cmsPageContext", "CreatedDateTime", c => c.DateTime());
            AlterColumn("dbo.cmsHtmlContent", "ExpireDateTime", c => c.DateTime());
            AlterColumn("dbo.cmsHtmlContent", "StartDateTime", c => c.DateTime());
            AlterColumn("dbo.cmsHtmlContent", "ApprovedDateTime", c => c.DateTime());
            AlterColumn("dbo.crmLocation", "GeocodeDate", c => c.DateTime());
            AlterColumn("dbo.crmLocation", "GeocodeAttempt", c => c.DateTime());
            AlterColumn("dbo.crmLocation", "StandardizeDate", c => c.DateTime());
            AlterColumn("dbo.crmLocation", "StandardizeAttempt", c => c.DateTime());
            AlterColumn("dbo.financialBatch", "BatchDate", c => c.DateTime());
            AlterColumn("dbo.financialTransaction", "TransactionDate", c => c.DateTime());
            AlterColumn("dbo.financialFund", "EndDate", c => c.DateTime());
            AlterColumn("dbo.financialFund", "StartDate", c => c.DateTime());
            AlterColumn("dbo.financialPledge", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.financialPledge", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.cmsUser", "FailedPasswordAttemptWindowStart", c => c.DateTime());
            AlterColumn("dbo.cmsUser", "LastLockedOutDate", c => c.DateTime());
            AlterColumn("dbo.cmsUser", "CreationDate", c => c.DateTime());
            AlterColumn("dbo.cmsUser", "LastPasswordChangedDate", c => c.DateTime());
            AlterColumn("dbo.cmsUser", "LastLoginDate", c => c.DateTime());
            AlterColumn("dbo.cmsUser", "LastActivityDate", c => c.DateTime());
            AlterColumn("dbo.crmPerson", "GraduationDate", c => c.DateTime());
            AlterColumn("dbo.crmPerson", "AnniversaryDate", c => c.DateTime());
        }
    }
}
