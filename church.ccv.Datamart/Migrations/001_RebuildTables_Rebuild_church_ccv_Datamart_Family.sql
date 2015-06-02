sp_rename '_church_ccv_Datamart_Family'
    ,'_church_ccv_Datamart_Family_orig'

CREATE TABLE [_church_ccv_Datamart_Family](
    [Id] [int] IDENTITY(1,1) NOT NULL,
	[FamilyId] [int] NOT NULL,
	[FamilyName] [varchar](100) NULL,
	[HHPersonId] [int] NULL,
	[HHFirstName] [varchar](50) NULL,
	[HHNickName] [varchar](50) NULL,
	[HHLastName] [varchar](50) NULL,
	[HHFullName] [varchar](100) NULL,
	[HHGender] [varchar](50) NULL,
	[HHMemberStatus] [varchar](50) NULL,
	[HHMaritalStatus] [varchar](50) NULL,
	[HHFirstVisit] [datetime] NULL,
	[HHFirstActivity] [datetime] NULL,
	[HHAge] [int] NULL,
	[NeighborhoodId] [int] NULL,
	[NeighborhoodName] [nvarchar](100) NULL,
	[InNeighborhoodGroup] [bit] NULL,
	[IsEra] [bit] NULL,
	[NearestNeighborhoodGroupName] [nvarchar](100) NULL,
	[NearestNeighborhoodGroupId] [int] NULL,
	[IsServing] [bit] NULL,
	[Attendance16Week] [int] NULL,
	[ConnectionStatus] [nvarchar](50) NULL,
	[Email] [varchar](100) NULL,
	[HomePhone] [varchar](50) NULL,
	[AdultCount] [int] NULL,
	[ChildCount] [int] NULL,
	[LocationId] [int] NULL,
	[Address] [varchar](200) NULL,
	[City] [varchar](50) NULL,
	[State] [varchar](50) NULL,
	[Country] [varchar](50) NULL,
	[PostalCode] [varchar](50) NULL,
	[GeoPoint] [geography] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[Campus] [varchar](50) NULL,
	[AdultNames] [varchar](200) NULL,
	[ChildNames] [varchar](2000) NULL,
	[2015Contrib] [money] NULL,
	[2014Contrib] [money] NULL,
	[2013Contrib] [money] NULL,
	[2012Contrib] [money] NULL,
	[2011Contrib] [money] NULL,
	[2010Contrib] [money] NULL,
	[2009Contrib] [money] NULL,
	[2008Contrib] [money] NULL,
	[2007Contrib] [money] NULL,
	[Guid] [uniqueidentifier] NOT NULL DEFAULT (newid()),
	[ForeignId] [nvarchar](100) NULL,
 CONSTRAINT [PK__church_ccv_Datamart_Family] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
)

insert into [_church_ccv_Datamart_Family]
SELECT [FamilyId]
      ,[FamilyName]
      ,[HHPersonId]
      ,[HHFirstName]
      ,[HHNickName]
      ,[HHLastName]
      ,[HHFullName]
      ,[HHGender]
      ,[HHMemberStatus]
      ,[HHMaritalStatus]
      ,[HHFirstVisit]
      ,[HHFirstActivity]
      ,[HHAge]
      ,[NeighborhoodId]
      ,[NeighborhoodName]
      ,[InNeighborhoodGroup]
      ,[IsEra]
      ,[NearestNeighborhoodGroupName]
      ,[NearestNeighborhoodGroupId]
      ,[IsServing]
      ,[Attendance16Week]
      ,[ConnectionStatus]
      ,[Email]
      ,[HomePhone]
      ,[AdultCount]
      ,[ChildCount]
      ,[LocationId]
      ,[Address]
      ,[City]
      ,[State]
      ,[Country]
      ,[PostalCode]
      ,[GeoPoint]
      ,[Latitude]
      ,[Longitude]
      ,[Campus]
      ,[AdultNames]
      ,[ChildNames]
      ,[2015Contrib]
      ,[2014Contrib]
      ,[2013Contrib]
      ,[2012Contrib]
      ,[2011Contrib]
      ,[2010Contrib]
      ,[2009Contrib]
      ,[2008Contrib]
      ,[2007Contrib]
      ,[Guid]
      ,[ForeignId]
  FROM [_church_ccv_Datamart_Family_orig]

  drop table [_church_ccv_Datamart_Family_orig]

  CREATE NONCLUSTERED INDEX [IDX_FamilyId] ON [_church_ccv_Datamart_Family]
(
	[FamilyId] ASC
)