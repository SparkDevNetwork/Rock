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
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class GivingAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AttributesUp();
            UpdatePreferredCurrencyAndTransactionSourceUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdatePreferredCurrencyAndTransactionSourceDown();
            AttributesDown();
        }

        /// <summary>
        /// Attributeses up.
        /// </summary>
        private void AttributesUp()
        {
            var givingAnalyticsCategory = new List<string>() { SystemGuid.Category.PERSON_ATTRIBUTES_GIVING_ANALYTICS };
            var givingAnalyticsAndEraCategory = new List<string>() { SystemGuid.Category.PERSON_ATTRIBUTES_GIVING_ANALYTICS, SystemGuid.Category.PERSON_ATTRIBUTES_ERA };

            RockMigrationHelper.UpdatePersonAttributeCategory( "Giving Analytics", "fas fa-hand-holding-usd", "Attributes that describe the most recent classification of this person's giving habits", SystemGuid.Category.PERSON_ATTRIBUTES_GIVING_ANALYTICS );

            // Person Attribute "Last Gave"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"6B6AA175-4758-453F-8D83-FCD8044B5F36", givingAnalyticsAndEraCategory, @"Last Gave", @"", @"core_EraLastGave", @"", @"", 6, @"", @"02F64263-E290-399E-4487-FC236F4DE81F" );

            // Person Attribute "First Gave"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"6B6AA175-4758-453F-8D83-FCD8044B5F36", givingAnalyticsAndEraCategory, @"First Gave", @"", @"core_EraFirstGave", @"", @"", 6, @"", @"EE5EC76A-D4B9-56B5-4B48-29627D945F10" );

            // Person Attribute "Preferred Currency"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"59D5A94C-94A0-4630-B80A-BB25697D74C7", givingAnalyticsCategory, @"Preferred Currency", @"Preferred Currency", @"PreferredCurrency", @"", @"The most used means of giving that this person employed in the past 12 months.", 1041, @"", SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY, @"definedtype", @"10", @"73E158CE-81E8-44DB-B3DC-F8AA2CF1B4B1" );

            // Person Attribute "Preferred Source"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"59D5A94C-94A0-4630-B80A-BB25697D74C7", givingAnalyticsCategory, @"Preferred Source", @"Preferred Source", @"PreferredSource", @"", @"The most used giving source (kiosk, app, web) that this person employed in the past 12 months.", 1042, @"", SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE, @"definedtype", @"12", @"B51B7E3B-5D14-41E1-BA5C-9DEBDA2528A7" );

            // Person Attribute "Frequency Label"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", givingAnalyticsCategory, @"Frequency Label", @"Frequency Label", @"FrequencyLabel", @"", @"The frequency that this person typically has given in the past 12 months.", 1043, @"", SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, @"fieldtype", @"ddl", @"73637E4E-F872-4066-8E04-B4A8429F6FD6" );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL, @"values", @"1^Weekly, 2^Bi-Weekly, 3^Monthly, 4^Quarterly, 5^Erratic, 6^Undetermined", @"DEDE252F-E8FF-4858-A616-BBE6A6FB95FF" );

            // Person Attribute "Percent of Gifts Scheduled"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", givingAnalyticsCategory, @"Percent of Gifts Scheduled", @"Percent of Gifts Scheduled", @"PercentofGiftsScheduled", @"", @"The percent of gifts in the past 12 months that have been part of a scheduled transaction. Note that this is stored as an integer. Ex: 15% is stored as 15.", 1044, @"", SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED );

            // Person Attribute "Gift Amount: IQR"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"3EE69CBC-35CE-4496-88CC-8327A447603F", givingAnalyticsCategory, @"Gift Amount: IQR", @"Gift Amount: IQR", @"GiftAmountIQR", @"", @"The gift amount interquartile range calculated from the past 12 months of giving.", 1046, @"", SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR );

            // Person Attribute "Gift Amount: Median"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"3EE69CBC-35CE-4496-88CC-8327A447603F", givingAnalyticsCategory, @"Gift Amount: Median", @"Gift Amount: Median", @"GiftAmountMedian", @"", @"The median gift amount given in the past 12 months.", 1047, @"", SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN );

            // Person Attribute "Gift Frequency Days: Mean"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"C757A554-3009-4214-B05D-CEA2B2EA6B8F", givingAnalyticsCategory, @"Gift Frequency Days: Mean", @"Gift Frequency Days: Mean", @"GiftFrequencyDaysMean", @"", @"The mean days between gifts given in the past 12 months.", 1048, @"", SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS );

            // Person Attribute "Gift Frequency Days: Standard Deviation"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"C757A554-3009-4214-B05D-CEA2B2EA6B8F", givingAnalyticsCategory, @"Gift Frequency Days: Standard Deviation", @"Gift Frequency Days: Standard Deviation", @"GiftFrequencyDaysStandardDeviation", @"", @"The standard deviation for the number of days between gifts given in the past 12 months.", 1049, @"", SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS );

            // Person Attribute "Giving Bin"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", givingAnalyticsCategory, @"Giving Bin", @"Giving Bin", @"GivingBin", @"", @"The bin that this person's giving habits fall within.", 1050, @"", SystemGuid.Attribute.PERSON_GIVING_BIN );

            // Person Attribute "Giving Percentile"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", givingAnalyticsCategory, @"Giving Percentile", @"Giving Percentile", @"GivingPercentile", @"", @"Within the context of all givers over the past twelve months, this is the percentile for this person.  Note that this is stored as an integer. Ex: 15% is stored as 15.", 1051, @"", SystemGuid.Attribute.PERSON_GIVING_PERCENTILE );

            // Person Attribute "Next Expected Gift Date"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"6B6AA175-4758-453F-8D83-FCD8044B5F36", givingAnalyticsCategory, @"Next Expected Gift Date", @"Next Expected Gift Date", @"NextExpectedGiftDate", @"", @"The date, based on giving habits, that this person is next anticipated to give.", 1052, @"", SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE, @"datePickerControlType", @"Date Picker", @"65E9B929-9A43-4CE6-AD8F-7055BEE35CF4" );

            // Person Attribute "Last Classification Run Date Time"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"6B6AA175-4758-453F-8D83-FCD8044B5F36", givingAnalyticsCategory, @"Last Classification Run Date Time", @"Last Classification Run Date Time", @"LastClassificationRunDateTime", @"", @"The date that this person's giving analytics were last classified.", 1053, @"", SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE );
            RockMigrationHelper.AddAttributeQualifier( SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE, @"datePickerControlType", @"Date Picker", @"8819FE04-D1A6-45DE-9BD4-7C1AD9AD9F63" );

            // Security on all new attributes: admins, financial roles, and deny everyone else
            var guidStrings = new string[] {
                SystemGuid.Attribute.PERSON_GIVING_PREFERRED_CURRENCY,
                SystemGuid.Attribute.PERSON_GIVING_PREFERRED_SOURCE,
                SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_LABEL,
                SystemGuid.Attribute.PERSON_GIVING_PERCENT_SCHEDULED,
                SystemGuid.Attribute.PERSON_GIVING_AMOUNT_MEDIAN,
                SystemGuid.Attribute.PERSON_GIVING_AMOUNT_IQR,
                SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_MEAN_DAYS,
                SystemGuid.Attribute.PERSON_GIVING_FREQUENCY_STD_DEV_DAYS,
                SystemGuid.Attribute.PERSON_GIVING_BIN,
                SystemGuid.Attribute.PERSON_GIVING_PERCENTILE,
                SystemGuid.Attribute.PERSON_GIVING_NEXT_EXPECTED_GIFT_DATE,
                SystemGuid.Attribute.PERSON_GIVING_LAST_CLASSIFICATION_DATE
            };

            foreach ( var guidString in guidStrings )
            {
                RockMigrationHelper.AddSecurityAuthForAttribute( guidString, 0, Security.Authorization.VIEW, true, SystemGuid.Group.GROUP_ADMINISTRATORS, 0, Guid.NewGuid().ToString() );
                RockMigrationHelper.AddSecurityAuthForAttribute( guidString, 1, Security.Authorization.VIEW, true, SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS, 0, Guid.NewGuid().ToString() );
                RockMigrationHelper.AddSecurityAuthForAttribute( guidString, 2, Security.Authorization.VIEW, true, SystemGuid.Group.GROUP_FINANCE_USERS, 0, Guid.NewGuid().ToString() );
                RockMigrationHelper.AddSecurityAuthForAttribute( guidString, 3, Security.Authorization.VIEW, false, null, 1, Guid.NewGuid().ToString() );
            }
        }

        /// <summary>
        /// Attributeses down.
        /// </summary>
        private void AttributesDown()
        {
        }

        /// <summary>
        /// Updates the preferred currency and transaction source up.
        /// </summary>
        private void UpdatePreferredCurrencyAndTransactionSourceUp()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "4F02B41E-AB7D-4345-8A97-3904DDD89B01", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Icon CSS Class", "IconCssClass", "", 1055, "fa-hand-holding-usd", "CB1E9401-E1FD-4DBB-B15F-4E6994602723" );
            RockMigrationHelper.AddAttributeQualifier( "CB1E9401-E1FD-4DBB-B15F-4E6994602723", "ispassword", "False", "746974E9-0A05-4DD4-8277-922C442F76F0" );
            RockMigrationHelper.AddAttributeQualifier( "CB1E9401-E1FD-4DBB-B15F-4E6994602723", "maxcharacters", "", "1ECBE523-F5EF-4362-A09A-FD9BC9436D7F" );
            RockMigrationHelper.AddAttributeQualifier( "CB1E9401-E1FD-4DBB-B15F-4E6994602723", "showcountdown", "False", "84CE2ECD-B7DE-4F10-B69A-57285AC950E3" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "260EEA80-821A-4F79-973F-49DF79C955F7", "CB1E9401-E1FD-4DBB-B15F-4E6994602723", @"fa-cash-register" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "61E46A46-7399-4817-A6EC-3D8495E2316E", "CB1E9401-E1FD-4DBB-B15F-4E6994602723", @"fa-money-check" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7D705CE7-7B11-4342-A58E-53617C5B4E69", "CB1E9401-E1FD-4DBB-B15F-4E6994602723", @"fa-desktop" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8ADCEC72-63FC-4F08-A4CC-72BCE470172C", "CB1E9401-E1FD-4DBB-B15F-4E6994602723", @"fa-mobile-alt" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8BA95E24-D291-499E-A535-4DCAC365689B", "CB1E9401-E1FD-4DBB-B15F-4E6994602723", @"fa-sms" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BE7ECF50-52BC-4774-808D-574BA842DB98", "CB1E9401-E1FD-4DBB-B15F-4E6994602723", @"fa-church" );

            RockMigrationHelper.AddDefinedTypeAttribute( "1D1304DE-E83A-44AF-B11D-0C66DD600B81", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Icon CSS Class", "IconCssClass", "", 1054, "fa-credit-card", "9617D1DC-6561-4314-83EB-7F0ACBA2E259" );
            RockMigrationHelper.AddAttributeQualifier( "9617D1DC-6561-4314-83EB-7F0ACBA2E259", "ispassword", "False", "7A492FD1-60F2-4055-BE72-6A367E31022C" );
            RockMigrationHelper.AddAttributeQualifier( "9617D1DC-6561-4314-83EB-7F0ACBA2E259", "maxcharacters", "", "AC52CCC9-FE9C-4481-A21C-B9CA0C1ADE29" );
            RockMigrationHelper.AddAttributeQualifier( "9617D1DC-6561-4314-83EB-7F0ACBA2E259", "showcountdown", "False", "E3188133-82B5-4B21-9286-D850CCC3A77E" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0FDF0BB3-B483-4C0A-9DFF-A35ABE3B688D", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-dollar-sign" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "56C9AE9C-B5EB-46D5-9650-2EF86B14F856", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-question" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6151F6E0-3223-46BA-A59E-E091BE4AF75C", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-google-pay" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7950FF66-80EE-E8AB-4A77-4A13EDEB7513", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-file-contract" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8B086A19-405A-451F-8D44-174E92D6B402", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-money-check" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "928A2E04-C77B-4282-888F-EC549CEE026A", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-credit-card" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D42C4DF7-1AE9-4DDE-ADA2-774B866B798C", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-apple-pay" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-university" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93", "9617D1DC-6561-4314-83EB-7F0ACBA2E259", @"fa-money-bill" );
        }

        /// <summary>
        /// Updates the preferred currency and transaction source down.
        /// </summary>
        private void UpdatePreferredCurrencyAndTransactionSourceDown()
        {
            RockMigrationHelper.DeleteAttribute( "CB1E9401-E1FD-4DBB-B15F-4E6994602723" ); // IconCssClass
            RockMigrationHelper.DeleteAttribute( "9617D1DC-6561-4314-83EB-7F0ACBA2E259" ); // IconCssClass
        }
    }
}
