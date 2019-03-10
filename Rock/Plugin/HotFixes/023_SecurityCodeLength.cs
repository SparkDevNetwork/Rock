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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Migration to add the Fundraising feature data bits.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 23, "1.6.3" )]
    public class SecurityCodeLength : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Moved to core migration: 201711271827181_V7Rollup
            //            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Alpha Length", "", 0, "0", "B76A6877-FD96-4A00-9470-AEFC3788D795", "core_checkin_SecurityCodeAlphaLength" );
            //            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Numeric Length", "", 0, "0", "90980CBA-9842-40AB-A258-880087973258", "core_checkin_SecurityCodeNumericLength" );
            //            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Numeric Value Randomized", "", 0, "True", "FD72C08A-81E9-4D93-9370-2BA1B4192601", "core_checkin_SecurityCodeNumericRandom" );

            //            Sql( @"
            //        UPDATE [Attribute] SET [Name] = 'Security Code Alpha-Numeric Length' WHERE [Guid] = '712CFC8A-7B67-4793-A71E-E2EEB2D1048D'

            //        DECLARE @GroupTypeEntityTypeId int = ( SELECT TOP 1[Id] FROM[EntityType] WHERE[Name] = 'Rock.Model.GroupType' )
            //        DECLARE @CheckInTemplatePurposeId int = ( SELECT TOP 1[Id] FROM[DefinedValue] WHERE[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01' )
            //        IF @GroupTypeEntityTypeId IS NOT NULL AND @CheckInTemplatePurposeId IS NOT NULL
            //        BEGIN

            //            UPDATE[Attribute] SET[EntityTypeQualifierValue] = CAST( @CheckInTemplatePurposeId AS varchar )
            //            WHERE[EntityTypeId] = @GroupTypeEntityTypeId
            //            AND[EntityTypeQualifierColumn] = 'GroupTypePurposeValueId'
            //            AND[Key] LIKE 'core_checkin_%'

            //        END
            //" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
           // RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Length", "", 0, "3", "712CFC8A-7B67-4793-A71E-E2EEB2D1048D", "core_checkin_SecurityCodeLength" );
        }
    }
}
