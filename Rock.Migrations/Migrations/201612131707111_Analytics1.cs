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
                "dbo.AnalyticsDimDate",
                c => new
                    {
                        DateKey = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false, storeType: "date"),
                        FullDateDescription = c.String(),
                        DayOfWeek = c.Int(nullable: false),
                        DayOfWeekName = c.String(maxLength: 450),
                        DayOfWeekAbbreviated = c.String(maxLength: 450),
                        DayNumberInCalendarMonth = c.Int(nullable: false),
                        DayNumberInCalendarYear = c.Int(nullable: false),
                        DayNumberInFiscalMonth = c.Int(nullable: false),
                        DayNumberInFiscalYear = c.Int(nullable: false),
                        LastDayInMonthIndictor = c.Boolean(nullable: false),
                        WeekNumberInMonth = c.Int(nullable: false),
                        SundayDate = c.DateTime(nullable: false, storeType: "date"),
                        GivingMonth = c.Int(nullable: false),
                        GivingMonthName = c.String(maxLength: 450),
                        CalendarMonthNumberInYear = c.Int(nullable: false),
                        CalendarMonth = c.Int(nullable: false),
                        CalendarMonthName = c.String(maxLength: 450),
                        CalendarMonthNameAbbrevated = c.String(maxLength: 450),
                        CalendarYearMonth = c.String(maxLength: 450),
                        CalendarQuarter = c.String(maxLength: 450),
                        CalendarYearQuarter = c.String(maxLength: 450),
                        CalendarYear = c.Int(nullable: false),
                        FiscalWeek = c.Int(nullable: false),
                        FiscalWeekNumberInYear = c.Int(nullable: false),
                        FiscalMonth = c.String(maxLength: 450),
                        FiscalMonthAbbrevated = c.String(maxLength: 450),
                        FiscalMonthNumberInYear = c.Int(nullable: false),
                        FiscalMonthYear = c.String(maxLength: 450),
                        FiscalQuarter = c.String(maxLength: 450),
                        FiscalYearQuarter = c.String(maxLength: 450),
                        FiscalHalfYear = c.String(maxLength: 450),
                        FiscalYear = c.Int(nullable: false),
                        HolidayIndicator = c.Boolean(nullable: false),
                        WeekHolidayIndicator = c.Boolean(nullable: false),
                        EasterIndicator = c.Boolean(nullable: false),
                        EasterWeekIndicator = c.Boolean(nullable: false),
                        ChristmasIndicator = c.Boolean(nullable: false),
                        ChristmasWeekIndicator = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.DateKey)
                .Index(t => t.Date, unique: true);
            
            CreateTable(
                "dbo.AnalyticsSourceAttendance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AttendanceDateKey = c.Int(nullable: false),
                        AttendanceTypeId = c.Int(),
                        DaysSinceLastAttendanceOfType = c.Int(),
                        IsFirstAttendanceOfType = c.Boolean(nullable: false),
                        Count = c.Int(nullable: false),
                        PersonKey = c.Int(nullable: false),
                        CurrentPersonKey = c.Int(nullable: false),
                        LocationId = c.Int(),
                        CampusId = c.Int(),
                        ScheduleId = c.Int(),
                        GroupId = c.Int(),
                        PersonAliasId = c.Int(),
                        DeviceId = c.Int(),
                        SearchTypeName = c.String(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(),
                        RSVP = c.Int(nullable: false),
                        DidAttend = c.Boolean(),
                        Note = c.String(),
                        SundayDate = c.DateTime(nullable: false, storeType: "date"),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.AttendanceDateKey)
                .Index(t => t.AttendanceTypeId)
                .Index(t => t.LocationId)
                .Index(t => t.ScheduleId)
                .Index(t => t.GroupId)
                .Index(t => t.DeviceId)
                .Index(t => t.StartDateTime)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AnalyticsSourceFamilyHistorical",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        CurrentRowIndicator = c.Boolean(nullable: false),
                        EffectiveDate = c.DateTime(nullable: false, storeType: "date"),
                        ExpireDate = c.DateTime(nullable: false, storeType: "date"),
                        Name = c.String(maxLength: 100),
                        FamilyTitle = c.String(maxLength: 250),
                        CampusId = c.Int(),
                        ConnectionStatus = c.String(maxLength: 250),
                        IsFamilyActive = c.Boolean(nullable: false),
                        AdultCount = c.Int(nullable: false),
                        ChildCount = c.Int(nullable: false),
                        HeadOfHouseholdPersonKey = c.Int(),
                        IsEra = c.Boolean(nullable: false),
                        MailingAddressLocationId = c.Int(),
                        MappedAddressLocationId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AnalyticsSourceFinancialTransaction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionKey = c.String(maxLength: 40),
                        TransactionDateKey = c.Int(nullable: false),
                        AuthorizedPersonKey = c.Int(nullable: false),
                        AuthorizedCurrentPersonKey = c.Int(nullable: false),
                        DaysSinceLastTransactionOfType = c.Int(),
                        IsFirstTransactionOfType = c.Boolean(nullable: false),
                        AuthorizedFamilyId = c.Int(),
                        IsScheduled = c.Boolean(nullable: false),
                        GivingGroupId = c.Int(),
                        GivingId = c.String(maxLength: 20),
                        Count = c.Int(nullable: false),
                        TransactionDateTime = c.DateTime(nullable: false),
                        TransactionCode = c.String(maxLength: 50),
                        Summary = c.String(),
                        TransactionTypeValueId = c.Int(nullable: false),
                        SourceTypeValueId = c.Int(),
                        AuthorizedPersonAliasId = c.Int(),
                        ProcessedByPersonAliasId = c.Int(),
                        ProcessedDateTime = c.DateTime(),
                        BatchId = c.Int(),
                        FinancialGatewayId = c.Int(),
                        EntityTypeId = c.Int(),
                        EntityId = c.Int(),
                        TransactionId = c.Int(nullable: false),
                        TransactionDetailId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        CurrencyTypeValueId = c.Int(),
                        CreditCardTypeValueId = c.Int(),
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
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AnalyticsSourcePersonHistorical",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonId = c.Int(nullable: false),
                        CurrentRowIndicator = c.Boolean(nullable: false),
                        EffectiveDate = c.DateTime(nullable: false, storeType: "date"),
                        ExpireDate = c.DateTime(nullable: false, storeType: "date"),
                        PrimaryFamilyId = c.Int(),
                        RecordTypeValueId = c.Int(),
                        RecordStatusValueId = c.Int(),
                        RecordStatusLastModifiedDateTime = c.DateTime(),
                        RecordStatusReasonValueId = c.Int(),
                        ConnectionStatusValueId = c.Int(),
                        ReviewReasonValueId = c.Int(),
                        IsDeceased = c.Boolean(nullable: false),
                        TitleValueId = c.Int(),
                        FirstName = c.String(maxLength: 50),
                        NickName = c.String(maxLength: 50),
                        MiddleName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        SuffixValueId = c.Int(),
                        PhotoId = c.Int(),
                        BirthDay = c.Int(),
                        BirthMonth = c.Int(),
                        BirthYear = c.Int(),
                        BirthDateKey = c.Int(),
                        Gender = c.Int(nullable: false),
                        MaritalStatusValueId = c.Int(),
                        AnniversaryDate = c.DateTime(storeType: "date"),
                        GraduationYear = c.Int(),
                        GivingGroupId = c.Int(),
                        GivingId = c.String(),
                        GivingLeaderId = c.Int(),
                        Email = c.String(maxLength: 75),
                        EmailPreference = c.Int(nullable: false),
                        ReviewReasonNote = c.String(maxLength: 1000),
                        InactiveReasonNote = c.String(maxLength: 1000),
                        SystemNote = c.String(maxLength: 1000),
                        ViewedCount = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.RecordTypeValueId)
                .Index(t => t.RecordStatusValueId)
                .Index(t => t.RecordStatusReasonValueId)
                .Index(t => t.ConnectionStatusValueId)
                .Index(t => t.ReviewReasonValueId)
                .Index(t => t.TitleValueId)
                .Index(t => t.SuffixValueId)
                .Index(t => t.BirthDateKey)
                .Index(t => t.MaritalStatusValueId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Attribute", "IsAnalytic", c => c.Boolean(nullable: false));
            AddColumn("dbo.Attribute", "IsAnalyticHistory", c => c.Boolean(nullable: false));
            AddColumn( "dbo.Metric", "EnableAnalytics", c => c.Boolean( nullable: false ) );

            // this could take a minute or so on a large database
            Sql( @"
    ALTER TABLE AttributeValue ADD [ValueAsBoolean] AS (
    CASE 
        WHEN (Value IS NULL)
            OR (Value = '')
            OR (len(Value) > len('false'))
            THEN NULL
        WHEN lower(Value) IN (
                'true'
                ,'yes'
                ,'t'
                ,'y'
                ,'1'
                )
            THEN convert(BIT, 1)
        ELSE convert(BIT, 0)
        END
    ) PERSISTED" );

            Sql( @"CREATE NONCLUSTERED INDEX [IX_ValueAsBoolean] ON [dbo].[AttributeValue]
(
	[ValueAsBoolean] ASC
)" );

            // indexes to help speed up ETL operations
            Sql( @"
CREATE NONCLUSTERED INDEX [IX_GivingID_TransactionDateTime_TransactionTypeValueId] ON [dbo].[AnalyticsSourceFinancialTransaction]
(
	[GivingId] ASC,
	[TransactionTypeValueId] ASC,
	[TransactionDateTime] ASC
)" );


            Sql( @"
CREATE UNIQUE NONCLUSTERED INDEX [IX_PersonId_ExpireDate] ON [dbo].[AnalyticsSourcePersonHistorical]
(
	[PersonId] ASC,
	[ExpireDate] ASC
)
INCLUDE ( [Id]) 

" );

            // Enforce that there isn't more than one CurrentRow per person
            // Notice the cool 'where CurrentRowIndicator = 1' filter, Woohooo!
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_PersonIdCurrentRow] ON [dbo].[AnalyticsSourcePersonHistorical]
(
	[PersonId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );

            Sql( @"
CREATE UNIQUE NONCLUSTERED INDEX [IX_GroupId_ExpireDate] ON [dbo].[AnalyticsSourceFamilyHistorical]
(
	[GroupId] ASC,
	[ExpireDate] ASC
)
INCLUDE ( [Id]) 

" );

            // Enforce that there isn't more than one CurrentRow per Family
            // Notice the cool 'where CurrentRowIndicator = 1' filter, Woohooo!
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_GroupIdCurrentRow] ON [dbo].[AnalyticsSourceFamilyHistorical]
(
	[GroupId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "MaritalStatusValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "BirthDateKey" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "SuffixValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "TitleValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "ReviewReasonValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "ConnectionStatusValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "RecordStatusReasonValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "RecordStatusValueId" });
            DropIndex("dbo.AnalyticsSourcePersonHistorical", new[] { "RecordTypeValueId" });

            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "CreditCardTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "CurrencyTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "AccountId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "BatchId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "SourceTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionTypeValueId" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionDateKey" });
            DropIndex("dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionKey" });

            DropIndex("dbo.AnalyticsSourceFamilyHistorical", new[] { "Guid" });

            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "Guid" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "StartDateTime" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "DeviceId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "GroupId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "ScheduleId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "LocationId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "AttendanceTypeId" });
            DropIndex("dbo.AnalyticsSourceAttendance", new[] { "AttendanceDateKey" });

            DropIndex("dbo.AnalyticsDimDate", new[] { "Date" });

            DropColumn("dbo.Metric", "EnableAnalytics");

            DropIndex( "dbo.AttributeValue", "IX_ValueAsBoolean" );
            DropColumn("dbo.AttributeValue", "ValueAsBoolean");
            DropColumn("dbo.Attribute", "IsAnalyticHistory");
            DropColumn("dbo.Attribute", "IsAnalytic");

            DropTable("dbo.AnalyticsSourcePersonHistorical");
            DropTable("dbo.AnalyticsSourceFinancialTransaction");
            DropTable("dbo.AnalyticsSourceFamilyHistorical");
            DropTable("dbo.AnalyticsSourceAttendance");
            
            DropTable("dbo.AnalyticsDimDate");
        }
    }
}
