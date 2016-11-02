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
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Analytics1 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.AnalyticsSourceFinancialTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TransactionKey = c.String(),
                        TransactionDateKey = c.Int(nullable: false),
                        TransactionCode = c.String(maxLength: 50),
                        Summary = c.String(),
                        TransactionTypeValueId = c.Int(nullable: false),
                        SourceTypeValueId = c.Int(nullable: false),
                        IsScheduled = c.Boolean(nullable: false),
                        AuthorizedPersonAliasId = c.Int(),
                        ProcessedByPersonAliasId = c.Int(),
                        ProcessedDateTime = c.DateTime(),
                        GivingGroupId = c.Int(),
                        BatchId = c.Int(nullable: false),
                        FinancialGatewayId = c.Int(),
                        EntityTypeId = c.Int(),
                        EntityId = c.Int(),
                        TransactionId = c.Int(nullable: false),
                        TransactionDetailId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        CurrencyTypeValueId = c.Int(nullable: false),
                        CreditCardTypeValueId = c.Int(nullable: false),
                        DaysSinceLastTransactionOfType = c.Int(),
                        IsFirstTransactionOfType = c.Boolean(nullable: false),
                        AuthorizedFamilyId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ModifiedDateTime = c.DateTime(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.TransactionKey, unique: true)
                .Index(t => t.TransactionDateKey)
                .Index(t => t.TransactionTypeValueId)
                .Index(t => t.SourceTypeValueId)
                .Index(t => t.BatchId)
                .Index(t => t.AccountId)
                .Index(t => t.CurrencyTypeValueId)
                .Index(t => t.CreditCardTypeValueId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            CreateTable(
                "dbo.AnalyticsDimDate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateKey = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false, storeType: "date"),
                        FullDateDescription = c.String(),
                        DayOfWeek = c.String(),
                        DayOfWeekAbbreviated = c.String(),
                        DayNumberInCalendarMonth = c.Int(nullable: false),
                        DayNumberInCalendarYear = c.Int(nullable: false),
                        DayNumberInFiscalMonth = c.Int(nullable: false),
                        DayNumberInFiscalYear = c.Int(nullable: false),
                        LastDayInMonthIndictor = c.Boolean(nullable: false),
                        WeekNumberInMonth = c.Int(nullable: false),
                        SundayDate = c.DateTime(nullable: false, storeType: "date"),
                        GivingMonth = c.Int(nullable: false),
                        CalendarWeekNumberInYear = c.Int(nullable: false),
                        CalendarInMonthName = c.String(),
                        CalendarInMonthNameAbbrevated = c.String(),
                        CalendarMonthNumberInYear = c.Int(nullable: false),
                        CalendarYearMonth = c.String(),
                        CalendarQuarter = c.String(),
                        CalendarYearQuarter = c.String(),
                        CalendarYear = c.Int(nullable: false),
                        FiscalWeek = c.Int(nullable: false),
                        FiscalWeekNumberInYear = c.Int(nullable: false),
                        FiscalMonth = c.String(),
                        FiscalMonthAbbrevated = c.String(),
                        FiscalMonthNumberInYear = c.Int(nullable: false),
                        FiscalMonthYear = c.Int(nullable: false),
                        FiscalQuarter = c.String(),
                        FiscalYearQuarter = c.String(),
                        FiscalHalfYear = c.String(),
                        FiscalYear = c.Int(nullable: false),
                        HolidayIndicator = c.Boolean(nullable: false),
                        WeekHolidayIndicator = c.Boolean(nullable: false),
                        EasterIndicator = c.Boolean(nullable: false),
                        EasterWeekIndicator = c.Boolean(nullable: false),
                        ChristmasIndicator = c.Boolean(nullable: false),
                        ChristmasWeekIndicator = c.Boolean(nullable: false),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
           
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "TransactionTypeValueId", "dbo.AnalyticsDimFinancialTransactionType");
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "TransactionDateKey", "dbo.AnalyticsDimDate");
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "SourceTypeValueId", "dbo.AnalyticsDimFinancialTransactionSource");
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "CurrencyTypeValueId", "dbo.AnalyticsDimFinancialTransactionCurrencyType");
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "CreditCardTypeValueId", "dbo.AnalyticsDimFinancialTransactionCreditCardType");
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "BatchId", "dbo.AnalyticsDimFinancialBatch");
            DropForeignKey("dbo.AnalyticsSourceFinancialTransaction", "AccountId", "dbo.AnalyticsDimFinancialAccount");
            DropIndex("dbo.AnalyticsDimDate", new[] { "ForeignKey" });
            DropIndex("dbo.AnalyticsDimDate", new[] { "ForeignGuid" });
            DropIndex("dbo.AnalyticsDimDate", new[] { "ForeignId" });
            DropIndex("dbo.AnalyticsDimDate", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "ForeignKey" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "ForeignGuid" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "ForeignId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "CreditCardTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "CurrencyTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "AccountId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "BatchId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "SourceTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionDateKey" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionKey" });

            DropTable("dbo.AnalyticsDimDate");
            DropTable("dbo.AnalyticsSourceFinancialTransaction");
        }
    }
}
