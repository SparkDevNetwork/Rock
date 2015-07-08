// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class SampleCalendarEvents : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
            INSERT [dbo].[EventCalendar] ( [Name], [Description], [IconCssClass], [IsActive],  [Guid], [ForeignId]) 
                VALUES ( N'Internal', N'A calendar for church staff and staff-like users.', N'fa fa-lock', 1, N'8c7f7f4e-1c51-41d3-9ac3-02b3f4054798', NULL)
			DECLARE @InternalCalendarId int = SCOPE_IDENTITY()


			DECLARE @PublicCalendarId int = (SELECT TOP 1 [Id] FROM [EventCalendar] WHERE [Guid] = '8A444668-19AF-4417-9C74-09F842572974')
			DECLARE @CarPhotoId int = (SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '8EE5A840-0A10-44E2-9DBE-214AF27C234B')
			DECLARE @FinancePhotoId int = (SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = '5047381D-0CA9-49F7-9894-699C296BAAB6')
			DECLARE @YouthPhotoId int = (SELECT TOP 1 [Id] FROM [BinaryFile] WHERE [Guid] = 'D00FCEAA-2D16-40BF-9BA4-352D42605E28')

            INSERT [dbo].[EventItem] ( [Name], [Summary], [Description], [PhotoId], [DetailsUrl], [IsActive], [Guid], [ForeignId], [IsApproved], [ApprovedByPersonAliasId], [ApprovedOnDateTime]) 
                VALUES ( N'Staff Meeting', NULL, N'A sample staff meeting calendar item.', NULL, N'', 1, N'93104654-dafa-489b-a175-5f2ab3a846f1', NULL, 1, 10, CAST(N'2015-06-26 09:19:05.597' AS DateTime))
			DECLARE @StaffMeetingId int = SCOPE_IDENTITY()

            INSERT [dbo].[EventItem] ( [Name], [Summary], [Description], [PhotoId], [DetailsUrl], [IsActive], [Guid], [ForeignId], [IsApproved], [ApprovedByPersonAliasId], [ApprovedOnDateTime]) 
                VALUES ( N'Customs & Classics Car Show', NULL, N'Curabitur a neque in nibh pretium rutrum nec pharetra ligula. Nulla molestie imperdiet rhoncus. Nulla non semper sapien. Phasellus vel nisi vel ante imperdiet lacinia eu quis odio. In et felis eu sem luctus lacinia. Donec et purus eu dui luctus vehicula. Proin malesuada arcu at ipsum volutpat ullamcorper. Donec facilisis eros a turpis volutpat, at faucibus turpis bibendum. Praesent faucibus mauris sit amet erat lobortis faucibus at rutrum nunc. Phasellus dapibus sed quam eu sodales. Nulla ornare venenatis venenatis.', @CarPhotoId, N'', 1, N'6bb29d45-d5a0-4381-a0d9-e5490a58e20b', NULL, 1, 10, CAST(N'2015-06-26 09:33:42.050' AS DateTime))
			DECLARE @CarShowId int = SCOPE_IDENTITY()

            INSERT [dbo].[EventItem] ( [Name], [Summary], [Description], [PhotoId], [DetailsUrl], [IsActive], [Guid], [ForeignId], [IsApproved], [ApprovedByPersonAliasId], [ApprovedOnDateTime]) 
                VALUES ( N'Rock Solid Finances Class', NULL, N'Duis vel massa egestas, cursus odio vestibulum, pulvinar felis. Quisque mattis enim nec libero euismod venenatis id nec arcu. Donec quis lectus leo. Nullam nec enim a massa placerat fermentum. Pellentesque dolor turpis, imperdiet nec nisl sed, ultricies condimentum sapien. Proin facilisis quam diam, quis varius risus aliquam eu. Suspendisse sed neque interdum nulla egestas molestie eget sed est. Mauris sed eros in neque scelerisque consequat. Ut commodo semper pharetra.', @FinancePhotoId, N'', 1, N'6efc00b0-f5d3-4352-bc3b-f09852fb5788', NULL, 1, 10, CAST(N'2015-06-26 09:36:21.923' AS DateTime))
			DECLARE @FinanceId int = SCOPE_IDENTITY()

            INSERT [dbo].[EventItem] ( [Name], [Summary], [Description], [PhotoId], [DetailsUrl], [IsActive], [Guid], [ForeignId], [IsApproved], [ApprovedByPersonAliasId], [ApprovedOnDateTime]) 
                VALUES ( N'Warrior Youth Event', NULL, N'Maecenas eget elit dui. Nullam eu elementum ante. Morbi placerat in nisi eget hendrerit. Cras facilisis massa sit amet luctus dictum. Quisque pretium sapien vitae tincidunt molestie. Etiam eu lacinia odio. Nullam sit amet interdum lectus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam tempor et tortor et tristique. Ut sagittis neque non metus molestie, et dignissim massa porta.', @YouthPhotoId, N'', 1, N'64966c21-648e-4b31-8d97-de1b62fa0820', NULL, 1, 10, CAST(N'2015-06-26 09:42:08.810' AS DateTime))
			DECLARE @YouthEventId int = SCOPE_IDENTITY()


            INSERT [dbo].[EventCalendarItem] ( [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @InternalCalendarId, @StaffMeetingId, N'4732588d-3dfc-40cc-81c4-d3563287d7e7', NULL)
 
            INSERT [dbo].[EventCalendarItem] (  [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @InternalCalendarId, @CarShowId, N'5afa9ed9-28d8-4b2e-ac61-85d6cf12e292', NULL)
 
            INSERT [dbo].[EventCalendarItem] (  [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @PublicCalendarId, @CarShowId, N'16b0028d-134b-4cdd-96fd-2874f2efc470', NULL)
 
            INSERT [dbo].[EventCalendarItem] (  [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @InternalCalendarId, @FinanceId, N'25b28af6-f004-46df-9b19-bd7308666042', NULL)
 
            INSERT [dbo].[EventCalendarItem] (  [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @PublicCalendarId, @FinanceId, N'837d5511-f6b1-4c84-a74e-702e7d51c0ad', NULL)
 
            INSERT [dbo].[EventCalendarItem] (  [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @InternalCalendarId, @YouthEventId, N'25da2631-98a4-4430-ab9c-d85b88f3500b', NULL)
 
            INSERT [dbo].[EventCalendarItem] (  [EventCalendarId], [EventItemId], [Guid], [ForeignId]) 
                VALUES ( @PublicCalendarId, @YouthEventId, N'cf164673-25ea-489a-ab06-f6dde5185e22', NULL)
 

			DECLARE @AllChurchId int =  (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '6107EA37-5DD3-4E4F-A2D0-1D4010811D4D')
			DECLARE @MenId int =  (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = 'A4BEBC2F-09F0-488A-B2F3-C416F4D02E35')
			DECLARE @WomenId int =  (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4CE2E860-2F03-40F9-8B60-68EBDB21E026')
			DECLARE @AdultsId int =  (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '95E49778-AE72-454F-91CC-2FC864557DEC')
			DECLARE @YouthId int =  (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '59CD7FD8-6A62-4C3B-8966-1520E74EED58')

			IF @AllChurchId IS NOT NULL
			BEGIN
				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @CarShowId, @AllChurchId, N'20bed1f5-a489-4d05-afb3-4679a47174dd', NULL)

				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @FinanceId, @AllChurchId, N'5e773ebf-9b32-4abe-84eb-59f4fe14d40f', NULL)				

				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @YouthEventId, @AllChurchId, N'db479dff-d2a2-413f-974d-91d89331e920', NULL)
			END

			IF @MenId IS NOT NULL
			BEGIN
				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @CarShowId, @MenId, N'5810d1c6-1789-4e8b-b567-dbcf4a50c3a1', NULL)

				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @FinanceId, @MenId, N'fcb438b7-7138-448c-a9bb-f7e5e23be47c', NULL)
			END

			IF @WomenId IS NOT NULL
			BEGIN
				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @FinanceId, @WomenId, N'11df9461-1baa-485b-a04a-b1573d407dd3', NULL)
			END
			
			IF @AdultsId IS NOT NULL
			BEGIN
				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @CarShowId, @AdultsId, N'7421a769-de2f-4c68-93c2-4baf53db4917', NULL)

				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @FinanceId, @AdultsId, N'de02cfde-3991-4e9f-af37-ee9ba4ca8698', NULL)

				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @YouthEventId, @AdultsId, N'99871ed7-8b61-4cbe-b004-faa3b7f21c03', NULL)
			END

			IF @YouthId IS NOT NULL
			BEGIN
				INSERT [dbo].[EventItemAudience] (  [EventItemId], [DefinedValueId], [Guid], [ForeignId]) 
					VALUES ( @YouthEventId, @YouthId, N'b05ee832-d2fc-4e75-8281-a627144e9c00', NULL)
			END


            INSERT [dbo].[EventItemCampus] (  [EventItemId], [CampusId], [Location], [ContactPersonAliasId], [ContactPhone], [ContactEmail], [CampusNote], [Guid], [ForeignId]) 
                VALUES ( @StaffMeetingId, NULL, N'', 10, N'', N'admin@organization.com', N'', N'f959d1a9-ee5c-4571-932b-10419971b76f', NULL)
 			DECLARE @StaffMeetingCampusId int = SCOPE_IDENTITY()

            INSERT [dbo].[EventItemCampus] (  [EventItemId], [CampusId], [Location], [ContactPersonAliasId], [ContactPhone], [ContactEmail], [CampusNote], [Guid], [ForeignId]) 
                VALUES ( @CarShowId, NULL, N'', 10, N'', N'admin@organization.com', N'', N'22579acd-e636-4d52-b682-5883edff331d', NULL)
			DECLARE @CarShowCampusId int = SCOPE_IDENTITY()
 
            INSERT [dbo].[EventItemCampus] (  [EventItemId], [CampusId], [Location], [ContactPersonAliasId], [ContactPhone], [ContactEmail], [CampusNote], [Guid], [ForeignId]) 
                VALUES ( @FinanceId, NULL, N'', 10, N'', N'admin@organization.com', N'', N'8d435c91-f2d7-4192-b0f1-27c1d48d5135', NULL)
 			DECLARE @FinanceCampusId int = SCOPE_IDENTITY()

            INSERT [dbo].[EventItemCampus] (  [EventItemId], [CampusId], [Location], [ContactPersonAliasId], [ContactPhone], [ContactEmail], [CampusNote], [Guid], [ForeignId]) 
                VALUES ( @YouthEventId, NULL, N'', 10, N'', N'admin@organization.com', N'', N'7b095d3d-ac48-4406-a1e9-22c0979f4aea', NULL)
			DECLARE @YouthEventCampusId int = SCOPE_IDENTITY()

			DECLARE @EntityTypeId int =  (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A')
			DECLARE @Order int = (SELECT Max([Order]) FROM [Category] WHERE [EntityTypeId] = @EntityTypeId)+1
			INSERT INTO [dbo].[Category] ( [IsSystem], [Order], [ParentCategoryId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Name], [IconCssClass], [Guid]) 
				VALUES ( 1, @Order, NULL, 54, N'', N'', N'Event Schedules', NULL, N'153b61a4-2faa-4364-b75a-ac2d8961d8b7')
			DECLARE @ScheduleCategoryId int = SCOPE_IDENTITY()
 		
			DECLARE @Date DATETIME = GETDATE()
			DECLARE @TwoWeeksDate DATETIME = DATEADD(DD, 14, @Date)
			DECLARE @MonthDate DATETIME = DATEADD(DD, 30, @Date)
			DECLARE @TwoWeeksStartDate VARCHAR(15)
			DECLARE @TwoWeeksEndDate VARCHAR(15)
			DECLARE @MonthStartDate VARCHAR(15)
			DECLARE @MonthEndDate VARCHAR(15)
			SET @TwoWeeksEndDate = REPLACE(REPLACE(REPLACE(CONVERT(char, DATEADD(SS, 1, @TwoWeeksDate), 126), '-',''),':',''),'.','')
			SET @TwoWeeksStartDate = REPLACE(REPLACE(REPLACE(CONVERT(char, @TwoWeeksDate, 126), '-',''),':',''),'.','')
			SET @MonthEndDate = REPLACE(REPLACE(REPLACE(CONVERT(char, DATEADD(SS, 1, @MonthDate), 126), '-',''),':',''),'.','')
			SET @MonthStartDate = REPLACE(REPLACE(REPLACE(CONVERT(char, @MonthDate, 126), '-',''),':',''),'.','')

 			INSERT [dbo].[Schedule] ( [Name], [Description], [iCalendarContent], [Guid], [ForeignId], [WeeklyDayOfWeek], [WeeklyTimeOfDay], [CategoryId]) 
				VALUES ( 'Staff Meeting', NULL, N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:20150610T000001
DTSTAMP:20150708T170113Z
DTSTART:20150610T000000
RRULE:FREQ=WEEKLY;INTERVAL=2;BYDAY=WE
SEQUENCE:0
UID:411ee1a1-8fe4-4b5a-88ea-9244563dffd6
END:VEVENT
END:VCALENDAR', N'84be7093-ecc4-4f13-9236-40b313514cd6', NULL, NULL, NULL, @ScheduleCategoryId)
			DECLARE @StaffMeetingScheduleId int = SCOPE_IDENTITY()

			INSERT [dbo].[Schedule] ( [Name], [Description], [iCalendarContent], [Guid], [ForeignId], [WeeklyDayOfWeek], [WeeklyTimeOfDay], [CategoryId]) 
				VALUES ( 'Car Show', NULL, N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:'+@TwoWeeksEndDate+'
DTSTAMP:20150708T170113Z
DTSTART:'+@TwoWeeksStartDate+'
SEQUENCE:0
UID:105e5538-5c17-4cc3-be03-aadd20527e3e
END:VEVENT
END:VCALENDAR', N'65e603b9-ca78-4a40-b72c-1948019fd1bb', NULL, NULL, NULL, @ScheduleCategoryId)
			DECLARE @CarShowScheduleId int = SCOPE_IDENTITY()

			INSERT [dbo].[Schedule] ( [Name], [Description], [iCalendarContent], [Guid], [ForeignId], [WeeklyDayOfWeek], [WeeklyTimeOfDay], [CategoryId]) 
				VALUES ( 'Rock Solid Finances', NULL, N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:'+@TwoWeeksEndDate+'
DTSTAMP:20150708T170113Z
DTSTART:'+@TwoWeeksStartDate+'
SEQUENCE:0
UID:39cf9c98-7a6c-4783-be87-38ebbf219ccf
END:VEVENT
END:VCALENDAR', N'5914a31f-723c-44fa-953c-f4dae77f4eae', NULL, NULL, NULL, @ScheduleCategoryId)
			DECLARE @FinanceScheduleId int = SCOPE_IDENTITY()

			INSERT [dbo].[Schedule] ( [Name], [Description], [iCalendarContent], [Guid], [ForeignId], [WeeklyDayOfWeek], [WeeklyTimeOfDay], [CategoryId]) 
				VALUES ( 'Warriors Youth Event', NULL, N'BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
DTEND:'+@MonthEndDate+'
DTSTAMP:20150708T170113Z
DTSTART:'+@MonthStartDate+'
SEQUENCE:0
UID:37d4fdd6-011b-4b61-9139-3299eecddccf
END:VEVENT
END:VCALENDAR', N'198ded50-0292-4219-a879-42ebc624e4ff', NULL, NULL, NULL, @ScheduleCategoryId)
			DECLARE @YouthEventScheduleId int = SCOPE_IDENTITY()

            INSERT [dbo].[EventItemSchedule] (  [EventItemCampusId], [ScheduleId], [ScheduleName], [Guid], [ForeignId]) 
                VALUES ( @StaffMeetingCampusId, @StaffMeetingScheduleId, N'Every Other Wednesday', N'38848d49-ffdc-48b9-b5a9-5beb6921c6ea', NULL)
 
            INSERT [dbo].[EventItemSchedule] (  [EventItemCampusId], [ScheduleId], [ScheduleName], [Guid], [ForeignId]) 
                VALUES ( @CarShowCampusId, @CarShowScheduleId, N'Event Date', N'c06be19c-2461-41fc-9199-eccb92f4169b', NULL)
 
            INSERT [dbo].[EventItemSchedule] (  [EventItemCampusId], [ScheduleId], [ScheduleName], [Guid], [ForeignId]) 
                VALUES ( @FinanceCampusId, @FinanceScheduleId, N'Event Date', N'e94f274c-ad00-4282-9091-b75801405ef8', NULL)
 
            INSERT [dbo].[EventItemSchedule] (  [EventItemCampusId], [ScheduleId], [ScheduleName], [Guid], [ForeignId]) 
                VALUES ( @YouthEventCampusId, @YouthEventScheduleId, N'Event Date', N'01acd0ea-7e26-4fd4-be9a-5d1bcf6d2fd4', NULL)
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
