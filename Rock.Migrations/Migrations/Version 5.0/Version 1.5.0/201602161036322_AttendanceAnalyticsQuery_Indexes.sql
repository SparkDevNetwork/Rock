-- Drop unneccessary indexes

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_AttendanceCodeId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_AttendanceCodeId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_CreatedByPersonAliasId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_CreatedByPersonAliasId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_DeviceId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_DeviceId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_DidAttend_StartDateTime' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_DidAttend_StartDateTime] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ForeignGuid] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ForeignId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ForeignKey] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_Guid' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_Guid] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_LocationId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_LocationId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_LocationId_ScheduleId_GroupId_StartDateTime' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_LocationId_ScheduleId_GroupId_StartDateTime] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ModifiedByPersonAliasId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_QualifierValueId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_QualifierValueId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ScheduleId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ScheduleId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_SearchTypeValueId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_SearchTypeValueId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ForeignId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ForeignId] ON [dbo].[Attendance]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_ForeignId] ON [dbo].[Attendance]
GO

-- Modify existing indexes to include needed fields for attendance analytics performance
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_PersonAliasId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_PersonAliasId] ON [dbo].[Attendance]
GO
CREATE NONCLUSTERED INDEX [IX_PersonAliasId] ON [dbo].[Attendance]
(
	[PersonAliasId] ASC
)
INCLUDE ( 	[Id],
	[GroupId],
	[StartDateTime],
	[DidAttend],
	[LocationId],
	[ScheduleId],
	[CampusId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_StartDateTime' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_StartDateTime] ON [dbo].[Attendance]
GO
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_StartDateTime_DidAttend' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_StartDateTime_DidAttend] ON [dbo].[Attendance]
GO
CREATE NONCLUSTERED INDEX [IX_StartDateTime_DidAttend] ON [dbo].[Attendance]
(
	[StartDateTime] ASC,
	[DidAttend] ASC
)
INCLUDE ( 	[Id],
	[LocationId],
	[ScheduleId],
	[CampusId],
	[PersonAliasId],
	[GroupId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_GroupId_StartDateTime_DidAttend' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_GroupId_StartDateTime_DidAttend] ON [dbo].[Attendance]
GO
CREATE NONCLUSTERED INDEX [IX_GroupId_StartDateTime_DidAttend] ON [dbo].[Attendance]
(
	[GroupId] ASC,
	[StartDateTime] ASC,
	[DidAttend] ASC
)
INCLUDE ( 	[Id],
	[LocationId],
	[ScheduleId],
	[CampusId],
	[PersonAliasId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_GroupRoleId' AND object_id = OBJECT_ID(N'[dbo].[GroupMember]') )
DROP INDEX [IX_GroupRoleId] ON [dbo].[GroupMember]
GO
CREATE NONCLUSTERED INDEX [IX_GroupRoleId] ON [dbo].[GroupMember]
(
	[GroupRoleId] ASC
)
INCLUDE ( 	[Id],
	[GroupId],
	[PersonId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_PersonId' AND object_id = OBJECT_ID(N'[dbo].[GroupMember]') )
DROP INDEX [IX_PersonId] ON [dbo].[GroupMember]
GO
CREATE NONCLUSTERED INDEX [IX_PersonId] ON [dbo].[GroupMember]
(
	[PersonId] ASC
)
INCLUDE ( 	[Id],
	[GroupId],
	[GroupRoleId],
	[GroupMemberStatus]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_Id' AND object_id = OBJECT_ID(N'[dbo].[PersonAlias]') )
DROP INDEX [IX_Id] ON [dbo].[PersonAlias]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Id] ON [dbo].[PersonAlias]
(
	[Id] ASC
)
INCLUDE ( 	[PersonId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO







