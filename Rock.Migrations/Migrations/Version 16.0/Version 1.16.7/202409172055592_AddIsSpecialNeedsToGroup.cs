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
    public partial class AddIsSpecialNeedsToGroup : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Group", "IsSpecialNeeds", c => c.Boolean( nullable: false, defaultValue: false ) );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.Person",
                SystemGuid.FieldType.BOOLEAN,
                string.Empty,
                string.Empty,
                "Special Needs",
                "Special Needs",
                "Used by the check-in system to help determine which groups this individual may attend.",
                0,
                string.Empty,
                "aa7e837c-4a12-4ecd-891e-7dc93bd7f5bc",
                "core_SpecialNeeds" );

            Sql( @"
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'aa7e837c-4a12-4ecd-891e-7dc93bd7f5bc')
DECLARE @CategoryId INT = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '752dc692-836e-4a3e-b670-4325cd7724bf')
DECLARE @MaxOrder INT = (SELECT ISNULL(MAX([Order]), 0) FROM [Attribute])

UPDATE [Attribute]
SET [Order] = @MaxOrder + 1
WHERE [Guid] = 'aa7e837c-4a12-4ecd-891e-7dc93bd7f5bc'

IF @CategoryId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT * FROM [AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId)
    BEGIN
        INSERT INTO [AttributeCategory] ([AttributeId], [CategoryId]) VALUES (@AttributeId, @CategoryId)
    END
END" );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                SystemGuid.FieldType.BOOLEAN,
                "GroupTypePurposeValueId",
                "0",
                "Remove Special Needs Groups",
                "Remove Special Needs Groups",
                string.Empty,
                0,
                string.Empty,
                "2c10c4aa-ca8b-47fd-a7af-3c5c627e8c07",
                "core_RemoveSpecialNeedsGroups" );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                SystemGuid.FieldType.BOOLEAN,
                "GroupTypePurposeValueId",
                "0",
                "Remove Non-Special Needs Groups",
                "Remove Non-Special Needs Groups",
                string.Empty,
                0,
                string.Empty,
                "eb6f18e8-c3b0-4fac-a128-8944c9ce4e73",
                "core_RemoveNonSpecialNeedsGroups" );

            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] IN ('2c10c4aa-ca8b-47fd-a7af-3c5c627e8c07', 'eb6f18e8-c3b0-4fac-a128-8944c9ce4e73')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "eb6f18e8-c3b0-4fac-a128-8944c9ce4e73" );
            RockMigrationHelper.DeleteAttribute( "2c10c4aa-ca8b-47fd-a7af-3c5c627e8c07" );

            RockMigrationHelper.DeleteAttribute( "aa7e837c-4a12-4ecd-891e-7dc93bd7f5bc" );

            DropColumn( "dbo.Group", "IsSpecialNeeds" );
        }
    }
}
