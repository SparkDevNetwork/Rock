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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.SystemKey;
using Rock.ViewModels.Blocks.Communication.CommunicationSettings;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Block used to set values specific to communication.
    /// </summary>
    [DisplayName( "Communication Settings" )]
    [Category( "Communication" )]
    [Description( "Block used to set values specific to communication." )]
    [IconCssClass( "ti ti-tool" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "0B3EA469-5969-4CD0-AE2A-7856F2EADF2B" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "7594EF64-737B-4CD8-B09F-89663A1AFDB0" )]
    [Rock.SystemGuid.BlockTypeGuid( "ED6447A6-F7E0-4680-BFD1-B45527C17156" )]
    public class CommunicationSettings : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new CommunicationSettingsBag
            {
                ApprovalEmailTemplate = Rock.Web.SystemSettings.GetValue( SystemSetting.COMMUNICATION_SETTING_APPROVAL_TEMPLATE ),
                ApprovalEmailTemplateOptions = GetApprovalEmailTemplateOptions()
            };
        }

        /// <summary>
        /// Gets the system communications available for selection as the approval
        /// notification template, ordered by title.
        /// </summary>
        /// <returns>The selectable system communications as list items keyed by Guid.</returns>
        private List<ListItemBag> GetApprovalEmailTemplateOptions()
        {
            return new SystemCommunicationService( RockContext )
                .Queryable()
                .OrderBy( c => c.Title )
                .Select( c => new ListItemBag { Text = c.Title, Value = c.Guid.ToString() } )
                .ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the communication settings.
        /// </summary>
        /// <param name="bag">The settings to save.</param>
        /// <returns>An OK result when saved, or a bad request when validation fails.</returns>
        [BlockAction]
        public BlockActionResult SaveSettings( CommunicationSettingsBag bag )
        {
            if ( string.IsNullOrWhiteSpace( bag?.ApprovalEmailTemplate ) )
            {
                return ActionBadRequest( "A Communication Approval Email Template is required." );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.COMMUNICATION_SETTING_APPROVAL_TEMPLATE, bag.ApprovalEmailTemplate );

            return ActionOk();
        }

        #endregion
    }
}
