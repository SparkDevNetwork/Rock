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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 246, "17.1" )]
    public class AddProximityCheckInAttribute : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            // Add new check-in configuration attribute.
            RockMigrationHelper.AddOrUpdateEntityAttributeByGuid( "Rock.Model.GroupType",
                SystemGuid.FieldType.BOOLEAN,
                "GroupTypePurposeValueId",
                "0",
                "Enable Proximity Check-in",
                "Enable Proximity Check-in",
                string.Empty,
                0,
                string.Empty,
                "0aadc315-6922-4b1c-a29f-5666df5ecadc",
                "core_EnableProximityCheckIn" );

            // Set the GroupTypePurposeValueId value.
            Sql( @"
DECLARE @CheckInTemplateValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '4a406cb0-495b-4795-b788-52bdfde00b01')

UPDATE [Attribute]
SET [EntityTypeQualifierValue] = @CheckInTemplateValueId
WHERE [Guid] = '0aadc315-6922-4b1c-a29f-5666df5ecadc'" );

            // Enable attendance for the adult service area.
            Sql( "UPDATE [GroupType] SET [TakesAttendance] = 1 WHERE [Guid] = '235bae2b-5760-4763-aadf-3938f34ba100'" );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "0aadc315-6922-4b1c-a29f-5666df5ecadc" );
        }
    }
}