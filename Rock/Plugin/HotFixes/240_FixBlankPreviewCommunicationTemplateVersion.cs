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

using Rock.Model;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber(240, "1.17.0")]
    public class FixBlankPreviewCommunicationTemplateVersion : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JMH_FixBlankPreviewCommunicationTemplateVersionUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // No down as this migration fixed the Blank (Preview) communication template's version
            // and it should not be reverted.
        }

        private void JMH_FixBlankPreviewCommunicationTemplateVersionUp()
        {
            // Update the Blank (Preview) communication template to have the new Beta version.
            Sql( $@"UPDATE [CommunicationTemplate]
SET [Version] = {( int ) CommunicationTemplateVersion.Beta}
WHERE [Guid] = '6280214C-404E-4F4E-BC33-7A5D4CDF8DBC'" );
        }
    }
}