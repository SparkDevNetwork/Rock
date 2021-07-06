// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System.Collections.Generic;
    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class GivingAnalyticsUpdates2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AttributesUp();
            DatabaseUp();
            CmsUp();

            RockMigrationHelper.UpdateSystemCommunication( "Finance", "Financial Transaction Alert Summary", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                "Giving Alert for {{ 'Global' | Attribute:'OrganizationName'}}",
                "{{ 'Global' | Attribute:'EmailHeader' }} <p> A giving alert has been created for {{ Person.FullName }}. Click <a href=\"{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ RelativeAlertLink }}\">here</a> to view the alert. </p> {{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemCommunication.FINANCIAL_TRANSACTION_ALERT_NOTIFICATION_SUMMARY );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.FINANCIAL_TRANSACTION_ALERT_NOTIFICATION_SUMMARY );

            CmsDown();
            DatabaseDown();
        }

        /// <summary>
        /// Databases up.
        /// </summary>
        private void DatabaseUp()
        {
            AddColumn( "dbo.FinancialTransactionAlertType", "RunDays", c => c.Int() );
            AddColumn( "dbo.FinancialTransactionAlertType", "AlertSummaryNotificationGroupId", c => c.Int() );
            CreateIndex( "dbo.FinancialTransactionAlertType", "AlertSummaryNotificationGroupId" );
            AddForeignKey( "dbo.FinancialTransactionAlertType", "AlertSummaryNotificationGroupId", "dbo.Group", "Id" );
        }

        /// <summary>
        /// Databases down.
        /// </summary>
        private void DatabaseDown()
        {
            DropForeignKey( "dbo.FinancialTransactionAlertType", "AlertSummaryNotificationGroupId", "dbo.Group" );
            DropIndex( "dbo.FinancialTransactionAlertType", new[] { "AlertSummaryNotificationGroupId" } );
            DropColumn( "dbo.FinancialTransactionAlertType", "AlertSummaryNotificationGroupId" );
            DropColumn( "dbo.FinancialTransactionAlertType", "RunDays" );
        }

        /// <summary>
        /// CMS up.
        /// </summary>
        private void CmsUp()
        {
            // Add Page Giving Alerts to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "18C9E5C3-3E28-4AA3-84F6-78CD4EA2DD3C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Giving Alerts", "", SystemGuid.Page.GIVING_ALERTS, "" );
            // Add Block Giving Analytics Alerts to Page: Giving Alerts, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.GIVING_ALERTS.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "0A813EC3-EC36-499B-9EBD-C3388DC7F49D".AsGuid(), "Giving Analytics Alerts", "Main", @"", @"", 0, "DB2CE9D5-B6BE-42C1-ACC0-3EBE03CD6208" );

            // Hide the page for now
            Sql( $"UPDATE [Page] SET [DisplayInNavWhen] = {( int ) DisplayInNavWhen.Never} WHERE [Guid] = '{SystemGuid.Page.GIVING_ALERTS}';" );
        }

        /// <summary>
        /// CMS down.
        /// </summary>
        private void CmsDown()
        {
            // Remove Block: Giving Analytics Alerts, from Page: Giving Alerts, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "DB2CE9D5-B6BE-42C1-ACC0-3EBE03CD6208" );
            // Delete Page Giving Alerts from Site:Rock RMS
            RockMigrationHelper.DeletePage( SystemGuid.Page.GIVING_ALERTS ); //  Page: Giving Alerts, Layout: Full Width, Site: Rock RMS
        }

        /// <summary>
        /// Attributes up.
        /// </summary>
        private void AttributesUp()
        {
            var givingAnalyticsCategory = new List<string>() { SystemGuid.Category.PERSON_ATTRIBUTES_GIVING_ANALYTICS };

            // Person Attribute "Giving History JSON"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid(
                SystemGuid.FieldType.CODE_EDITOR,
                givingAnalyticsCategory,
                "Giving History JSON",
                "",
                "GivingHistoryJson",
                "",
                "",
                2000,
                "",
                SystemGuid.Attribute.PERSON_GIVING_HISTORY_JSON );

            // Person Attribute "Last 12 Month Giving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid(
                SystemGuid.FieldType.CURRENCY,
                givingAnalyticsCategory,
                "Last 12 Months Giving",
                "",
                "LastTwelveMonthsGiving",
                "",
                "",
                2001,
                "",
                SystemGuid.Attribute.PERSON_GIVING_12_MONTHS );

            // Person Attribute "Last 90 Days Giving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid(
                SystemGuid.FieldType.CURRENCY,
                givingAnalyticsCategory,
                "Last 90 Days Giving",
                "",
                "LastNinetyDaysGiving",
                "",
                "",
                2002,
                "",
                SystemGuid.Attribute.PERSON_GIVING_90_DAYS );

            // Person Attribute "Prior 90 Days Giving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid(
                SystemGuid.FieldType.CURRENCY,
                givingAnalyticsCategory,
                "Prior 90 Days Giving",
                "",
                "PriorNinetyDaysGiving",
                "",
                "",
                2003,
                "",
                SystemGuid.Attribute.PERSON_GIVING_PRIOR_90_DAYS );

            // Person Attribute "Last 12 Month Gift Count"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid(
                SystemGuid.FieldType.INTEGER,
                givingAnalyticsCategory,
                "Last 12 Months Gift Count",
                "",
                "LastTwelveMonthsGiftCount",
                "",
                "",
                2004,
                "",
                SystemGuid.Attribute.PERSON_GIVING_12_MONTHS_COUNT );

            // Person Attribute "Last 90 Day Gift Count"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid(
                SystemGuid.FieldType.INTEGER,
                givingAnalyticsCategory,
                "Last 90 Days Gift Count",
                "",
                "LastNinetyDaysGiftCount",
                "",
                "",
                2005,
                "",
                SystemGuid.Attribute.PERSON_GIVING_90_DAYS_COUNT );
        }
    }
}
