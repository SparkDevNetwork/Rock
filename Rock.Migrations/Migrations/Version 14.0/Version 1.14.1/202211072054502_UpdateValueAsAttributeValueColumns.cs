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
    public partial class UpdateValueAsAttributeValueColumns : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Get rid of the old trigger that won't be needed anymore.
            Sql( "DROP TRIGGER [dbo].[tgrAttributeValue_InsertUpdate]" );

            // Re-create the ValueAsBoolean as a regular nullable column.
            DropIndex( "dbo.AttributeValue", new string[] { "ValueAsBoolean" } );
            DropColumn( "dbo.AttributeValue", "ValueAsBoolean" );
            AddColumn( "dbo.AttributeValue", "ValueAsBoolean", c => c.Boolean( nullable: true ) );
            CreateIndex( "dbo.AttributeValue", "ValueAsBoolean" );

            // Re-create the ValueAsPersonId as a regular nullable column.
            DropColumn( "dbo.AttributeValue", "ValueAsPersonId" );
            AddColumn( "dbo.AttributeValue", "ValueAsPersonId", c => c.Int( nullable: true ) );
            AddForeignKey( "dbo.AttributeValue", "ValueAsPersonId", "dbo.Person", "Id" );

            // Add a job that will update all the existing values.
            Sql( $@"
            IF NOT EXISTS (
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PostV141UpdateValueAsColumns'
                    AND [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_VALUEAS_ATTRIBUTE_VALUE_COLUMNS}'
            )
            BEGIN
                INSERT INTO [ServiceJob] (
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES (
                      1
                    , 1
                    , 'Rock Update Helper v14.1 - Update ValueAs Columns'
                    , 'Updates the ValueAs___ AttributeValue columns that were converted away from computed columns.'
                    , 'Rock.Jobs.PostV141UpdateValueAsColumns'
                    , '0 0 21 1/1 * ? *'
                    , 1
                    , '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_VALUEAS_ATTRIBUTE_VALUE_COLUMNS}'
                );
            END" );

            Sql( MigrationSQL._202211072054502_UpdateValueAsAttributeValueColumns_spCrm_PersonMerge );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( MigrationSQL._202202111031395_FixPersonMergeWithGroupAssignments_spCrm_PersonMerge );

            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_141_UPDATE_VALUEAS_ATTRIBUTE_VALUE_COLUMNS}'" );

            DropForeignKey( "dbo.AttributeValue", "ValueAsPersonId", "dbo.Person" );

            DropIndex( "dbo.AttributeValue", new string[] { "ValueAsBoolean" } );
            DropColumn( "dbo.AttributeValue", "ValueAsBoolean" );
            Sql( "ALTER TABLE [AttributeValue] ADD [ValueAsBoolean] AS (case when [Value] IS NULL OR [Value]='' OR len([Value])>len('false') then NULL when lower([Value])='1' OR lower([Value])='y' OR lower([Value])='t' OR lower([Value])='yes' OR lower([Value])='true' then CONVERT([bit],(1)) else CONVERT([bit],(0)) end) PERSISTED" );
            CreateIndex( "dbo.AttributeValue", "ValueAsBoolean" );

            DropColumn( "dbo.AttributeValue", "ValueAsPersonId" );
            Sql( "ALTER TABLE [AttributeValue] ADD [ValueAsPersonId] AS (case when [Value] like '________-____-____-____-____________' then [dbo].[ufnUtility_GetPersonIdFromPersonAliasGuid]([Value])  end)" );

            Sql( @"
CREATE TRIGGER [dbo].[tgrAttributeValue_InsertUpdate]
       ON  [dbo].[AttributeValue]
       AFTER INSERT, UPDATE
    AS 
    BEGIN

        UPDATE [AttributeValue] SET ValueAsDateTime = 
		    CASE WHEN 
			    LEN(value) < 50 and 
			    ISNULL(value,'') != '' and 
			    ISNUMERIC([value]) = 0 THEN
				    CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN 
					    ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				    ELSE
					    TRY_CAST( [value] as datetime )
				    END
		    END
        WHERE [Id] IN ( SELECT [Id] FROM INSERTED )    

    END" );
            Sql( "ALTER TABLE [dbo].[AttributeValue] ENABLE TRIGGER [tgrAttributeValue_InsertUpdate]" );
        }
    }
}
