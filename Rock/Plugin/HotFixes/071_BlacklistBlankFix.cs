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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{

    /// <summary>
    /// This hotfix update is to address an issue where the blacklist value was edited and then deleted (so the defaults would be used) before the WhitelistBlacklist migration.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 71, "1.7.5" )]
    public class BlacklistBlankFix : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdateContentBlackList();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Down migration functionality not yet available for hotfix migrations.
        }

        /// <summary>
        /// Delete the attribute value for the Global blacklist if the value is exactly ', config' 
        /// </summary>
        private void UpdateContentBlackList()
        {
            Sql( @"
                DELETE FROM [dbo].[AttributeValue]
                WHERE [Value] = ', config' 
	                AND [AttributeId] = ( SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '9FFB15C1-AA53-4FBA-A480-64C9B348C5E5' )" );
        }
    }
}
