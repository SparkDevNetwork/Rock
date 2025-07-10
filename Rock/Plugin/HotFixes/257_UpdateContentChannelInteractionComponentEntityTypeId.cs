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

using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 257, "17.3" )]
    public class UpdateContentChannelInteractionComponentEntityTypeId : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateContentChannelInteractionComponentEntityTypeIdUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateContentChannelInteractionComponentEntityTypeIdDown();
        }

        #region MSE - Fix InteractionChannel ComponentEntityTypeId from ContentChannel (209) to ContentChannelItem (208) for ContentChannelItem interactions

        private void UpdateContentChannelInteractionComponentEntityTypeIdUp()
        {
            Sql( @"
UPDATE [InteractionChannel]
SET [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = 'BF12AE64-21FB-433B-A8A4-E40E8C426DDA'
)
WHERE [ChannelTypeMediumValueId] = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'F1A19D09-E010-EEB3-465A-940A6F023CEB'
)
AND [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = '44484685-477E-4668-89A6-84F29739EB68'
);" );
        }

        private void UpdateContentChannelInteractionComponentEntityTypeIdDown()
        {
            // Revert InteractionChannel.ComponentEntityTypeId back to ContentChannel (209) from ContentChannelItem (208)
            // for all ContentChannel interaction channels
            Sql( @"
UPDATE [InteractionChannel]
SET [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = '44484685-477E-4668-89A6-84F29739EB68'
)
WHERE [ChannelTypeMediumValueId] = (
    SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'F1A19D09-E010-EEB3-465A-940A6F023CEB'
)
AND [ComponentEntityTypeId] = (
    SELECT [Id] FROM [EntityType] WHERE [Guid] = 'BF12AE64-21FB-433B-A8A4-E40E8C426DDA'
);" );
        }

        #endregion
    }
}