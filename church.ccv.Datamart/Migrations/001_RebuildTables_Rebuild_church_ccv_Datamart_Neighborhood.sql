CREATE TABLE [_church_ccv_Datamart_Neighborhood](
	[Id] [int] IDENTITY(1,1) NOT NULL,
    [NeighborhoodId] [int] NULL,
	[NeighborhoodName] [varchar](100) NULL,
	[NeighborhoodLeaderName] [varchar](100) NULL,
	[NeighborhoodLeaderId] [int] NULL,
	[NeighborhoodPastorName] [varchar](100) NULL,
	[NeighborhoodPastorId] [int] NULL,
	[HouseholdCount] [int] NULL,
	[AdultCount] [int] NULL,
	[AdultsInGroups] [int] NULL,
	[AdultsInGroupsPercentage] [decimal](9, 2) NULL,
	[AdultsBaptized] [int] NULL,
	[AdultsBaptizedPercentage] [decimal](9, 2) NULL,
	[AdultMemberCount] [int] NULL,
	[AdultMembersInGroups] [int] NULL,
	[AdultMembersInGroupsPercentage] [decimal](9, 2) NULL,
	[AdultAttendeeCount] [int] NULL,
	[AdultAttendeesInGroups] [int] NULL,
	[AdultAttendeesInGroupsPercentage] [decimal](9, 2) NULL,
	[AdultVisitors] [int] NULL,
	[AdultParticipants] [int] NULL,
	[ChildrenCount] [int] NULL,
	[GroupCount] [int] NULL,
	[Guid] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[ForeignId] [nvarchar](100) NULL,
 CONSTRAINT [PK_church_ccv_Datamart_Neighborhood] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

insert into [_church_ccv_Datamart_Neighborhood]
SELECT [NeighborhoodId]
      ,[NeighborhoodName]
      ,[NeighborhoodLeaderName]
      ,[NeighborhoodLeaderId]
      ,[NeighborhoodPastorName]
      ,[NeighborhoodPastorId]
      ,[HouseholdCount]
      ,[AdultCount]
      ,[AdultsInGroups]
      ,[AdultsInGroupsPercentage]
      ,[AdultsBaptized]
      ,[AdultsBaptizedPercentage]
      ,[AdultMemberCount]
      ,[AdultMembersInGroups]
      ,[AdultMembersInGroupsPercentage]
      ,[AdultAttendees]
      ,[AdultAttendeesInGroups]
      ,[AdultAttendeesInGroupsPercentage]
      ,[AdultVisitors]
      ,[AdultParticipants]
      ,[ChildrenCount]
      ,[GroupCount]
      ,[Guid]
      ,[ForeignId]
  FROM [_church_ccv_Datamart_Neighborhoods]

drop table [_church_ccv_Datamart_Neighborhoods]

CREATE NONCLUSTERED INDEX [IDX_NeighborhoodId] ON [_church_ccv_Datamart_Neighborhood]
(
	[NeighborhoodId] ASC
)