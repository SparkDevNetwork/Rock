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
    [MigrationNumber( 245, "1.17.1" )]
    public class UpdateMergeFieldAttributeDescription : Migration
    {
        /// <summary>
        /// Up methods
        /// </summary>
        public override void Up()
        {
            // We need to fix the descriptions of the following attributes due to being incorrect:
            Sql( @"
                UPDATE [Attribute]
                SET [Description] = 'The Lava syntax for accessing values from the check-in state object.'
                WHERE [Guid] = '51eb8583-55ea-4431-8b66-b5bd0f83d81e';
            " );

            Sql( @"
                UPDATE [Attribute] 
                SET [Description] = 'The Lava syntax to use for formatting addresses.'
                WHERE [Guid] = 'b6ef4138-c488-4043-a628-d35f91503843'
            " );
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Down()
        {
        }
    }
}