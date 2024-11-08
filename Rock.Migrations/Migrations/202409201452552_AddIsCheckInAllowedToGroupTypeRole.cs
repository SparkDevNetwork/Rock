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
    /// <summary>
    ///
    /// </summary>
    public partial class AddIsCheckInAllowedToGroupTypeRole : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.GroupType", "AlreadyEnrolledMatchingLogic", c => c.Int( nullable: false, defaultValue: 0 ) );
            AddColumn( "dbo.GroupTypeRole", "IsCheckInAllowed", c => c.Boolean( nullable: false, defaultValue: false ) );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                SystemGuid.FieldType.BOOLEAN,
                "GroupTypePurposeValueId",
                "0",
                "Allow Remove From Family at Kiosk",
                "Allow Remove From Family at Kiosk",
                string.Empty,
                0,
                string.Empty,
                "4b12ddd0-eeb9-4443-838a-b4878f26fbca",
                SystemKey.GroupTypeAttributeKey.CHECKIN_GROUPTYPE_ALLOW_REMOVE_FROM_FAMILY_KIOSK );

            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] IN ('4b12ddd0-eeb9-4443-838a-b4878f26fbca')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "4b12ddd0-eeb9-4443-838a-b4878f26fbca" );

            DropColumn( "dbo.GroupTypeRole", "IsCheckInAllowed" );
            DropColumn( "dbo.GroupType", "AlreadyEnrolledMatchingLogic" );
        }
    }
}
