-- History table indexe changes
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_CreatedByPersonAliasId' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_CreatedByPersonAliasId] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_EntityTypeId' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_EntityTypeId] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignGuid' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_ForeignGuid] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignId' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_ForeignId] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ForeignKey' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_ForeignKey] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_Guid' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_Guid] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_ModifiedByPersonAliasId' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_RelatedEntityTypeId' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_RelatedEntityTypeId] ON [dbo].[History]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_RelatedEntityTypeId_RelatedEntityId' AND object_id = OBJECT_ID(N'[dbo].[History]') )
DROP INDEX [IX_RelatedEntityTypeId_RelatedEntityId] ON [dbo].[History]
GO

CREATE NONCLUSTERED INDEX [IX_RelatedEntityTypeId_RelatedEntityId] ON [dbo].[History]
(
	[RelatedEntityTypeId] ASC,
	[RelatedEntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

-- Financial Transaction Detail
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_EntityTypeId' AND object_id = OBJECT_ID(N'[dbo].[FinancialTransactionDetail]') )
DROP INDEX [IX_EntityTypeId] ON [dbo].[FinancialTransactionDetail]
GO

-- Financial Scheduled Transaction Detail
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_EntityTypeId' AND object_id = OBJECT_ID(N'[dbo].[FinancialScheduledTransactionDetail]') )
DROP INDEX [IX_EntityTypeId] ON [dbo].[FinancialScheduledTransactionDetail]
GO

IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_EntityTypeId_EntityId' AND object_id = OBJECT_ID(N'[dbo].[FinancialScheduledTransactionDetail]') )
DROP INDEX [IX_EntityTypeId_EntityId] ON [dbo].[FinancialScheduledTransactionDetail]
GO

CREATE NONCLUSTERED INDEX [IX_EntityTypeId_EntityId] ON [dbo].[FinancialScheduledTransactionDetail]
(
	[EntityTypeId] ASC,
	[EntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

-- Group Member Indexes
IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_GroupId_PersonId_GroupRoleId' AND object_id = OBJECT_ID(N'[dbo].[GroupMember]') )
DROP INDEX [IX_GroupId_PersonId_GroupRoleId] ON [dbo].[GroupMember]
GO

CREATE NONCLUSTERED INDEX [IX_GroupId_PersonId_GroupRoleId] ON [dbo].[GroupMember]
(
	[GroupId] ASC, 
	[PersonId] ASC, 
	[GroupRoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO