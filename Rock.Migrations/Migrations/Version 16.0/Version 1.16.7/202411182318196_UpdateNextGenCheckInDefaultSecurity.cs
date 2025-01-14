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
    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateNextGenCheckInDefaultSecurity : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Next-gen check-in default security.
            RockMigrationHelper.AddSecurityAuthForSite( SystemGuid.Site.NEXT_GEN_CHECK_IN,
                0,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_ADMINISTRATORS,
                ( int ) SpecialRole.None,
                "9719652d-21f2-4cb2-9332-015c3c066fc0" );

            RockMigrationHelper.AddSecurityAuthForSite( SystemGuid.Site.NEXT_GEN_CHECK_IN,
                1,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_MEMBERS,
                ( int ) SpecialRole.None,
                "06bf82c8-4fc4-41fd-8eee-77838abcf55e" );

            RockMigrationHelper.AddSecurityAuthForSite( SystemGuid.Site.NEXT_GEN_CHECK_IN,
                2,
                Security.Authorization.VIEW,
                true,
                SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
                ( int ) SpecialRole.None,
                "2535c24c-59ae-4b2a-9010-3028c038fae6" );

            RockMigrationHelper.AddSecurityAuthForSite( SystemGuid.Site.NEXT_GEN_CHECK_IN,
                3,
                Security.Authorization.VIEW,
                false,
                null,
                ( int ) SpecialRole.AllUsers,
                "60bb3f30-1f48-49e8-a908-0748523a6e14" );

            RockMigrationHelper.AddSecurityAuthForPage( "7D1732D5-3957-475F-A259-4DB8261C2049",
                0,
                Security.Authorization.VIEW,
                true,
                "51e02a99-b7cb-4e64-b7c8-065076aabc05",
                ( int ) SpecialRole.None,
                "edd66626-c867-42d1-bc34-ea6c88488127" );

            // Add new check-in configuration attribute.
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                SystemGuid.FieldType.TEXT,
                "GroupTypePurposeValueId",
                "0",
                "Promotions Content Channel",
                "Promotions Content Channel",
                string.Empty,
                0,
                string.Empty,
                "ca4f948d-2390-4532-afbf-ebf51f43516c",
                "core_PromotionsContentChannel" );

            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] = 'ca4f948d-2390-4532-afbf-ebf51f43516c'" );

            // Set default values for existing check-in configurations.
            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')
DECLARE @AttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'ca4f948d-2390-4532-afbf-ebf51f43516c')

INSERT INTO [AttributeValue]
    ([AttributeId], [EntityId], [Value], [IsSystem], [Guid])
    SELECT
        @AttributeId,
        [Id],
        'a57bdbcd-fa77-4a6e-967d-1c5ace962587',
        0,
        NEWID()
    FROM [GroupType] AS [GT]
    WHERE [GT].[GroupTypePurposeValueId] = @CheckInTemplateValueId
      AND [GT].[Id] NOT IN (SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @AttributeId)" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "ca4f948d-2390-4532-afbf-ebf51f43516c" );
            RockMigrationHelper.DeleteSecurityAuth( "edd66626-c867-42d1-bc34-ea6c88488127" );
            RockMigrationHelper.DeleteSecurityAuth( "60bb3f30-1f48-49e8-a908-0748523a6e14" );
            RockMigrationHelper.DeleteSecurityAuth( "2535c24c-59ae-4b2a-9010-3028c038fae6" );
            RockMigrationHelper.DeleteSecurityAuth( "06bf82c8-4fc4-41fd-8eee-77838abcf55e" );
            RockMigrationHelper.DeleteSecurityAuth( "9719652d-21f2-4cb2-9332-015c3c066fc0" );
        }
    }
}
