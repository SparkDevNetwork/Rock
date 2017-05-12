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
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 25, "1.6.3" )]
    public class PersonGivingEnvelopeAttribute : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Finance Internal", "fa fa-money", "Internal Finance Attributes", SystemGuid.Category.PERSON_ATTRIBUTES_FINANCE_INTERNAL );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.INTEGER, SystemGuid.Category.PERSON_ATTRIBUTES_FINANCE_INTERNAL, "Envelope Number", "core_GivingEnvelopeNumber", "fa fa-money", "The Giving Envelope Number that is associated with this Person", 1, "", SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER );
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.BOOLEAN, null, null, "Enable Giving Envelope Number", "Enables the Giving Envelope Number feature", 0, false.ToString(), Rock.SystemGuid.Attribute.GLOBAL_ENABLE_GIVING_ENVELOPE, "core.EnableGivingEnvelopeNumber" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
