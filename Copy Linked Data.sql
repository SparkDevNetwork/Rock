BEGIN TRANSACTION

SET NOCOUNT ON

DECLARE @LinkedServerDB varchar(128)
SET @LinkedServerDB = '[VSERVER01.CYTANIUM.COM].[sparkdevcms]'
--SET @LinkedServerDB = '[24.249.179.215].[RockChMS]'

DECLARE @ConstraintName varchar(128)
DECLARE @IsDisabled bit
DECLARE @UpdateAction varchar(128)
DECLARE @DeleteAction varchar(128)
DECLARE @TableName varchar(128)
DECLARE @ColumnName varchar(128)
DECLARE @SourceObject varchar(128)
DECLARE @RelColumn varchar(128)

DECLARE @SqlStatement varchar(max)

-- Create a temporary table for storing the SQL statements to recreate the relationships
CREATE TABLE #ConstraintTable ([create_contraint] varchar(4000))

-- Create a cursor to spin through all the relationships
DECLARE RelationshipCursor INSENSITIVE CURSOR FOR
SELECT
    k.name AS constraint_name,
    k.is_disabled,
    CASE k.update_referential_action
		WHEN 1 THEN 'CASCADE'
        WHEN 2 THEN 'SET NULL'
        ELSE 'NO ACTION' END,
    CASE k.delete_referential_action
		WHEN 1 THEN 'CASCADE'
        WHEN 2 THEN 'SET NULL'
        ELSE 'NO ACTION' END,
    tso.name,
    tac.name,
    so.name,
    rac.name
FROM sys.foreign_key_columns kc
INNER JOIN sys.foreign_keys k ON kc.constraint_object_id = k.object_id
INNER JOIN sys.all_objects so ON so.object_id = kc.referenced_object_id
INNER JOIN sys.all_columns rac ON rac.column_id = kc.referenced_column_id AND rac.object_id = so.object_id
INNER JOIN sys.all_objects tso ON tso.object_id = kc.parent_object_id
INNER JOIN sys.all_columns tac ON tac.column_id = kc.parent_column_id AND tac.object_id = tso.object_id

OPEN RelationshipCursor

FETCH NEXT
FROM RelationshipCursor
INTO @ConstraintName, @IsDisabled, @UpdateAction, @DeleteAction, @TableName, @ColumnName, @SourceObject, @RelColumn

WHILE (@@FETCH_STATUS <> -1)
BEGIN

    IF (@@FETCH_STATUS = 0)
    BEGIN

        -- Drop the relationship
        SET @SqlStatement = 'ALTER TABLE ' + @TableName + ' DROP CONSTRAINT ' + @ConstraintName
        EXEC (@SqlStatement)

        -- Create the SQL statement to recreate the relationship and save it to the temporary table
        INSERT INTO #ConstraintTable
        VALUES
        (
            'ALTER TABLE ' + @TableName + ' ADD '  + CHAR(13) + CHAR(10) +
            '    CONSTRAINT ' + @ConstraintName + ' FOREIGN KEY(' + @ColumnName + ') REFERENCES ' + @SourceObject + '(' + @RelColumn + ') ON UPDATE ' + @UpdateAction + ' ON DELETE ' + @DeleteAction + CHAR(13) + CHAR(10) +
            CHAR(13) + CHAR(10)
        )

        IF @IsDisabled = 1
        BEGIN

            INSERT INTO #ConstraintTable
            VALUES
            (
                'ALTER TABLE ' + @TableName + CHAR(13) + CHAR(10) +
                '    NOCHECK CONSTRAINT ' + @ConstraintName + CHAR(13) + CHAR(10) +
                CHAR(13) + CHAR(10)
            )
        END

    END

    FETCH NEXT
    FROM RelationshipCursor
    INTO @ConstraintName, @IsDisabled, @UpdateAction, @DeleteAction, @TableName, @ColumnName, @SourceObject, @RelColumn

END

CLOSE RelationshipCursor
DEALLOCATE RelationshipCursor

-- Create a cursor to spin through all the tables
DECLARE TableCursor INSENSITIVE CURSOR FOR
SELECT name
FROM sys.tables

OPEN TableCursor

FETCH NEXT
FROM TableCursor
INTO @TableName

WHILE (@@FETCH_STATUS <> -1)
BEGIN

    IF (@@FETCH_STATUS = 0)
    BEGIN

		-- Get the column names for the table
		DECLARE @ColNames varchar(4000)
		SET @ColNames = ''
		SELECT @ColNames = @ColNames + ', [' + COLUMN_NAME + ']'
		FROM INFORMATION_SCHEMA.COLUMNS 
		WHERE TABLE_NAME = @TableName

		DECLARE @HasIdentity bit
		IF EXISTS (
			SELECT COLUMN_NAME
			FROM INFORMATION_SCHEMA.COLUMNS 
			WHERE TABLE_NAME = @TableName
			AND COLUMNPROPERTY(object_id(TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1
		)
			SET @HasIdentity = 1
		ELSE
			SET @HasIdentity = 0
			
		-- Delete the existing records and copy all records from the linked server
		SET @SqlStatement = 'DELETE ' + @TableName + '; '
		
		IF (@HasIdentity = 1)
			SET @SqlStatement = @SqlStatement + 'SET IDENTITY_INSERT dbo.' + @TableName + ' ON; '
			
		SET @SqlStatement = @SqlStatement + 'INSERT INTO ' + @TableName + '(' + SUBSTRING(@ColNames,2,4000) + ') ' +
		'SELECT ' + SUBSTRING(@ColNames,2,4000) + ' FROM ' + @LinkedServerDB + '.dbo.' + @TableName + '; '
		
		IF (@HasIdentity = 1)
			SET @SqlStatement = @SqlStatement + 'SET IDENTITY_INSERT dbo.' + @TableName + ' OFF; '
			
		EXEC (@SqlStatement)

    END

    FETCH NEXT
    FROM TableCursor
    INTO @TableName

END

CLOSE TableCursor
DEALLOCATE TableCursor

-- Create a cursor to spin through all the create statements
DECLARE ConstraintCursor INSENSITIVE CURSOR FOR
SELECT create_contraint
FROM #ConstraintTable

OPEN ConstraintCursor

FETCH NEXT
FROM ConstraintCursor
INTO @SqlStatement

WHILE (@@FETCH_STATUS <> -1)
BEGIN

    IF (@@FETCH_STATUS = 0)
    BEGIN

        -- Execute the SQL statement to recreate the relationship.
        EXEC (@SqlStatement)

    END

    FETCH NEXT
    FROM ConstraintCursor
    INTO @SqlStatement

END

CLOSE ConstraintCursor
DEALLOCATE ConstraintCursor

DROP TABLE #ConstraintTable

-- Update the statistics on the table since all the records have been deleted
--UPDATE STATISTICS util_blob

SET NOCOUNT OFF

COMMIT TRANSACTION