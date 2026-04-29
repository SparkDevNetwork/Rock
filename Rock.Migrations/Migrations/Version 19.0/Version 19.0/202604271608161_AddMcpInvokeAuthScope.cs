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
    public partial class AddMcpInvokeAuthScope : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
IF NOT EXISTS (SELECT 1 FROM [AuthScope] WHERE [Guid] = '67e7f27c-9022-47ba-8207-08d1fb54474f')
BEGIN
INSERT INTO [AuthScope]
	([IsActive], [IsSystem], [Name], [PublicName], [Guid])
	VALUES
	(1, 1, 'mcp:invoke', 'MCP Tools', '67e7f27c-9022-47ba-8207-08d1fb54474f')
END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( "DELETE FROM [AuthScope] WHERE [Guid] = '67e7f27c-9022-47ba-8207-08d1fb54474f'" );
        }
    }
}
