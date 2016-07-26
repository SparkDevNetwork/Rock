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
    /// 
    /// </summary>
    [MigrationNumber( 2, "1.5.0" )]
    public class CheckinGradeRequired : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Moved to core migration: 201606231322565_RegistrationWorkflow
            //RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Grade Required", "", 0, "False", "A4899874-9EDF-4549-B054-4F593F4C4362", "core_checkin_GradeRequired" );
            //RockMigrationHelper.UpdateAttributeQualifier( "A4899874-9EDF-4549-B054-4F593F4C4362", "falsetext", "No", "B61ED891-C631-4172-A05D-D86265CA2A1D" );
            //RockMigrationHelper.UpdateAttributeQualifier( "A4899874-9EDF-4549-B054-4F593F4C4362", "truetext", "Yes", "D4C52849-6ED4-414A-95D7-2F7F805CF9A3" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //RockMigrationHelper.DeleteAttribute( "46C8DC94-D57E-4B9A-8FB9-1A797DD3D525" );
        }
    }
}
