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
    [MigrationNumber( 30, "1.6.6" )]
    public class MyConnectionOpportunitiesLava : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "My Connection Opportunities Lava", "Block to display connection opportunities that are assigned to the current user. The display format is controlled by a lava template.", "~/Blocks/Connection/MyConnectionOpportunitiesLava.ascx", "Connection", "1B8E50A0-7AC4-475F-857C-50D0809A3F04" );
            RockMigrationHelper.AddBlock( "AE1818D8-581C-4599-97B9-509EA450376A", "", "1B8E50A0-7AC4-475F-857C-50D0809A3F04", "My Connection Opportunities Lava", "Main", "", "", 2, "35B7FF3C-969E-44BE-BACA-EDB490450DFF" );


            RockMigrationHelper.AddBlockAttributeValue( "35B7FF3C-969E-44BE-BACA-EDB490450DFF", "9E6887CA-6D20-47EE-8158-3EC9F06F063D", @"50f04e77-8d3b-4268-80ab-bc15dd6cb262" ); // Detail Page

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
