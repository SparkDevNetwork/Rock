
-- Migrations

-- Create table for Adult Totals 
CREATE TABLE [dbo]._church_ccv_Actions_History_Adult( 
 [Id] int IDENTITY(1,1) NOT NULL
,[Date] date
,[CampusId] int NOT NULL
,[RegionId] int NOT NULL
,[TotalPeople] int NOT NULL
,[Baptised] int NOT NULL
,[ERA] int NOT NULL
,[Give] int NOT NULL
,[Member] int NOT NULL
,[StartingPoint] int NOT NULL
,[Serving] int NOT NULL
,[PeerLearning] int NOT NULL
,[Mentored] int NOT NULL
,[Teaching] int NOT NULL
,[PeerLearning_NH] int NOT NULL
,[PeerLearning_YA] int NOT NULL
,[Mentored_NH] int NOT NULL
,[Mentored_YA] int NOT NULL
,[Mentored_NS] int NOT NULL
,[Teaching_NH_SubSection] int NOT NULL
,[Teaching_NH] int NOT NULL
,[Teaching_YA_Section] int NOT NULL
,[Teaching_YA] int NOT NULL
,[Teaching_NS_SubSection] int NOT NULL
,[Teaching_NS] int NOT NULL
,[Teaching_NG_Section] int NOT NULL
,[Teaching_NG] int NOT NULL
,CreatedDateTime datetime
,ModifiedDateTime datetime
,CreatedByPersonAliasId datetime
,ModifiedByPersonAliasId datetime
,[Guid] uniqueidentifier NOT NULL
,ForeignId int
,ForeignGuid uniqueidentifier
,ForeignKey varchar(100)
    CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryAdult] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )
)

-- Create table for Adult Individuals
CREATE TABLE [dbo]._church_ccv_Actions_History_Adult_Person( 
  [Id] int IDENTITY(1,1) NOT NULL,
  [Date] date NOT NULL, 
  PersonAliasId int NOT NULL, 
  Baptised date, 
  ERA bit NOT NULL, 
  Give bit NOT NULL, 
  Member date, 
  StartingPoint date,
  Serving bit NOT NULL, 
  PeerLearning bit NOT NULL, 
  Mentored bit NOT NULL, 
  Teaching bit NOT NULL, 

  PeerLearning_NH bit NOT NULL, 
  PeerLearning_YA bit NOT NULL, 
  Mentored_NH bit NOT NULL,
  Mentored_YA bit NOT NULL,
  Mentored_NS bit NOT NULL,
  Teaching_NH_SubSection bit NOT NULL,
  Teaching_NH bit NOT NULL,
  Teaching_YA_Section bit NOT NULL,
  Teaching_YA bit NOT NULL,
  Teaching_NS_SubSection bit NOT NULL,
  Teaching_NS bit NOT NULL,
  Teaching_NG_Section bit NOT NULL,
  Teaching_NG bit NOT NULL,
  
  CreatedDateTime datetime,
  ModifiedDateTime datetime,
  CreatedByPersonAliasId datetime,
  ModifiedByPersonAliasId datetime,
  [Guid] uniqueidentifier NOT NULL,
  ForeignId int,
  ForeignGuid uniqueidentifier,
  ForeignKey varchar(100)
  CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryAdultPerson] PRIMARY KEY CLUSTERED 
  (
	[Id] ASC
  )
)



-- Create table for Student Totals
CREATE TABLE [dbo]._church_ccv_Actions_History_Student( 
 [Id] int IDENTITY(1,1) NOT NULL
,[Date] date
,[CampusId] int NOT NULL
,[RegionId] int NOT NULL
,[TotalPeople] int NOT NULL
,[Baptised] int  NOT NULL
,[ERA] int NOT NULL
,[Give] int NOT NULL
,[Member] int NOT NULL
,[StartingPoint] int NOT NULL
,[Serving] int NOT NULL
,[PeerLearning] int NOT NULL
,[Mentored] int NOT NULL
,[Teaching] int NOT NULL
,[PeerLearning_NG] int NOT NULL
,[Mentored_NG] int NOT NULL
,[Teaching_Undefined] int NOT NULL
,CreatedDateTime datetime
,ModifiedDateTime datetime
,CreatedByPersonAliasId datetime
,ModifiedByPersonAliasId datetime
,[Guid] uniqueidentifier NOT NULL
,ForeignId int
,ForeignGuid uniqueidentifier
,ForeignKey varchar(100)
	CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryStudent] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )
)

-- Create table for Student Individuals
  CREATE TABLE [dbo]._church_ccv_Actions_History_Student_Person( 
  [Id] int IDENTITY(1,1) NOT NULL,
  [Date] date NOT NULL, 
  PersonAliasId int NOT NULL, 

  Baptised date, 
  ERA bit NOT NULL, 
  Give bit NOT NULL, 
  Member date, 
  StartingPoint date,
  Serving bit NOT NULL, 
  PeerLearning bit NOT NULL, 
  Mentored bit NOT NULL, 
  Teaching bit NOT NULL, 

  PeerLearning_NG bit NOT NULL, 
  Mentored_NG bit NOT NULL,
  Teaching_Undefined bit NOT NULL,
  
  CreatedDateTime datetime,
  ModifiedDateTime datetime,
  CreatedByPersonAliasId datetime,
  ModifiedByPersonAliasId datetime,
  [Guid] uniqueidentifier NOT NULL,
  ForeignId int,
  ForeignGuid uniqueidentifier,
  ForeignKey varchar(100)
  CONSTRAINT [PK_dbo._church_ccv_ActionsHistoryStudentPerson] PRIMARY KEY CLUSTERED 
    (
	    [Id] ASC
    )
)