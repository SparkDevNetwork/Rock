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
                    DateKey = c.Int( nullable: false ),
                    Date = c.DateTime( nullable: false, storeType: "date" ),
                    FullDateDescription = c.String(),
                    DayOfWeek = c.Int( nullable: false ),
                    DayOfWeekName = c.String( maxLength: 450 ),
                    DayOfWeekAbbreviated = c.String( maxLength: 450 ),
                    DayNumberInCalendarMonth = c.Int( nullable: false ),
                    DayNumberInCalendarYear = c.Int( nullable: false ),
                    DayNumberInFiscalMonth = c.Int( nullable: false ),
                    DayNumberInFiscalYear = c.Int( nullable: false ),
                    LastDayInMonthIndictor = c.Boolean( nullable: false ),
                    WeekNumberInMonth = c.Int( nullable: false ),
                    SundayDate = c.DateTime( nullable: false, storeType: "date" ),
                    GivingMonth = c.Int( nullable: false ),
                    GivingMonthName = c.String( maxLength: 450 ),
                    CalendarWeek = c.Int( nullable: false ),
                    CalendarMonth = c.Int( nullable: false ),
                    CalendarMonthName = c.String( maxLength: 450 ),
                    CalendarMonthNameAbbrevated = c.String( maxLength: 450 ),
                    CalendarYearMonth = c.String( maxLength: 450 ),
                    CalendarQuarter = c.String( maxLength: 450 ),
                    CalendarYearQuarter = c.String( maxLength: 450 ),
                    CalendarYear = c.Int( nullable: false ),
                    FiscalWeek = c.Int( nullable: false ),
                    FiscalWeekNumberInYear = c.Int( nullable: false ),
                    FiscalMonth = c.String( maxLength: 450 ),
                    FiscalMonthAbbrevated = c.String( maxLength: 450 ),
                    FiscalMonthNumberInYear = c.Int( nullable: false ),
                    FiscalMonthYear = c.String( maxLength: 450 ),
                    FiscalQuarter = c.String( maxLength: 450 ),
                    FiscalYearQuarter = c.String( maxLength: 450 ),
                    FiscalHalfYear = c.String( maxLength: 450 ),
                    FiscalYear = c.Int( nullable: false ),
                    HolidayIndicator = c.Boolean( nullable: false ),
                    WeekHolidayIndicator = c.Boolean( nullable: false ),
                    EasterIndicator = c.Boolean( nullable: false ),
                    EasterWeekIndicator = c.Boolean( nullable: false ),
                    ChristmasIndicator = c.Boolean( nullable: false ),
                    ChristmasWeekIndicator = c.Boolean( nullable: false ),
                } )
                .PrimaryKey( t => t.DateKey )
                .Index( t => t.Date, unique: true );

            CreateTable(
                "dbo.AnalyticsSourceAttendance",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    AttendanceId = c.Int( nullable: false ),
                    AttendanceDateKey = c.Int( nullable: false ),
                    AttendanceTypeId = c.Int(),
                    DaysSinceLastAttendanceOfType = c.Int(),
                    IsFirstAttendanceOfType = c.Boolean( nullable: false ),
                    Count = c.Int( nullable: false ),
                    PersonKey = c.Int(),
                    CurrentPersonKey = c.Int(),
                    LocationId = c.Int(),
                    CampusId = c.Int(),
                    ScheduleId = c.Int(),
                    GroupId = c.Int(),
                    PersonAliasId = c.Int(),
                    DeviceId = c.Int(),
                    SearchTypeName = c.String(),
                    StartDateTime = c.DateTime( nullable: false ),
                    EndDateTime = c.DateTime(),
                    RSVP = c.Int( nullable: false ),
                    DidAttend = c.Boolean(),
                    Note = c.String(),
                    SundayDate = c.DateTime( nullable: false, storeType: "date" ),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .Index( t => t.AttendanceId, unique: true )
                .Index( t => t.AttendanceDateKey )
                .Index( t => t.StartDateTime )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AnalyticsSourceFamilyHistorical",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    FamilyId = c.Int( nullable: false ),
                    CurrentRowIndicator = c.Boolean( nullable: false ),
                    EffectiveDate = c.DateTime( nullable: false, storeType: "date" ),
                    ExpireDate = c.DateTime( nullable: false, storeType: "date" ),
                    Name = c.String( maxLength: 100 ),
                    FamilyTitle = c.String( maxLength: 250 ),
                    CampusId = c.Int(),
                    ConnectionStatus = c.String( maxLength: 250 ),
                    IsFamilyActive = c.Boolean( nullable: false ),
                    AdultCount = c.Int( nullable: false ),
                    ChildCount = c.Int( nullable: false ),
                    HeadOfHouseholdPersonKey = c.Int(),
                    IsEra = c.Boolean( nullable: false ),
                    MailingAddressLocationId = c.Int(),
                    MappedAddressLocationId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AnalyticsSourceFinancialTransaction",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    TransactionKey = c.String( maxLength: 40 ),
                    TransactionDateKey = c.Int( nullable: false ),
                    AuthorizedPersonKey = c.Int(),
                    AuthorizedCurrentPersonKey = c.Int(),
                    DaysSinceLastTransactionOfType = c.Int(),
                    IsFirstTransactionOfType = c.Boolean( nullable: false ),
                    AuthorizedFamilyId = c.Int(),
                    IsScheduled = c.Boolean( nullable: false ),
                    TransactionFrequency = c.String( maxLength: 250 ),
                    GivingGroupId = c.Int(),
                    GivingId = c.String( maxLength: 20 ),
                    Count = c.Int( nullable: false ),
                    TransactionDateTime = c.DateTime( nullable: false ),
                    TransactionCode = c.String( maxLength: 50 ),
                    Summary = c.String(),
                    TransactionTypeValueId = c.Int( nullable: false ),
                    SourceTypeValueId = c.Int(),
                    AuthorizedPersonAliasId = c.Int(),
                    ProcessedByPersonAliasId = c.Int(),
                    ProcessedDateTime = c.DateTime(),
                    BatchId = c.Int(),
                    FinancialGatewayId = c.Int(),
                    EntityTypeId = c.Int(),
                    EntityId = c.Int(),
                    TransactionId = c.Int( nullable: false ),
                    TransactionDetailId = c.Int( nullable: false ),
                    AccountId = c.Int( nullable: false ),
                    CurrencyTypeValueId = c.Int(),
                    CreditCardTypeValueId = c.Int(),
                    Amount = c.Decimal( nullable: false, precision: 18, scale: 2 ),
                    ModifiedDateTime = c.DateTime(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .Index( t => t.TransactionKey, unique: true )
                .Index( t => t.TransactionDateKey )
                .Index( t => t.Guid, unique: true );

            CreateTable(
               "dbo.AnalyticsSourcePersonHistorical",
               c => new
               {
                   Id = c.Int( nullable: false, identity: true ),
                   PersonId = c.Int( nullable: false ),
                   CurrentRowIndicator = c.Boolean( nullable: false ),
                   EffectiveDate = c.DateTime( nullable: false, storeType: "date" ),
                   ExpireDate = c.DateTime( nullable: false, storeType: "date" ),
                   PrimaryFamilyId = c.Int(),
                   RecordTypeValueId = c.Int(),
                   RecordStatusValueId = c.Int(),
                   RecordStatusLastModifiedDateTime = c.DateTime(),
                   RecordStatusReasonValueId = c.Int(),
                   ConnectionStatusValueId = c.Int(),
                   ReviewReasonValueId = c.Int(),
                   IsDeceased = c.Boolean( nullable: false ),
                   TitleValueId = c.Int(),
                   FirstName = c.String( maxLength: 50 ),
                   NickName = c.String( maxLength: 50 ),
                   MiddleName = c.String( maxLength: 50 ),
                   LastName = c.String( maxLength: 50 ),
                   SuffixValueId = c.Int(),
                   PhotoId = c.Int(),
                   BirthDay = c.Int(),
                   BirthMonth = c.Int(),
                   BirthYear = c.Int(),
                   BirthDateKey = c.Int(),
                   Age = c.Int(),
                   Gender = c.Int( nullable: false ),
                   MaritalStatusValueId = c.Int(),
                   AnniversaryDate = c.DateTime( storeType: "date" ),
                   GraduationYear = c.Int(),
                   GivingGroupId = c.Int(),
                   GivingId = c.String(),
                   GivingLeaderId = c.Int(),
                   Email = c.String( maxLength: 75 ),
                   EmailPreference = c.Int( nullable: false ),
                   ReviewReasonNote = c.String( maxLength: 1000 ),
                   InactiveReasonNote = c.String( maxLength: 1000 ),
                   SystemNote = c.String( maxLength: 1000 ),
                   ViewedCount = c.Int(),
                   Guid = c.Guid( nullable: false ),
                   ForeignId = c.Int(),
                   ForeignGuid = c.Guid(),
                   ForeignKey = c.String( maxLength: 100 ),
               } )
               .PrimaryKey( t => t.Id )
               .Index( t => t.BirthDateKey )
               .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.Attribute", "IsAnalytic", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Attribute", "IsAnalyticHistory", c => c.Boolean( nullable: false ) );
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
CREATE UNIQUE NONCLUSTERED INDEX [IX_FamilyId_ExpireDate] ON [dbo].[AnalyticsSourceFamilyHistorical]
(
	[FamilyId] ASC,
	[ExpireDate] ASC
)
INCLUDE ( [Id]) 

" );

            // Enforce that there isn't more than one CurrentRow per Family
            // Notice the cool 'where CurrentRowIndicator = 1' filter, Woohooo!
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_FamilyIdCurrentRow] ON [dbo].[AnalyticsSourceFamilyHistorical]
(
	[FamilyId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );

            // NOTE: Some of these have dependancies on others, so order is important!
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimAttendanceLocation );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFinancialAccount );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFinancialBatch );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFinancialTransactionType );

            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFamilyHistorical );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFamilyCurrent );

            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimPersonHistorical );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimPersonCurrent );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFamilyHeadOfHousehold );

            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsFactAttendance );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsFactFinancialTransaction );

            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimPersonBirthDate );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimAttendanceDate );
            Sql( MigrationSQL._201701040116351_Analytics1_AnalyticsDimFinancialTransactionDate );

            // Stored Procs for BI Analytics
            Sql( MigrationSQL._201701040116351_Analytics1_spAnalytics_ETL_Attendance );
            Sql( MigrationSQL._201701040116351_Analytics1_spAnalytics_ETL_Family );
            Sql( MigrationSQL._201701040116351_Analytics1_spAnalytics_ETL_FinancialTransaction );

            Sql( MigrationSQL._201701040116351_Analytics1_SetAttributesIsAnalytics );

            // Add Process BI Analytics ETL Job
            Sql( @"
INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Process BI Analytics ETL'
         ,'Run the Stored Procedures that do the ETL for the AnalyticsDimFamily, AnalyticsFactFinancialTransaction, and AnalyticsFactAttendance tables.'
         ,'Rock.Jobs.RunSQL'
         ,'0 0 5 1/1 * ? *'
         ,3
         ,'447B248B-2187-4368-9EE3-6E17B8F542A7')
" );

            // Set the RunSQL Job to run the ETL Stored Procs
            Sql( @"
                
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '7AD0C57A-D40E-4A14-81D8-8ACA68600FF5')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [ServiceJob] where [Guid] = '447B248B-2187-4368-9EE3-6E17B8F542A7')

                DELETE FROM [AttributeValue] WHERE [Guid] = 'A6F83C17-5950-44B6-9C07-94DDAFCFBC39'

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'EXEC [dbo].[spAnalytics_ETL_Family]

EXEC [dbo].[spAnalytics_ETL_FinancialTransaction]

EXEC [dbo].[spAnalytics_ETL_Attendance]','A6F83C17-5950-44B6-9C07-94DDAFCFBC39')" );


            // Set the RunSQL Job to have a timeout of 3600 seconds (one hour) 
            Sql( @"
                
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FF66ABF1-B01D-4AE7-814E-95D842B2EA99')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [ServiceJob] where [Guid] = '447B248B-2187-4368-9EE3-6E17B8F542A7')

                DELETE FROM [AttributeValue] WHERE [Guid] = '4753297A-7012-4400-B161-2C9F54360E62'

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'3600','4753297A-7012-4400-B161-2C9F54360E62')" );


            // Job for Analytics Dim Person
            Sql( @"
INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Process Analytics Dimension Tables for Person'
         ,'Job to take care of schema changes ( dynamic Attribute Value Fields ) and data updates to Person analytic tables'
         ,'Rock.Jobs.ProcessAnalyticsDimPerson'
         ,'0 0 4 1/1 * ? *'
         ,3
         ,'BBBB1D16-E4B5-439E-94F6-52AB14AE5292')" );


            // Calender Dimensions Page/BlockType/Block
            RockMigrationHelper.AddPage( "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Calendar Dimension Settings", "", "2660D554-D161-44A1-9763-A73C60559B50", "fa fa-calendar" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Calendar Dimension Settings", "Helps configure and generate the AnalyticsDimDate table for BI Analytics", "~/Blocks/Reporting/CalendarDimensionSettings.ascx", "Reporting", "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43" );

            // Add Block to Page: Calendar Dimension Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( "2660D554-D161-44A1-9763-A73C60559B50", "", "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43", "Calendar Dimension Settings", "Main", "", "", 0, "18F256EF-8888-4009-814B-B85F36FABE31" );

            // Attrib for BlockType: Calendar Dimension Settings:GivingMonthUseSundayDate
            RockMigrationHelper.AddBlockTypeAttribute( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GivingMonthUseSundayDate", "GivingMonthUseSundayDate", "", "", 1, @"False", "2C3BA4C0-2721-4648-A192-CE45E2B48364" );

            // Attrib for BlockType: Calendar Dimension Settings:FiscalStartMonth
            RockMigrationHelper.AddBlockTypeAttribute( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "FiscalStartMonth", "FiscalStartMonth", "", "", 0, @"1", "EDD9EFC5-6BFA-4979-815E-AC53214B6D47" );

            // Attrib for BlockType: Calendar Dimension Settings:StartDate
            RockMigrationHelper.AddBlockTypeAttribute( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "StartDate", "StartDate", "", "", 0, @"", "0FADA4D7-043C-45A6-9449-9F36684B2DFB" );

            // Attrib for BlockType: Calendar Dimension Settings:EndDate
            RockMigrationHelper.AddBlockTypeAttribute( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "EndDate", "EndDate", "", "", 0, @"", "BF6DACA9-BCC6-445C-88D1-1D2947755BB7" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete Process BI Analytics ETL Job
            Sql( "DELETE FROM [ServiceJob] where [Guid] = '447B248B-2187-4368-9EE3-6E17B8F542A7'" );

            // delete Job for Analytics Dim Person
            Sql( "DELETE FROM [ServiceJob] where [Guid] = 'BBBB1D16-E4B5-439E-94F6-52AB14AE5292'" );

            // Attrib for BlockType: Calendar Dimension Settings:GivingMonthUseSundayDate
            RockMigrationHelper.DeleteAttribute( "2C3BA4C0-2721-4648-A192-CE45E2B48364" );
            // Attrib for BlockType: Calendar Dimension Settings:FiscalStartMonth
            RockMigrationHelper.DeleteAttribute( "EDD9EFC5-6BFA-4979-815E-AC53214B6D47" );
            // Attrib for BlockType: Calendar Dimension Settings:EndDate
            RockMigrationHelper.DeleteAttribute( "BF6DACA9-BCC6-445C-88D1-1D2947755BB7" );
            // Attrib for BlockType: Calendar Dimension Settings:StartDate
            RockMigrationHelper.DeleteAttribute( "0FADA4D7-043C-45A6-9449-9F36684B2DFB" );

            // Remove Block: Calendar Dimension Settings, from Page: Calendar Dimension Settings, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "18F256EF-8888-4009-814B-B85F36FABE31" );

            RockMigrationHelper.DeleteBlockType( "7711EAE9-5CF0-46E4-A4E6-26C05A71FE43" ); // Calendar Dimension Settings

            RockMigrationHelper.DeletePage( "2660D554-D161-44A1-9763-A73C60559B50" ); //  Page: Calendar Dimension Settings, Layout: Full Width, Site: Rock RMS

            Sql( @"
DROP VIEW AnalyticsDimPersonBirthDate;
DROP VIEW AnalyticsDimAttendanceDate;
DROP VIEW AnalyticsDimFinancialTransactionDate;
DROP VIEW AnalyticsDimAttendanceLocation;
DROP VIEW AnalyticsDimFamilyCurrent;
DROP VIEW AnalyticsDimFamilyHeadOfHousehold;
DROP VIEW AnalyticsDimFamilyHistorical;
DROP VIEW AnalyticsDimFinancialAccount;
DROP VIEW AnalyticsDimFinancialBatch;
DROP VIEW AnalyticsDimFinancialTransactionType;
DROP VIEW AnalyticsDimPersonCurrent;
DROP VIEW AnalyticsDimPersonHistorical;
DROP VIEW AnalyticsFactAttendance;
DROP VIEW AnalyticsFactFinancialTransaction;
DROP PROCEDURE spAnalytics_ETL_Attendance;
DROP PROCEDURE spAnalytics_ETL_Family;
DROP PROCEDURE spAnalytics_ETL_FinancialTransaction;
" );

            DropIndex( "dbo.AnalyticsSourcePersonHistorical", new[] { "Guid" } );
            DropIndex( "dbo.AnalyticsSourcePersonHistorical", new[] { "BirthDateKey" } );

            DropIndex( "dbo.AnalyticsSourceFinancialTransaction", new[] { "Guid" } );

            DropIndex( "dbo.AnalyticsSourceFinancialTransaction", new[] { "AccountId" } );
            DropIndex( "dbo.AnalyticsSourceFinancialTransaction", new[] { "BatchId" } );
            DropIndex( "dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionTypeValueId" } );
            DropIndex( "dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionDateKey" } );
            DropIndex( "dbo.AnalyticsSourceFinancialTransaction", new[] { "TransactionKey" } );

            DropIndex( "dbo.AnalyticsSourceFamilyHistorical", new[] { "Guid" } );

            DropIndex( "dbo.AnalyticsSourceAttendance", new[] { "Guid" } );
            DropIndex( "dbo.AnalyticsSourceAttendance", new[] { "StartDateTime" } );
            DropIndex( "dbo.AnalyticsSourceAttendance", new[] { "LocationId" } );
            DropIndex( "dbo.AnalyticsSourceAttendance", new[] { "AttendanceDateKey" } );
            DropIndex( "dbo.AnalyticsSourceAttendance", new[] { "AttendanceId" } );

            DropIndex( "dbo.AnalyticsDimDate", new[] { "Date" } );

            DropColumn( "dbo.Metric", "EnableAnalytics" );

            DropIndex( "dbo.AttributeValue", "IX_ValueAsBoolean" );
            DropColumn( "dbo.AttributeValue", "ValueAsBoolean" );
            DropColumn( "dbo.Attribute", "IsAnalyticHistory" );
            DropColumn( "dbo.Attribute", "IsAnalytic" );

            DropTable( "dbo.AnalyticsSourcePersonHistorical" );
            DropTable( "dbo.AnalyticsSourceFinancialTransaction" );
            DropTable( "dbo.AnalyticsSourceFamilyHistorical" );
            DropTable( "dbo.AnalyticsSourceAttendance" );
            DropTable( "dbo.AnalyticsDimDate" );
        }
    }
}
