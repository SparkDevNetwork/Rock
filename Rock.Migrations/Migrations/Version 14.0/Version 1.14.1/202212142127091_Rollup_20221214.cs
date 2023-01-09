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
    public partial class Rollup_20221214 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AccountProtectionProfileViewSecurity();
            MobileLiveExperienceOccurrencesUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            MobileLiveExperienceOccurrencesDown();
        }

        /// <summary>
        /// SK: Account Protection Profile View Security (data migration)
        /// </summary>
        private void AccountProtectionProfileViewSecurity()
        {
            Sql( @"DECLARE @EntityTypeId INT = (
            		SELECT TOP 1 [Id]
            		FROM [EntityType]
            		WHERE [Name] = 'Rock.Model.Block'
            		)
            DECLARE @PersonBioBlockId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Block]
            		WHERE [Guid] = '1E6AF671-9C1A-4C6C-8156-36B6D7589F34'
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
            DECLARE @StaffWorkersGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '2C112948-FF4C-46E7-981A-0257681EADF4'
            		)
            DECLARE @StaffLikeWorkersGroupId INT = (
            		SELECT TOP 1 [Id]
            		FROM [Group]
            		WHERE [Guid] = '300BA2C8-49A3-44BA-A82A-82E3FD8C3745'
            		)
            DECLARE @BioBlockOrder INT = (
            		SELECT ISNULL(MAX([order])+1,0)
            		FROM [Auth]
            		WHERE EntityTypeId = @EntityTypeId
            			AND EntityId = @PersonBioBlockId
            			AND [Action] = 'ViewProtectionProfile'
            		)

            DECLARE @PersonEditBlockOrder INT = (
            		SELECT ISNULL(MAX([order])+1,0)
            		FROM [Auth]
            		WHERE EntityTypeId = @EntityTypeId
            			AND EntityId = @PersonBioBlockId
            			AND [Action] = 'ViewProtectionProfile'
            		)

            --select * from [Auth] 
            IF NOT EXISTS (
            		SELECT *
            		FROM Auth
            		WHERE Action = 'ViewProtectionProfile'
            			AND EntityTypeId = @EntityTypeId
            			AND EntityId = @PersonBioBlockId
            		)
            BEGIN
                -- Add Rock Administration
            	INSERT INTO [Auth] (
            		EntityTypeid
            		,EntityId
            		,[Order]
            		,[Action]
            		,AllowOrDeny
            		,SpecialRole
            		,GroupId
            		,[Guid]
            		)
                VALUES (
            		@EntityTypeId
            		,@PersonBioBlockId
            		,@BioBlockOrder
            		,'ViewProtectionProfile'
            		,'A'
            		,0
            		,@RockAdminGroupId
            		,NEWID()
            		)

                INSERT INTO [Auth] (
            		EntityTypeid
            		,EntityId
            		,[Order]
            		,[Action]
            		,AllowOrDeny
            		,SpecialRole
            		,GroupId
            		,[Guid]
            		)
                VALUES (
            		@EntityTypeId
            		,@PersonEditBlockId
            		,@PersonEditBlockOrder
            		,'ViewProtectionProfile'
            		,'A'
            		,0
            		,@RockAdminGroupId
            		,NEWID()
            		)

               -- Staff Workers Group

                INSERT INTO [Auth] (
            		EntityTypeid
            		,EntityId
            		,[Order]
            		,[Action]
            		,AllowOrDeny
            		,SpecialRole
            		,GroupId
            		,[Guid]
            		)
                VALUES (
            		@EntityTypeId
            		,@PersonBioBlockId
            		,@BioBlockOrder + 1
            		,'ViewProtectionProfile'
            		,'A'
            		,0
            		,@StaffWorkersGroupId
            		,NEWID()
            		)

                INSERT INTO [Auth] (
            		EntityTypeid
            		,EntityId
            		,[Order]
            		,[Action]
            		,AllowOrDeny
            		,SpecialRole
            		,GroupId
            		,[Guid]
            		)
                VALUES (
            		@EntityTypeId
            		,@PersonEditBlockId
            		,@PersonEditBlockOrder + 1
            		,'ViewProtectionProfile'
            		,'A'
            		,0
            		,@StaffWorkersGroupId
            		,NEWID()
            		)
                
                -- Staff Like Workers Group

                INSERT INTO [Auth] (
            		EntityTypeid
            		,EntityId
            		,[Order]
            		,[Action]
            		,AllowOrDeny
            		,SpecialRole
            		,GroupId
            		,[Guid]
            		)
                VALUES (
            		@EntityTypeId
            		,@PersonBioBlockId
            		,@BioBlockOrder + 2
            		,'ViewProtectionProfile'
            		,'A'
            		,0
            		,@StaffLikeWorkersGroupId
            		,NEWID()
            		)

                INSERT INTO [Auth] (
            		EntityTypeid
            		,EntityId
            		,[Order]
            		,[Action]
            		,AllowOrDeny
            		,SpecialRole
            		,GroupId
            		,[Guid]
            		)
                VALUES (
            		@EntityTypeId
            		,@PersonEditBlockId
            		,@PersonEditBlockOrder + 2
            		,'ViewProtectionProfile'
            		,'A'
            		,0
            		,@StaffLikeWorkersGroupId
            		,NEWID()
            		)

                -- Deny All other Users

            	INSERT INTO [dbo].[Auth] (
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
            		,@PersonBioBlockId
            		,@BioBlockOrder + 3
            		,'ViewProtectionProfile'
            		,'D'
            		,1
            		,NEWID()
            		)

                INSERT INTO [dbo].[Auth] (
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
            		,@PersonEditBlockOrder + 3
            		,'ViewProtectionProfile'
            		,'D'
            		,1
            		,NEWID()
            		)
            END
            " );
        }
    
        /// <summary>
        /// DH: Add Block Template for Mobile Live Experiences block.
        /// </summary>
        private void MobileLiveExperienceOccurrencesUp()
        {
            const string standardIconSvg = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.TEMPLATE_BLOCK,
                "Mobile > Event > Live Experience Occurrences > Template",
                string.Empty,
                SystemGuid.DefinedValue.BLOCK_TEMPLATE_LIVE_EXPERIENCE_OCCURRENCES );

            RockMigrationHelper.AddOrUpdateTemplateBlockTemplate(
                "0C75B833-E710-45AE-B3B2-3FAC97A79BB2",
                Rock.SystemGuid.DefinedValue.BLOCK_TEMPLATE_LIVE_EXPERIENCE_OCCURRENCES,
                "Default",
                @"<StackLayout>
    {% for occurrence in Occurrences %}
        {% if occurrence.Campus != null %}
            {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }} at {{ occurrence.Campus.Name }}{% endcapture %}
        {% else %}
            {% capture occurrenceName %}{{ occurrence.InteractiveExperienceSchedule.InteractiveExperience.Name }}{% endcapture %}
        {% endif %}

        <Button Text=""{{ occurrenceName | Escape }}"" StyleClass=""btn,btn-primary"" Command=""{Binding PushPage}"" CommandParameter=""{{ DestinationPageGuid }}?InteractiveExperienceOccurrenceKey={{ occurrence.Guid }}"" />
    {% endfor %}

    {% if LoginRecommended == true %}
        <Button Text=""Login"" StyleClass=""btn,btn-secondary"" Command=""{Binding InteractiveExperienceOccurrences.Login}"" />
    {% endif %}

    {% if GeoLocationRecommended == true %}
        <Button Text=""Provide Location"" StyleClass=""btn,btn-secondary"" />
    {% endif %}
</StackLayout>",
                standardIconSvg,
                "standard-template.svg",
                "image/svg+xml" );
        }

        /// <summary>
        /// DH: Add Block Template for Mobile Live Experiences block.
        /// </summary>
        private void MobileLiveExperienceOccurrencesDown()
        {
            RockMigrationHelper.DeleteTemplateBlockTemplate( "0C75B833-E710-45AE-B3B2-3FAC97A79BB2" );
            RockMigrationHelper.DeleteDefinedValue( SystemGuid.DefinedValue.BLOCK_TEMPLATE_LIVE_EXPERIENCE_OCCURRENCES );
        }
    }
}
