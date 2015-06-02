sp_rename '_church_ccv_Datamart_NearestGroup', '_church_ccv_Datamart_NearestGroup_orig'

CREATE TABLE [_church_ccv_Datamart_NearestGroup](
	[Id] [int] IDENTITY(1,1) NOT NULL,
    [FamilyLocationId] [int] NOT NULL,
	[GroupLocationId] [int] NOT NULL,
	[Distance] [float] NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[ForeignId] [nvarchar](100) NULL,
 CONSTRAINT [PK__church_ccv_Datamart_NearestGroup] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

insert into [_church_ccv_Datamart_NearestGroup]
SELECT [FamilyLocationId]
      ,[GroupLocationId]
      ,[Distance]
      ,[Guid]
      ,[ForeignId]
  FROM [_church_ccv_Datamart_NearestGroup_orig]

drop table [_church_ccv_Datamart_NearestGroup_orig]



