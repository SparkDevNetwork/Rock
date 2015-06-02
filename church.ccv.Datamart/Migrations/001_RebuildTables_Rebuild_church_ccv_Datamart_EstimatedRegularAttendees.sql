CREATE TABLE [_church_ccv_Datamart_ERA](
	[Id] [int] IDENTITY(1,1) NOT NULL,
    [WeekendDate] [datetime] NOT NULL,
	[FamilyId] [int] NOT NULL,
	[TimesAttendedLast16Weeks] [int] NULL,
	[FirstAttended] [datetime] NULL,
	[LastAttended] [datetime] NULL,
	[TimesGaveLast6Weeks] [int] NULL,
	[TimesGaveLastYear] [int] NULL,
	[TimesGaveTotal] [int] NULL,
	[LastGave] [datetime] NULL,
	[RegularAttendee] [bit] NOT NULL DEFAULT ((0)),
	[RegularAttendeeC] [bit] NOT NULL DEFAULT ((0)),
	[RegularAttendeeG] [bit] NOT NULL DEFAULT ((0)),
	[Guid] [uniqueidentifier] NOT NULL  DEFAULT (newid()),
	[ForeignId] [nvarchar](100) NULL,
 CONSTRAINT [PK__church_ccv_Datamart_ERA] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

insert into [_church_ccv_Datamart_ERA]
SELECT [WeekendDate]
      ,[FamilyId]
      ,[TimesAttendedLast16Weeks]
      ,[FirstAttended]
      ,[LastAttended]
      ,[TimesGaveLast6Weeks]
      ,[TimesGaveLastYear]
      ,[TimesGaveTotal]
      ,[LastGave]
      ,[RegularAttendee]
      ,[RegularAttendeeC]
      ,[RegularAttendeeG]
      ,[Guid]
      ,[ForeignId]
  FROM [_church_ccv_Datamart_EstimatedRegularAttendees]

drop table [_church_ccv_Datamart_EstimatedRegularAttendees]

CREATE NONCLUSTERED INDEX [IDX_WeekendDateFamilyId] ON [dbo].[_church_ccv_Datamart_ERA]
(
	[WeekendDate] ASC,
	[FamilyId] ASC
)








