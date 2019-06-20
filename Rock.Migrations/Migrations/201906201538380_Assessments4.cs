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
    public partial class Assessments4 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddMotivatorsEngagement();
            MoveLegacyDiscAttributesToPADCategory();
            CopyLegacyDiscPersonalityTypeToAssessmentDiscScore();
            UpdateDiscProfileAttributeCategory();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Adds the person attribute Motivator Engaging
        /// </summary>
        private void AddMotivatorsEngagement()
        {
            var categories = new System.Collections.Generic.List<string> { "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" };
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( SystemGuid.FieldType.DECIMAL, categories, "Motivator Engaging", "Motivator Engaging", "core_MotivatorEngaging", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_ENGAGING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( SystemGuid.Attribute.PERSON_MOTIVATOR_ENGAGING );
            AddDenyToAllSecurityToAttribute( SystemGuid.Attribute.PERSON_MOTIVATOR_ENGAGING );
        }

        /// <summary>
        /// Adds the deny to all security to attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void AddDenyToAllSecurityToAttribute( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );
        }

        /// <summary>
        /// Move three Legacy DISC attributes to the new, Personality Assessment Data category
        /// </summary>
        private void MoveLegacyDiscAttributesToPADCategory()
        {
            Sql( @"
                DECLARE @DISCCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = '0B187C81-2106-4875-82B6-FBF1277AE23B' ) -- DISC
                DECLARE @HiddenCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = 'B08A3096-FCFA-4DA0-B95D-1F3F11CC9969' ) -- Personality Assessment Data

                DECLARE @LastSaveDateAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '990275DB-611B-4D2E-94EA-3FFA1186A5E1' ) -- key LastSaveDate
                DECLARE @LastDiscRequestDateAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '41B73365-A984-879E-4749-7DB4FC720138' ) -- key LastDiscRequestDate
                DECLARE @PersonalityTypeAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = 'C7B529C6-B6C8-45B5-B892-5D9821CEDDCD' ) -- key PersonalityType

                UPDATE [AttributeCategory]
                SET [CategoryId] = @HiddenCategoryId
                WHERE [AttributeId] IN ( @LastSaveDateAttributeId, @LastDiscRequestDateAttributeId, @PersonalityTypeAttributeId )
	                AND [CategoryId] = @DISCCategoryId" );
        }

        private void UpdateDiscProfileAttributeCategory()
        {
            Sql( @"
                DECLARE @DISCCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = '0B187C81-2106-4875-82B6-FBF1277AE23B' ) -- DISC
                DECLARE @HiddenCategoryId INT = ( SELECT TOP 1 ID FROM [Category] WHERE [Guid] = 'B08A3096-FCFA-4DA0-B95D-1F3F11CC9969' ) -- Personality Assessment Data
                DECLARE @DiscProfileAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D' ) -- key LastSaveDate

                UPDATE [AttributeCategory]
                SET [CategoryId] = @DISCCategoryID
                WHERE [AttributeId] = @DiscProfileAttributeId
	                AND [CategoryId] = @HiddenCategoryId" );
        }

        /// <summary>
        /// Copy values from old PersonalityType to new core_DISCDISCProfile type attribute
        /// </summary>
        private void CopyLegacyDiscPersonalityTypeToAssessmentDiscScore()
        {
            Sql( @"
                DECLARE @DefinedTypeId INT = ( SELECT TOP 1 ID FROM [DefinedType] WHERE [Guid] = 'F06DDAD8-6058-4182-AD0A-B523BB7A2D78' )
                DECLARE @OldDISCPersonalityAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = 'C7B529C6-B6C8-45B5-B892-5D9821CEDDCD' ) -- key PersonalityType
                DECLARE @NewDISCPersonalityAttributeId INT = ( SELECT TOP 1 ID FROM [Attribute] WHERE [Guid] = '6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D' ) -- key core_DISCDISCProfile

                -- Delete any blank values that exist
                DELETE FROM [AttributeValue] 
                WHERE [AttributeId] = @NewDISCPersonalityAttributeId
	                AND ( [Value] IS NULL OR [Value] = '')

                INSERT INTO [AttributeValue] ([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
                SELECT 0, @NewDISCPersonalityAttributeId, [EntityId], dv.[Guid], NEWID()
                FROM [AttributeValue] av
                -- join on the defined value table to find the one that matches... in order to get the DV's GUID
                INNER JOIN [DefinedValue] dv ON dv.[Value] = av.[Value] and dv.[DefinedTypeId] = @DefinedTypeId
                WHERE av.[AttributeId] = @OldDISCPersonalityAttributeId AND av.[Value] <> ''
                AND NOT EXISTS (
	                SELECT * FROM [AttributeValue] c
                    WHERE c.[AttributeId] = @NewDISCPersonalityAttributeId AND av.[EntityId] = c.[EntityId])" );
        }

    }
}
