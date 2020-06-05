/********************************************************
Name:           Test 1 - Person Allowed on Page.
Inputs:         None
Output:         Table of affected page where a Person is allowed authorization to View, Edit, Administrate, etc on a page.
Prerequisites:  Stored procedure: GetAuthorizationByPage
Created by:     Robert Jones
Date:           January 7, 2020
**********************************************************/

declare @page_id char( 11 )

DECLARE @tt2 as TABLE
(
	Id INT --IDENTITY(1, 1) primary key 
	, [PageName] NVARCHAR(200)
	, [PageId] int
	, [AuthId] int
	, [Order] int
	, [Action] NVARCHAR(100)
	, [AllowOrDeny] NVARCHAR(20)
	, [SpecialRole] NVARCHAR(50)
	, [GroupName] NVARCHAR(200)
	, [GroupId] NVARCHAR(50)
	, [PersonName] NVARCHAR(200)
	, [PersonId] NVARCHAR(50)
	, [Path] NVARCHAR(520)
);

Declare @results as TABLE
(
	--Id INT --IDENTITY(1,1) primary key
	[AffectedPageId] NVARCHAR(11)
	, [SourcePageName] NVARCHAR(200)
	, [SourcePageId] int
	, [AuthId] int
	, [Order] int
	, [Action] NVARCHAR(100)
	, [AllowOrDeny] NVARCHAR(20)
	, [SpecialRole] NVARCHAR(50)
	, [GroupName] NVARCHAR(200)
	, [GroupId] NVARCHAR(50)
	, [PersonName] NVARCHAR(200)
	, [PersonId] NVARCHAR(50)
	, [Path] NVARCHAR(520)
);

select @page_id = min( Id ) from Page

while @page_id is not null
begin

	INSERT INTO @tt2
	EXEC GetAuthorizationsByPage @page_id

	INSERT INTO @results
		(
			AffectedPageId, SourcePageName, SourcePageId, AuthId, [Order], [Action], AllowOrDeny, SpecialRole, GroupName, GroupId, PersonName, PersonId, [Path]
		)
		SELECT @page_id, t.PageName, t.PageId, t.AuthId, t.[Order], t.[Action], t.AllowOrDeny, t.SpecialRole, t.GroupName, t.GroupId, t.PersonName, t.PersonId, t.[Path]
		FROM @tt2 t
		WHERE t.PersonId > 0

	DELETE FROM @tt2

    select @page_id = min( p.Id ) from [Page] p where p.Id > @page_id
end


SELECT * FROM @results