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
    public partial class AddAgeRestrictionToCheckInConfigurationGroupType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.GroupType",
                SystemGuid.FieldType.TEXT,
                "GroupTypePurposeValueId",
                "0",
                "Age Restriction",
                "Age Restriction",
                string.Empty,
                0,
                string.Empty,
                "cc975f7e-dab3-4d83-8a59-3a6862903350",
                "core_AgeRestriction" );

            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] = 'cc975f7e-dab3-4d83-8a59-3a6862903350'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "cc975f7e-dab3-4d83-8a59-3a6862903350" );
        }
    }
}
