CREATE TABLE [_church_ccv_Datamart_EraLoss](
    [Id] [int] not null identity(1,1),
	[FamilyId] [int] NOT NULL,
	[LossDate] [datetime] NOT NULL,
	[Processed] [bit] NOT NULL DEFAULT ((0)),
	[SendEmail] [bit] NOT NULL DEFAULT ((0)),
	[Sent] [bit] NOT NULL DEFAULT ((0)),
    [Guid] uniqueidentifier not null DEFAULT NEWID(),
    [ForeignId] nvarchar(100) null
 CONSTRAINT [PK_church_ccv_ERALoss] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

insert into [_church_ccv_Datamart_EraLoss]
SELECT [FamilyId]
      ,[LossDate]
      ,[Processed]
      ,[SendEmail]
      ,[Sent]
      ,newid()
      ,null
  FROM [_church_ccv_ERALosses]


drop table [_church_ccv_ERALosses]


CREATE NONCLUSTERED INDEX [IDX_FamilyId] ON [_church_ccv_Datamart_EraLoss]
(
	[FamilyId] ASC
)

