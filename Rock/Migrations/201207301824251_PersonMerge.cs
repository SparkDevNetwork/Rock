namespace Rock.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class PersonMerge : DbMigration
    {
        public override void Up()
        {
            Sql( @"
CREATE PROC [dbo].[crmPerson_sp_Merge]
@OldId int, 
@NewId int

AS

DECLARE @OldGuid uniqueidentifier
DECLARE @NewGuid uniqueidentifier

SET @OldGuid = (SELECT [Guid] FROM crmPerson WHERE [Id] = @OldId)
SET @NewGuid = (SELECT [Guid] FROM crmPerson WHERE [Id] = @NewId)

IF @OldGuid IS NOT NULL AND @NewGuid IS NOT NULL
BEGIN

	BEGIN TRANSACTION

	INSERT INTO crmPersonMerged ([Id], [Guid], [CurrentId], [CurrentGuid])
	VALUES (@OldId, @OldGuid, @NewId, @NewGuid)

	DECLARE @Sql varchar(max)

	DECLARE ForeignKeyCursor INSENSITIVE CURSOR FOR
	SELECT 
		' UPDATE ' + tso.name +
		' SET ' + tac.name + ' = ' + CAST(@NewId as varchar) +
		' WHERE ' + tac.name + ' = ' + CAST(@OldId as varchar) 
	FROM sys.foreign_key_columns kc
	INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
	INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
	INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
	INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
	INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id
	WHERE so.name = 'crmPerson'
	AND rac.name = 'Id'

	OPEN ForeignKeyCursor

	FETCH NEXT
	FROM ForeignKeyCursor
	INTO @Sql

	WHILE (@@FETCH_STATUS <> -1)
	BEGIN

		IF (@@FETCH_STATUS = 0)
		BEGIN

			EXEC(@Sql)
			
		END
		
		FETCH NEXT
		FROM ForeignKeyCursor
		INTO @Sql

	END

	CLOSE ForeignKeyCursor
	DEALLOCATE ForeignKeyCursor

	DELETE crmPerson
	WHERE [Id] = @OldId
	
	COMMIT TRANSACTION

END
" );
        }
        
        public override void Down()
        {

            Sql( @"
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[crmPerson_sp_Merge]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[crmPerson_sp_Merge]
" );

        }
    }
}