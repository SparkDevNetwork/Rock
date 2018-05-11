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
using Rock.Plugin;

namespace Rock.StatementGenerator.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 3, "1.7.4" )]
    public class StatementGeneratorMoreOptions : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.StatementGenerator.SystemGuid.DefinedType.STATEMENT_GENERATOR_LAVA_TEMPLATE, Rock.SystemGuid.FieldType.INTEGER,
                "Footer Height", "FooterHeight", "The height of the footer in the generated pdf in millimeters. Adjust this if you have a custom footer that needs a custom height.", 103, "10", Rock.StatementGenerator.SystemGuid.Attribute.DEFINEDVALUE_STATEMENT_GENERATOR_LAVA_TEMPLATE_FOOTERHEIGHT );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
