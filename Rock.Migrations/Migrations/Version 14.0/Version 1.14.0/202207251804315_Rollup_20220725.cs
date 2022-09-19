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
    public partial class Rollup_20220725 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateIncorrectBlockCategories();
            RockbadgeMarkupChanges();
            FixLavaShortcodeToCategoryLinking();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// GJ: Update Incorrect Block Categories
        /// </summary>
        private void UpdateIncorrectBlockCategories()
        {
            Sql( @"
                UPDATE [BlockType] SET [Category]=N'Check-in' WHERE ([Guid]='A5ECE422-D473-4B8F-BEE9-5651AFCB2AB3')
                UPDATE [BlockType] SET [Category]=N'Check-in' WHERE ([Guid]='678ED4B6-D76F-4D43-B069-659E352C9BD8')
                UPDATE [BlockType] SET [Category]=N'Security > Background Check' WHERE ([Guid]='562A5CA4-1697-40E3-A54A-C451291A3251')
                UPDATE [BlockType] SET [Category]=N'Security > Background Check' WHERE ([Guid]='AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663')" );
        }

        /// <summary>
        /// GJ: Fix Additional Badges to Rockbadge markup
        /// </summary>
        private void RockbadgeMarkupChanges()
        {
            // Update Group Requirements
            Sql( @"
                DECLARE @BadgeId INT = (SELECT TOP 1 [Id] FROM [Badge] WHERE [Guid]='132F9C2A-0AF4-4AD9-87EF-7730B284E10E')
                DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')
                UPDATE [AttributeValue] SET [Value] = REPLACE(Value,'class=""badge""','class=""rockbadge rockbadge-icon""') 
                WHERE [AttributeId] = @AttributeId AND [EntityId] = @BadgeId" );

            // Update Record Status Value
            Sql( @"
                UPDATE [AttributeValue]
                SET [Value]=N'{% if Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Inactive"" -%}
    <div class=""rockbadge rockbadge-label"">
                    <span class=""label label-danger"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
                    {% elseif Person.RecordStatusValue.Value != empty and Person.RecordStatusValue.Value == ""Pending"" -%}
                    <span class=""label label-warning"" title=""{{ Person.RecordStatusReasonValue.Value }}"" data-toggle=""tooltip"">{{ Person.RecordStatusValue.Value }}</span>
    </div>
{% endif -%}'
                WHERE ([Guid]='B434C492-D5AA-4F81-BEBA-C50F4B82263A')" );
        }

        /// <summary>
        /// ED: Fix LavaShortcode to Category linking
        /// </summary>
        private void FixLavaShortcodeToCategoryLinking()
        {
            Sql( @"
                DECLARE @LavaShortcodeCategoryEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.LavaShortcodeCategory')
                DECLARE @LavaShortcodeEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.LavaShortcode')

                UPDATE [Category]
                SET [EntityTypeId] = @LavaShortcodeEntityTypeId
                WHERE [EntityTypeId] = @LavaShortcodeCategoryEntityTypeId" );
        }
    }
}
