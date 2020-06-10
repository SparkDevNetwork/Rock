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
    [MigrationNumber( 127, "1.10.3" )]
    public class MigrationRollupsFor10_4_0 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //UpdateWorkflowProcessingIntervals();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// SK: Update ProcessingIntervalSeconds for few Workflow Types
        /// </summary>
        private void UpdateWorkflowProcessingIntervals()
        {
            // Update ProcessingIntervalSeconds for several Workflow Types and the logging level for one
            Sql(
                @"UPDATE
                    [WorkflowType]
                SET [ProcessingIntervalSeconds]=3600
                WHERE
                    [Guid] IN (
                                '655BE2A4-2735-4CF9-AEC8-7EF5BE92724C',
                                '221bf486-a82c-40a7-85b7-bb44da45582f',
                                '036f2f0b-c2dc-49d0-a17b-ccdac7fc71e2',
                                '16d12ef7-c546-4039-9036-b73d118edc90',
                                'f5af8224-44dc-4918-aab7-c7c9a5a6338d',
                                '9bc07356-3b2f-4bff-9320-fa8f3a28fc39',
                                '31ddc001-c91a-4418-b375-cab1475f7a62'
                                )
                    AND ([ProcessingIntervalSeconds] IS NULL OR [ProcessingIntervalSeconds] = 0)

                UPDATE
                    [WorkflowType]
                SET [LoggingLevel] = 0
                WHERE
                    [Guid] = '655BE2A4-2735-4CF9-AEC8-7EF5BE92724C'
                    AND [LoggingLevel] = 3" );
        }

    }
}
