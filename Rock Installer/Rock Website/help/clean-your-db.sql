/*            _    _  ___  ______ _   _ _____ _   _ _____         
             | |  | |/ _ \ | ___ \ \ | |_   _| \ | |  __ \        
             | |  | / /_\ \| |_/ /  \| | | | |  \| | |  \/        
             | |/\| |  _  ||    /| . ` | | | | . ` | | __         
      _ _ _  \  /\  / | | || |\ \| |\  |_| |_| |\  | |_\ \  _ _ _ 
     (_|_|_)  \/  \/\_| |_/\_| \_\_| \_/\___/\_| \_/\____/ (_|_|_)

** DANGER ** DANGER ** DANGER ** DANGER ** DANGER ** DANGER ** DANGER **

Running this script will delete EVERYTHING out of your database! Only use
this script if you know EXACTLY what you are doing.  We are providing it
as a service to help you clean out your database.  Please DON'T SHOOT YOURSELF
IN THE FOOT with it. If you are the least bit unsure what what is going on
please walk away from the computer now.

YOU HAVE BEEN WARNED!!!

*/




/* Drop all non-system stored procs */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 ORDER BY [name])

WHILE @name is not null
BEGIN
    SELECT @SQL = 'DROP PROCEDURE [' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Procedure: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all views */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP VIEW [' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped View: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all functions */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP FUNCTION [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Function: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all Foreign Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)

WHILE @name is not null
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint IS NOT NULL
    BEGIN
        SELECT @SQL = 'ALTER TABLE [' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint) +']'
        EXEC (@SQL)
        PRINT 'Dropped FK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all Primary Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)

WHILE @name IS NOT NULL
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint is not null
    BEGIN
        SELECT @SQL = 'ALTER TABLE [' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint)+']'
        EXEC (@SQL)
        PRINT 'Dropped PK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all tables */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP TABLE [' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Table: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO