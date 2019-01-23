@echo off
cls
REM --------------------------------------------------------------------------
REM Script to rapidly create a copy of a database from a template .bacpac file.
REM --------------------------------------------------------------------------

REM --------------------------------------------------------------------------
REM Usage
REM 
REM 1) Verify SQLPCKDIR is the folder where your SQL data tools are installed.
REM 2) Verify/put a database .bacpac file called TEMPLATE.bacpac
REM    under this script's Data\ folder. Recommend the .bacpac be a Rock db ~v7 with
REM    sample data preloaded.
REM 3) Verify SERVERNAME is the name of your SQL server ("." works for a local
REM    server.)
REM 4) Run me without arguments and enter the name of a new database you wish to create.
REM --------------------------------------------------------------------------

IF NOT "%1" == "" GOTO usage:

REM --------------------------------------------------------------------------
REM Settings
REM --------------------------------------------------------------------------

REM This is the directory where the SQL data tools are installed
SET SQLPCKDIR=C:\Program Files (x86)\Microsoft SQL Server\120\DAC\bin\

REM The .bacpac file template
SET PACNAME=TEMPLATE.bacpac

REM Your SQL Server
SET SERVERNAME=.

REM --------------------------------------------------------------------------
REM Script...
REM --------------------------------------------------------------------------

SET ROCKWEBFOLDER="%~dp0\..\..\RockWeb"
SET SQLPCK="%SQLPCKDIR%SqlPackage.exe"
SET DBFILE=%~dp0\Data\%PACNAME%

IF NOT EXIST %SQLPCK% GOTO sqltoolnotfound:

IF NOT EXIST "%DBFILE%" GOTO bacpacnotfound:

cd /D %ROCKWEBFOLDER%

SET /p bn="Enter database name to create: "

REM The name of the database to import
SET DBNAME=%bn%

echo [96mrestoring %PACNAME% as new database called [0m [93m%DBNAME%[0m

echo ...using: [34m %SQLPCK% /a:Import /sf:"%DBFILE%" /tdn:%DBNAME% /tsn:%SERVERNAME% [0m

echo [90m
%SQLPCK% /a:Import /sf:"%DBFILE%" /tdn:%DBNAME% /tsn:%SERVERNAME%
IF %errorlevel% NEQ 0 GOTO :error
echo [0m

echo [92m...done.[0m

REM Add RockUser as dbo
echo [96mAdding RockUser as db_owner... [0m
sqlcmd -s %SERVERNAME% -d %DBNAME% -Q "CREATE USER [RockUser] FROM LOGIN [RockUser];"
sqlcmd -s %SERVERNAME% -d %DBNAME% -Q "exec sp_addrolemember 'db_owner', 'RockUser';"
echo [92m...done.[0m

REM Create an empty Run.Migration in the App_Data folder
echo [96mAdding Run.Migration file in App_Data... [0m
CD /D %ROCKWEBFOLDER%\App_Data

echo > Run.Migration
echo [92m...done.[0m


GOTO :done

REM -------------------------------------------------------------------------
REM error and exit functions...
REM -------------------------------------------------------------------------
:sqltoolnotfound
echo [0m
echo [91mSomething is not right... SqlPackage.exe not found. Are you sure you have Microsoft SQL Server installed? If so, check the location in this scripts settings.[0m
EXIT /B 1

:bacpacnotfound
echo [0m
echo [93mNo .bacpac template file found to import.  Did you put one under the Data folder? It should be called %DBFILE%[0m
EXIT /B 1

:error
echo [0m
echo [91mFailed to import bacpac file[0m
EXIT /B 1

:usage
echo [0m[96m
echo Usage: %0
echo [0m[96m
echo  1) Verify SQLPCKDIR is the folder where your SQL data tools are installed.
echo  2) Verify/put a database .bacpac file called TEMPLATE.bacpac
echo     under this scripts Data\ folder. Recommend the .bacpac be a Rock db ~v7 with
echo     sample data preloaded.
echo  3) Verify SERVERNAME is the name of your SQL server ("." works for a local
echo     server.)
echo  4) Run me without arguments and enter the name of a new database you wish to create.
echo [0m

EXIT /B 1



:done
ENDLOCAL
REM PAUSE