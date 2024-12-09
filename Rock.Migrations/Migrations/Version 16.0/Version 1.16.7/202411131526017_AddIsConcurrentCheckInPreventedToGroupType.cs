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
    public partial class AddIsConcurrentCheckInPreventedToGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.GroupType", "IsConcurrentCheckInPrevented", c => c.Boolean( nullable: false, defaultValue: false ) );

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                SystemGuid.FieldType.TEXT,
                "GroupTypePurposeValueId",
                "0",
                "Grade and Age Matching Behavior",
                "Grade and Age Matching Behavior",
                string.Empty,
                0,
                string.Empty,
                "54e11d6f-2de6-4331-837c-7fa420f06450",
                "core_checkin_GradeAndAgeMatchingBehavior" );

            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] = '54e11d6f-2de6-4331-837c-7fa420f06450'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "54e11d6f-2de6-4331-837c-7fa420f06450" );

            DropColumn( "dbo.GroupType", "IsConcurrentCheckInPrevented" );
        }
    }
}
