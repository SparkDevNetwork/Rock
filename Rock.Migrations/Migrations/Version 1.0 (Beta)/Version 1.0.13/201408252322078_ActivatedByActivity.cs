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
    public partial class ActivatedByActivity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update the schema for all stored procedures and functions to be 'dbo'
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

            #region Add Activated by Activity property

            AddColumn("dbo.WorkflowActivity", "ActivatedByActivityId", c => c.Int());
            CreateIndex("dbo.WorkflowActivity", "ActivatedByActivityId");
            AddForeignKey("dbo.WorkflowActivity", "ActivatedByActivityId", "dbo.WorkflowActivity", "Id");

            #endregion

            #region Convert Safe Sender Domains to Defined Type

            RockMigrationHelper.AddDefinedType_pre201409101843015( "Communication", "Safe Sender Domains", @"
Are the domains that can be used to send emails.  If an Email communication is created with a From Address that is not from 
one of these domains, the Organization Email global attribute value will be used instead for the From Address and the original 
value will be used as the Reply To address.  This is to help reduce the likelihood of communications being rejected by the 
receiving email servers.", "DB91D0E9-DCA6-45A9-8276-AEF032BE8AED" );
            RockMigrationHelper.AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Safe Sender Domains", "", "B90576B0-110E-4DC0-8EB8-4668C5238508", "fa fa-check" );
            RockMigrationHelper.AddBlock( "B90576B0-110E-4DC0-8EB8-4668C5238508", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "74583AD9-36F5-44CE-9346-AA009440A49A" );
            RockMigrationHelper.AddBlockAttributeValue( "74583AD9-36F5-44CE-9346-AA009440A49A", "0305EF98-C791-4626-9996-F189B9BB674C", @"DB91D0E9-DCA6-45A9-8276-AEF032BE8AED" );
            RockMigrationHelper.AddBlock( "B90576B0-110E-4DC0-8EB8-4668C5238508", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "840878A8-6CFF-4A9E-B9E7-48D33E37959C" );
            RockMigrationHelper.AddBlockAttributeValue( "840878A8-6CFF-4A9E-B9E7-48D33E37959C", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"DB91D0E9-DCA6-45A9-8276-AEF032BE8AED" );

            Sql( @"
    DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'DB91D0E9-DCA6-45A9-8276-AEF032BE8AED' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'CDD29C51-5D33-435F-96AB-2C06BA772F88' )
    DECLARE @Domains varchar(8000) = ( SELECT TOP 1 [Value] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId )
    DECLARE @Domain varchar(1000)
    WHILE LEN(@Domains) > 0
    BEGIN
        SET @Domain= LEFT(@Domains, ISNULL(NULLIF(CHARINDEX(',', @Domains) - 1, -1), LEN(@Domains)))
        SET @Domains = SUBSTRING(@Domains,ISNULL(NULLIF(CHARINDEX(',', @Domains), 0), LEN(@Domains)) + 1, LEN(@Domains))
		INSERT INTO [DefinedValue] ( [IsSystem], [DefinedTypeId], [Order], [Value], [Guid] )
        SELECT 0, @DefinedTypeId, 0, @Domain, NEWID()
    END
" );
            RockMigrationHelper.DeleteAttribute( "CDD29C51-5D33-435F-96AB-2C06BA772F88" );

            #endregion

            #region Workflow Action Rename

            Sql( @"
    UPDATE [EntityType] SET [FriendlyName] = 'Send SMS'
    WHERE [Name] = 'Rock.Workflow.Action.SendSms'

    UPDATE [EntityType]	SET [FriendlyName] = 'Set Workflow Name'
	WHERE [Name] = 'Rock.Workflow.Action.SetName'

    UPDATE [EntityType]	SET [FriendlyName] = 'Assign Activity From Attribute Value'
	WHERE [Name] = 'Rock.Workflow.Action.AssignActivityToAttributeValue'
" );
            #endregion

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.AddGlobalAttribute( "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "", "", "Safe Sender Domains", "Delimited list of domains that can be used to send emails.  If an Email communication is created with a From Address that is not from one of these domains, the Organization Email global attribute value will be used instead for the From Address and the original value will be used as the Reply To address.  This is to help reduce the likelihood of communications being rejected by the receiving email servers.", 0, "", "CDD29C51-5D33-435F-96AB-2C06BA772F88" );
            RockMigrationHelper.DeleteBlockAttributeValue("74583AD9-36F5-44CE-9346-AA009440A49A", "0305EF98-C791-4626-9996-F189B9BB674C");
            RockMigrationHelper.DeleteBlockAttributeValue( "840878A8-6CFF-4A9E-B9E7-48D33E37959C", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637" );
            RockMigrationHelper.DeleteBlock( "840878A8-6CFF-4A9E-B9E7-48D33E37959C" );
            RockMigrationHelper.DeleteBlock( "74583AD9-36F5-44CE-9346-AA009440A49A" );
            RockMigrationHelper.DeletePage( "B90576B0-110E-4DC0-8EB8-4668C5238508" );
            RockMigrationHelper.DeleteDefinedType( "DB91D0E9-DCA6-45A9-8276-AEF032BE8AED" );

            DropForeignKey( "dbo.WorkflowActivity", "ActivatedByActivityId", "dbo.WorkflowActivity" );
            DropIndex("dbo.WorkflowActivity", new[] { "ActivatedByActivityId" });
            DropColumn("dbo.WorkflowActivity", "ActivatedByActivityId");
        }
    }
}
