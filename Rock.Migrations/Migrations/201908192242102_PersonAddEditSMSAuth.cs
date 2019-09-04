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
    public partial class PersonAddEditSMSAuth : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddPersonEditSMSAuthorization();
        }

        private void AddPersonEditSMSAuthorization()
        {
            Sql( @"
           DECLARE @EntityTypeId INT = (
            		SELECT TOP 1 [Id]
            		FROM [EntityType]
            		WHERE [Name] = 'Rock.Model.Block'
            		)
            DECLARE @PersonEditBlockId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Block]
            		WHERE [Guid] = '59C7EA79-2073-4EA9-B439-7E74F06E8F5B'
            		)
            DECLARE @RockAdminGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E'
            		)
            DECLARE @Order INT = (
            		SELECT MAX([Order])
            		FROM [Auth]
            		WHERE [EntityTypeId] = @EntityTypeId
            			AND [EntityId] = @PersonEditBlockId
            			AND [Action] = 'Edit'
            		)

            -- If not already added...
            IF NOT EXISTS (
            		SELECT *
            		FROM [Auth]
            		WHERE [Action] = 'EditSMS'
            			AND [EntityTypeId] = @EntityTypeId
            			AND [EntityId] = @PersonEditBlockId
            		)
            BEGIN
                -- For anyone who currently has auth for 'Edit' action, give them the same for 'EditSMS' action
            	INSERT INTO [Auth] (
            		[EntityTypeId]
            		,[EntityId]
            		,[Order]
            		,[Action]
            		,[AllowOrDeny]
            		,[SpecialRole]
            		,[GroupId]
            		,[PersonAliasId]
            		,[Guid]
            		)
            	SELECT [EntityTypeId]
            		,[EntityId]
            		,[Order]
            		,'EditSMS'
            		,[AllowOrDeny]
            		,[SpecialRole]
            		,[GroupId]
            		,[PersonAliasId]
            		,NEWID()
            	FROM [Auth]
            	WHERE [EntityTypeId] = @EntityTypeId
            		AND [EntityId] = @PersonEditBlockId
            		AND [Action] = 'Edit'

                -- Give the Rock Admin explicit ALLOW 'EditSMS' action
            	IF @RockAdminGroupId IS NOT NULL
            	BEGIN
                    IF NOT EXISTS (
            		        SELECT *
            		        FROM [Auth]
            		        WHERE [Action] = 'EditSMS'
            			        AND [EntityTypeId] = @EntityTypeId
            			        AND [EntityId] = @PersonEditBlockId
                                AND [AllowOrDeny] = 'A'
                                AND [GroupId] = @RockAdminGroupId
            		        )
                    BEGIN
            		    INSERT INTO [Auth] (
            			    [EntityTypeId]
            			    ,[EntityId]
            			    ,[Order]
            			    ,[Action]
            			    ,[AllowOrDeny]
            			    ,[SpecialRole]
            			    ,[GroupId]
            			    ,[Guid]
            			    )
            		    VALUES (
            			    @EntityTypeId
            			    ,@PersonEditBlockId
            			    ,@Order + 1
            			    ,'EditSMS'
            			    ,'A'
            			    ,0
            			    ,@RockAdminGroupId
            			    ,NEWID()
            			    )
            	    END
            	END

                -- Add the Deny ALL USERS auth for EditSMS
            	INSERT INTO [Auth] (
            		[EntityTypeId]
            		,[EntityId]
            		,[Order]
            		,[Action]
            		,[AllowOrDeny]
            		,[SpecialRole]
            		,[Guid]
            		)
            	VALUES (
            		@EntityTypeId
            		,@PersonEditBlockId
            		,@Order + 2
            		,'EditSMS'
            		,'D'
            		,1
            		,NEWID()
            		)
            END
            " );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
