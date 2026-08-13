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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.ViewModels.Blocks.CheckIn.Manager.SelectArea;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Displays the list of Check-in Configurations (areas) that a Check-in
    /// Manager attendant can select. Clicking an area saves its Guid to the
    /// shared Check-in Manager cookie and navigates to the configured
    /// manager page.
    /// </summary>
    [DisplayName( "Select Check-In Area" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block used to select the check-in area (Check-in Configuration) for Check-in Manager." )]

    [LinkedPage(
        "Check-in Manager Page",
        Key = AttributeKey.ManagerPage,
        Order = 2 )]

    [CheckinConfigurationTypeField(
        "Check-in Areas",
        Description = "Select the Check Areas to display, or select none to show all.",
        Key = AttributeKey.CheckinConfigurationTypes,
        Order = 3 )]

    [Rock.SystemGuid.EntityTypeGuid( "B3071C02-D96C-449B-B91C-3A4C3862B371" )]
    [Rock.SystemGuid.BlockTypeGuid( "17E8F764-562A-4E94-980D-FF1B15640670" )]
    public class SelectArea : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            /*
                8/13/26 - NA

                The block-setting key must remain "LocationPage" so upgrades
                do not wipe out existing configurations that predate the
                display-name change to "Check-in Manager Page".

                Reason: Preserve backward compatibility with stored attribute values.
            */
            public const string ManagerPage = "LocationPage";
            public const string CheckinConfigurationTypes = "CheckinConfigurationTypes";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new SelectAreaOptionsBag
            {
                Areas = GetCheckInAreas(),
                ManagerPageUrl = this.GetLinkedPageUrl( AttributeKey.ManagerPage )
            };
        }

        /// <summary>
        /// Returns the check-in areas (GroupTypes whose purpose is
        /// "Check-in Template"), optionally filtered by the block-setting
        /// list of area Guids, ordered by Name.
        /// </summary>
        /// <returns>A list of items whose Value is the area Guid and Text is the area Name.</returns>
        private System.Collections.Generic.List<ListItemBag> GetCheckInAreas()
        {
            var checkinTemplatePurposeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );

            if ( !checkinTemplatePurposeValueId.HasValue )
            {
                return new System.Collections.Generic.List<ListItemBag>();
            }

            var configuredAreaGuids = GetAttributeValues( AttributeKey.CheckinConfigurationTypes ).AsGuidList();

            var areas = GroupTypeCache.All()
                .Where( gt => gt.GroupTypePurposeValueId == checkinTemplatePurposeValueId.Value );

            if ( configuredAreaGuids.Any() )
            {
                areas = areas.Where( gt => configuredAreaGuids.Contains( gt.Guid ) );
            }

            return areas
                .OrderBy( gt => gt.Name )
                .Select( gt => new ListItemBag
                {
                    Value = gt.Guid.ToString(),
                    Text = gt.Name
                } )
                .ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Persists the selected check-in area to the shared Check-in Manager
        /// cookie and returns the URL for the configured manager page.
        /// </summary>
        /// <param name="areaGuid">The Guid of the selected check-in area (GroupType).</param>
        /// <returns>A response containing the redirect URL, or an empty URL when the manager page setting is not configured.</returns>
        [BlockAction]
        public BlockActionResult SetSelectedArea( Guid areaGuid )
        {
            if ( areaGuid == Guid.Empty )
            {
                return ActionBadRequest( "A check-in area must be selected." );
            }

            CheckinManagerHelper.SaveSelectedCheckinAreaGuidToCookie( areaGuid );

            return ActionOk( new
            {
                RedirectUrl = this.GetLinkedPageUrl( AttributeKey.ManagerPage )
            } );
        }

        #endregion
    }
}
