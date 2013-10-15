-- drop/create the function
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ufn_person_group_to_person_names]') and [type] = 'TF')
BEGIN
    DROP FUNCTION [dbo].[ufn_person_group_to_person_names];
END
GO

CREATE FUNCTION [dbo].[ufn_person_group_to_person_names] 
( 
@personId int, -- NULL means generate person names from Group Members. NOT-NULL means just get FullName from Person
@groupId int
)
RETURNS @personNamesTable TABLE ( PersonNames varchar(max))
AS
BEGIN
   declare @personNames varchar(max); 

    if (@personId is not null) 
    begin
      select @personNames = FullName from Person where Id = @personId;
    end
    else
    begin
      set @personNames = 'hello!'  
         --select  
    end

    insert into @personNamesTable ( [PersonNames] ) values ( @personNames);

  return
END
GO
