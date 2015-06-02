sp_rename '_church_ccv_Datamart_Person'
    ,'_church_ccv_Datamart_Person_orig'

CREATE TABLE [_church_ccv_Datamart_Person](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[FamilyId] [int] NOT NULL,
	[PersonGuid] [uniqueidentifier] NOT NULL,
	[FirstName] [varchar](50) NULL,
	[NickName] [varchar](50) NULL,
	[MiddleName] [varchar](50) NULL,
	[LastName] [varchar](50) NOT NULL,
	[FullName] [varchar](100) NULL,
	[Age] [int] NULL,
	[Grade] [int] NULL,
	[BirthDate] [date] NULL,
	[Gender] [varchar](10) NULL,
	[MaritalStatus] [varchar](15) NULL,
	[FamilyRole] [varchar](50) NULL,
	[Campus] [varchar](50) NULL,
	[CampusId] [int] NULL,
	[ConnectionStatus] [varchar](50) NULL,
	[AnniversaryDate] [date] NULL,
	[AnniversaryYears] [int] NULL,
	[NeighborhoodId] [int] NULL,
	[NeighborhoodName] [varchar](100) NULL,
	[TakenStartingPoint] [bit] NULL,
	[StartingPointDate] [date] NULL,
	[InNeighborhoodGroup] [bit] NULL,
	[NeighborhoodGroupId] [int] NULL,
	[NeighborhoodGroupName] [varchar](100) NULL,
	[NearestGroupId] [int] NULL,
	[NearestGroupName] [varchar](100) NULL,
	[FirstVisitDate] [date] NULL,
	[IsStaff] [bit] NULL,
	[PhotoUrl] [varchar](150) NULL,
	[IsServing] [bit] NULL,
	[IsEra] [bit] NULL,
	[ServingAreas] [varchar](350) NULL,
	[CreatedDateTime] [datetime] NULL,
	[ModifiedDateTime] [datetime] NULL,
	[CreatedByPersonAliasId] [int] NULL,
	[ModifiedByPersonAliasId] [int] NULL,
	[SpouseName] [varchar](50) NULL,
	[IsHeadOfHousehold] [bit] NOT NULL,
	[Address] [varchar](150) NULL,
	[City] [varchar](50) NULL,
	[State] [varchar](50) NULL,
	[PostalCode] [varchar](50) NULL,
	[Email] [varchar](150) NULL,
	[HomePhone] [varchar](50) NULL,
	[CellPhone] [varchar](50) NULL,
	[WorkPhone] [varchar](50) NULL,
	[IsBaptized] [bit] NOT NULL,
	[BaptismDate] [date] NULL,
	[LastContributionDate] [date] NULL,
	[2015Contrib] [money] NULL,
	[2014Contrib] [money] NULL,
	[2013Contrib] [money] NULL,
	[2012Contrib] [money] NULL,
	[2011Contrib] [money] NULL,
	[2010Contrib] [money] NULL,
	[2009Contrib] [money] NULL,
	[2008Contrib] [money] NULL,
	[2007Contrib] [money] NULL,
	[GeoPoint] [geography] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[Guid] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[ForeignId] [nvarchar](100) NULL,
	[ViewedCount] [int] NULL,
 CONSTRAINT [PK__church_ccv_Datamart_Person] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

insert into [_church_ccv_Datamart_Person]
SELECT
     [PersonId]		
    ,[FamilyId]
    ,[PersonGuid]
    ,[FirstName]
    ,[NickName]
    ,[MiddleName]
    ,[LastName]
    ,[FullName]
    ,[Age]
    ,[Grade]
    ,[BirthDate]
    ,[Gender]
    ,[MaritalStatus]
    ,[FamilyRole]
    ,[Campus]
    ,[CampusId]
    ,[ConnectionStatus]
    ,[AnniversaryDate]
    ,[AnniversaryYears]
    ,[NeighborhoodId]
    ,[NeighborhoodName]
    ,[TakenStartingPoint]
    ,[StartingPointDate]
    ,[InNeighborhoodGroup]
    ,[NeighborhoodGroupId]
    ,[NeighborhoodGroupName]
    ,[NearestGroupId]
    ,[NearestGroupName]
    ,[FirstVisitDate]
    ,[IsStaff]
    ,[PhotoUrl]
    ,[IsServing]
    ,[IsEra]
    ,[ServingAreas]
    ,[CreatedDateTime]
    ,[ModifiedDateTime]
    ,[CreatedByPersonAliasId]
    ,[ModifiedByPersonAliasId]
    ,[SpouseName]
    ,CASE [IsHeadOfHousehold]
        WHEN 'Yes'
            THEN 1
        ELSE 0
        END [IsHeadOfHousehold]
    ,[Address]
    ,[City]
    ,[State]
    ,[PostalCode]
    ,[Email]
    ,[HomePhone]
    ,[CellPhone]
    ,[WorkPhone]
    ,CASE [IsBaptized]
        WHEN 'Yes'
            THEN 1
        ELSE 0
        END [IsBaptized]
    ,[BaptismDate]
    ,[LastContributionDate]
    ,[2015Contrib]
    ,[2014Contrib]
    ,[2013Contrib]
    ,[2012Contrib]
    ,[2011Contrib]
    ,[2010Contrib]
    ,[2009Contrib]
    ,[2008Contrib]
    ,[2007Contrib]
    ,[GeoPoint]
    ,[Latitude]
    ,[Longitude]
    ,[Guid]
    ,[ForeignId]
    ,[ViewedCount]
from [_church_ccv_Datamart_Person_orig]

drop table [_church_ccv_Datamart_Person_orig]

CREATE NONCLUSTERED INDEX [IDX_PersonId] ON [_church_ccv_Datamart_Person]
(
	[PersonId] ASC
)


