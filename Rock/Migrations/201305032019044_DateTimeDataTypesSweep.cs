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
    public partial class DateTimeDataTypesSweep : RockMigration_5
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ServiceLog", "LogDateTime", c => c.DateTime());
            AlterColumn("dbo.Person", "AnniversaryDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.Person", "GraduationDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.Schedule", "StartTime", c => c.Time(nullable: false));
            AlterColumn("dbo.Schedule", "EndTime", c => c.Time(nullable: false));
            AlterColumn("dbo.Schedule", "CheckInStartTime", c => c.Time());
            AlterColumn("dbo.Schedule", "CheckInEndTime", c => c.Time());
            AlterColumn("dbo.Schedule", "EffectiveStartDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.Schedule", "EffectiveEndDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.FinancialAccount", "StartDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.FinancialAccount", "EndDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.FinancialBatch", "BatchDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.FinancialPledge", "StartDate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.FinancialPledge", "EndDate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.FinancialScheduledTransaction", "StartDate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.FinancialScheduledTransaction", "EndDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.FinancialScheduledTransaction", "CardReminderDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.FinancialScheduledTransaction", "LastRemindedDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.MarketingCampaignAd", "StartDate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.MarketingCampaignAd", "EndDate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.PrayerRequest", "EnteredDate", c => c.DateTime(nullable: false, storeType: "date"));
            AlterColumn("dbo.PrayerRequest", "ExpirationDate", c => c.DateTime(storeType: "date"));
            AlterColumn("dbo.PrayerRequest", "ApprovedOnDate", c => c.DateTime(storeType: "date"));
            Sql( "UPDATE dbo.ServiceLog set [LogDateTime] = [Time]" );
            DropColumn("dbo.ServiceLog", "Time");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddColumn("dbo.ServiceLog", "Time", c => c.DateTime());
            AlterColumn("dbo.PrayerRequest", "ApprovedOnDate", c => c.DateTime());
            AlterColumn("dbo.PrayerRequest", "ExpirationDate", c => c.DateTime());
            AlterColumn("dbo.PrayerRequest", "EnteredDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.MarketingCampaignAd", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.MarketingCampaignAd", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.FinancialScheduledTransaction", "LastRemindedDate", c => c.DateTime());
            AlterColumn("dbo.FinancialScheduledTransaction", "CardReminderDate", c => c.DateTime());
            AlterColumn("dbo.FinancialScheduledTransaction", "EndDate", c => c.DateTime());
            AlterColumn("dbo.FinancialScheduledTransaction", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.FinancialPledge", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.FinancialPledge", "StartDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.FinancialBatch", "BatchDate", c => c.DateTime());
            AlterColumn("dbo.FinancialAccount", "EndDate", c => c.DateTime());
            AlterColumn("dbo.FinancialAccount", "StartDate", c => c.DateTime());
            AlterColumn("dbo.Schedule", "EffectiveEndDate", c => c.DateTimeOffset());
            AlterColumn("dbo.Schedule", "EffectiveStartDate", c => c.DateTimeOffset());
            AlterColumn("dbo.Schedule", "CheckInEndTime", c => c.DateTime());
            AlterColumn("dbo.Schedule", "CheckInStartTime", c => c.DateTime());
            AlterColumn("dbo.Schedule", "EndTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Schedule", "StartTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Person", "GraduationDate", c => c.DateTime());
            AlterColumn("dbo.Person", "AnniversaryDate", c => c.DateTime());
            DropColumn("dbo.ServiceLog", "LogDateTime");
        }
    }
}
