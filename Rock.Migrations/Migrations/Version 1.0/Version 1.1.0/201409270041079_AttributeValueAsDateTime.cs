// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class AttributeValueAsDateTime : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the schema again for all stored procedures and functions to be 'dbo' in case anyone missed it by installing v1.0.13 before this fix
            Sql( @"
    DECLARE 
	    @SchemaName varchar(max),
	    @ObjectName varchar(max),
	    @Sql varchar(max)

    DECLARE schema_cursor CURSOR FOR
	    SELECT S.[Name], O.[Name]
	    FROM sys.all_objects O
	    INNER JOIN sys.schemas S ON S.[schema_id] = O.[schema_id]
	    WHERE O.[Type] IN ( 'FN', 'P', 'TF')
	    AND S.[name] NOT IN ('sys','dbo')

    OPEN schema_cursor 
    FETCH NEXT FROM schema_cursor INTO @SchemaName, @ObjectName

    WHILE @@FETCH_STATUS = 0
    BEGIN
	
	    BEGIN TRY
		    SELECT @SQL = 'ALTER SCHEMA dbo TRANSFER [' + @SchemaName  + '].[' + @ObjectName + ']'
		    EXEC (@SQL)
        END TRY
        BEGIN CATCH
        END CATCH
    
	    FETCH NEXT FROM schema_cursor INTO @SchemaName, @ObjectName

    END

    CLOSE schema_cursor
    DEALLOCATE schema_cursor

" );

            Sql( @"alter table AttributeValue add ValueAsDateTime as case when ISDATE( value) = 1 then convert(datetime, value) else null end" );
            Sql( @"alter table AttributeValue add ValueAsNumeric as case when (ISNUMERIC( value) = 1 and value not like '%[^0-9.]%') then convert(numeric(38,10), value ) else null end" );

            Sql( @"
-- Step 1) Change all Business records to store BusinessName in the LastName field instead of the FirstName/NickName field
UPDATE [Person]
SET [LastName] = [FirstName]
WHERE isnull([LastName], '') = ''
    AND [RecordTypeValueId] = (
        SELECT TOP 1 [Id]
        FROM [DefinedValue]
        WHERE [Guid] = 'BF64ADD3-E70A-44CE-9C4B-E76BBED37550' /* RecordType Business */
        )

-- Step 2) Clean up Business records that have non-null FirstName,NickName values
UPDATE [Person]
SET [FirstName] = NULL
    ,[NickName] = NULL
WHERE [RecordTypeValueId] = (
        SELECT TOP 1 [Id]
        FROM [DefinedValue]
        WHERE [Guid] = 'BF64ADD3-E70A-44CE-9C4B-E76BBED37550' /* RecordType Business */
        )
" );
            // add page route for PersonDuplicateDetail page so that there is a known route for things that want to check for duplicates of a person (like the Pending Person Report)
            RockMigrationHelper.AddPageRoute( "6F9CE971-75DF-4F2A-BD5E-A12B149A442E", "PersonDuplicate/{PersonId}" );

            // Ensure Rock.Reporting.DataSelect.LiquidSelect has a known Guid
            RockMigrationHelper.UpdateEntityType( "Rock.Reporting.DataSelect.LiquidSelect", "Liquid Select", "Rock.Reporting.DataSelect.LiquidSelect, Rock, Version=1.0.14.0, Culture=neutral, PublicKeyToken=null", false, true, Rock.SystemGuid.EntityType.REPORTING_DATASELECT_LIQUIDSELECT );

            // add 'Find Duplicates' liquid field to the Pending Persons's report
            RockMigrationHelper.DeleteReportField( "7A4C4C85-61C9-4378-9CDE-3F38516FB7DE" );
            RockMigrationHelper.AddReportField(
                "4E3ECAE0-9D36-4C22-994D-AD31DE0F6FB7",
                Model.ReportFieldType.DataSelectComponent,
                true,
                Rock.SystemGuid.EntityType.REPORTING_DATASELECT_LIQUIDSELECT,
                @"<a class='btn btn-default' href='/PersonDuplicate/{{ Id }}' title='Find Duplicates'><i class='fa fa-search-plus'></i></a>",
                2,
                "Action",
                "7A4C4C85-61C9-4378-9CDE-3F38516FB7DE" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteReportField( "7A4C4C85-61C9-4378-9CDE-3F38516FB7DE" );

            DropColumn( "dbo.AttributeValue", "ValueAsDateTime" );
            DropColumn( "dbo.AttributeValue", "ValueAsNumeric" );
        }
    }
}
