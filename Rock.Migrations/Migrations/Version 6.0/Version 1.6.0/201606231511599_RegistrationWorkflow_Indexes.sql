-- Add back defined value indexes that were removed
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_QualifierValueId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_QualifierValueId] ON [dbo].[Attendance]
GO

CREATE NONCLUSTERED INDEX [IX_QualifierValueId] ON [dbo].[Attendance] ( [QualifierValueId] ASC )
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_SearchTypeValueId' AND object_id = OBJECT_ID(N'[dbo].[Attendance]') )
DROP INDEX [IX_SearchTypeValueId] ON [dbo].[Attendance]
GO

CREATE NONCLUSTERED INDEX [IX_SearchTypeValueId] ON [dbo].[Attendance] ( [SearchTypeValueId] ASC )
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_LocationTypeValueId' AND object_id = OBJECT_ID(N'[dbo].[GroupTypeLocationType]') )
DROP INDEX [IX_LocationTypeValueId] ON [dbo].[GroupTypeLocationType]
GO

CREATE NONCLUSTERED INDEX [IX_LocationTypeValueId] ON [dbo].[GroupTypeLocationType] ( [LocationTypeValueId] ASC )
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
