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
    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddEntitySetAdditionalSettingsJson : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.EntitySet", "AdditionalSettingsJson", c => c.String());

            // Add procedure (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCrm_PersonMerge]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCrm_PersonMerge];" );

            Sql( RockMigrationSQL._202601212358292_AddEntitySetAdditionalSettingsJson_spCrm_PersonMerge );

            // Add new spCrm_PersonMerge_ChangeHistory stored procedure for use with PersonMerge
            // to get the meta data for property/attribute last value changes.
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spCrm_PersonMerge_ChangeHistory]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spCrm_PersonMerge_ChangeHistory];
");
            
            Sql( @"
/*
<doc>
	<summary>
 		This procedure returns the ""Property"" type change history for the give person
        so you can tell who last changed the property's value and when.
	</summary>

    <returns>
        A result set containing one row per distinct History.ValueName (latest entry only),
        filtered to ChangeType = 'Property' for the specified person and their Primary Family.

        Columns:
        - CreatedDateTime (datetime): When the change was recorded.
        - CreatedByPersonAliasId (int): PersonAliasId of the user who made the change.
        - NickName (nvarchar): Nickname of the user who made the change.
        - LastName (nvarchar): Last name of the user who made the change.
        - ValueName (nvarchar): The name of the property that changed (History.ValueName).
        - AttributeId (int, nullable): History.RelatedEntityId when the change is for a Person Attribute;
          NULL for non-attribute properties and Primary Family address-related changes.
        - NewValue (nvarchar, nullable): The new value recorded by History.
        - OldValue (nvarchar, nullable): The prior value recorded by History.

        Ordering:
        - Returns rows ordered by CreatedDateTime descending.
    </returns>

	<param name=""Person Id"" datatype=""int"">The person id of a Person being merged</param>
	<code>
		EXEC [dbo].[spCrm_PersonMerge_ChangeHistory] @PersonId
	</code>
</doc>
*/

CREATE PROCEDURE [dbo].spCrm_PersonMerge_ChangeHistory
	@PersonId int

AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AttributeEntityTypeId INT = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '5997C8D3-8840-4591-99A5-552919F90CBD' );
    DECLARE @PrimaryFamilyId INT = ( SELECT TOP 1 [PrimaryFamilyId] FROM [Person] WHERE [Id] = @PersonId );

    ;WITH LatestPerProperty AS
    (
        SELECT
            h.[CreatedDateTime],
            h.[CreatedByPersonAliasId],
            h.[ValueName],
            h.[RelatedEntityId],
            h.[NewValue],
            h.[OldValue],
            ROW_NUMBER() OVER
            (
                PARTITION BY h.[ValueName]
                ORDER BY h.[CreatedDateTime] DESC, h.[Id] DESC
            ) AS rn
        FROM dbo.[History] AS h
        WHERE 
        (
          (    
              -- Get regular properties...
            h.[EntityTypeId] = 15
            AND h.[EntityId] = @PersonId
            -- ...and get Attribute EntityType changes
            AND ( h.[RelatedEntityId] IS NULL OR h.[RelatedEntityTypeId] = @AttributeEntityTypeId )
          ) 
          OR 
          (
             -- Get PrimaryFamily Address (Group Location) changes too 
            h.[EntityTypeId] = 16
            AND h.[EntityId] IS NOT NULL 
            AND h.[EntityId] = @PrimaryFamilyId
            AND h.[RelatedEntityId] IS NULL
          )
        )
        AND h.[ChangeType] = 'Property'
    )
    SELECT
        lpp.[CreatedDateTime],
        lpp.[CreatedByPersonAliasId],
        p.[NickName],
        p.[LastName],
        lpp.[ValueName],
        lpp.[RelatedEntityId] AS 'AttributeId',
        lpp.[NewValue],
        lpp.[OldValue]
    FROM [LatestPerProperty] AS lpp
    INNER JOIN dbo.[PersonAlias] AS pa
        ON pa.Id = lpp.[CreatedByPersonAliasId]
    INNER JOIN dbo.[Person] AS p
        ON p.Id = pa.[PersonId]
    WHERE lpp.rn = 1
    ORDER BY lpp.[CreatedDateTime] DESC;
  
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.EntitySet", "AdditionalSettingsJson");
        }
    }
}
