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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Manager
{
    /// <summary>
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
    [Rock.SystemGuid.BlockTypeGuid( "17E8F764-562A-4E94-980D-FF1B15640670" )]
    public partial class SelectArea : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            // this used to be called "LocationPage", so we'll keep it that way
            public const string ManagerPage = "LocationPage";
            public const string CheckinConfigurationTypes = "CheckinConfigurationTypes";
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            upnlContent.OnPostBack += upnlContent_OnPostBack;

            rptNavItems.ItemDataBound += rptNavItems_ItemDataBound;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindData();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the OnPostBack event of the upnlContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PostBackEventArgs"/> instance containing the event data.</param>
        protected void upnlContent_OnPostBack( object sender, PostBackEventArgs e )
        {
            var checkinAreaGuid = e.EventArgument.AsGuid();
            CheckinManagerHelper.SaveSelectedCheckinAreaGuidToCookie( checkinAreaGuid );
            NavigateToLinkedPage( AttributeKey.ManagerPage );
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptNavItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptNavItems_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var groupType = e.Item.DataItem as GroupType;
            var li = e.Item.FindControl( "liNavItem" ) as HtmlGenericControl;
            if ( groupType != null && li != null )
            {
                li.Attributes["onClick"] = upnlContent.GetPostBackEventReference( groupType.Guid.ToString() );
            }
        }

        #endregion

        #region Methods

        private void BindData()
        {
            int? groupTypePurposeCheckinTemplateValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );

            using ( var rockContext = new RockContext() )
            {
                var checkinAreaTypeQuery = new GroupTypeService( rockContext )
                    .Queryable()
                    .Where( g => g.GroupTypePurposeValueId.HasValue && g.GroupTypePurposeValueId.Value == groupTypePurposeCheckinTemplateValueId );

                var checkinAreaTypeGuids = this.GetAttributeValues( AttributeKey.CheckinConfigurationTypes ).AsGuidList();
                if ( checkinAreaTypeGuids.Any() )
                {
                    checkinAreaTypeQuery = checkinAreaTypeQuery.Where( a => checkinAreaTypeGuids.Contains( a.Guid ) );
                }

                rptNavItems.DataSource = checkinAreaTypeQuery.ToList()
                    .OrderBy( g => g.Name )
                    .ToList();

                rptNavItems.DataBind();
            }
        }

        #endregion
    }
}