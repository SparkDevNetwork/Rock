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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn.Config
{
    /// <summary>
    /// Displays the calendars that user is authorized to view.
    /// </summary>
    [DisplayName( "Check-in Types" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Displays the check-in types." )]

    [Rock.SystemGuid.BlockTypeGuid( "50029382-75A6-4B73-9644-880845B3116A" )]
    public partial class CheckinTypes : Rock.Web.UI.RockBlock
    {
        private int _templatePurposeId = 0;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var templatePurpose = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
            if ( templatePurpose != null )
            {
                _templatePurposeId = templatePurpose.Id;
                lbAddCheckinType.Visible = UserCanAdministrate;
            }
            else
            {
                lbAddCheckinType.Visible = false;
            }

            rptCheckinTypes.ItemCommand += rptCheckinTypes_ItemCommand;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                GetData();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetData();
        }

        /// <summary>
        /// Handles the Click event of the lbAddCheckinType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCheckinType_Click( object sender, EventArgs e )
        {
            var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
            pageRef.Parameters.Add( "CheckinTypeId", "0" );
            NavigateToPage( pageRef );
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptCheckinTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCheckinTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? groupTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( groupTypeId.HasValue )
            {
                var pageRef = new PageReference( CurrentPageReference.PageId, CurrentPageReference.RouteId );
                pageRef.Parameters.Add( "CheckinTypeId", groupTypeId.Value.ToString() );
                NavigateToPage( pageRef );
            }
        }

        #endregion

        #region Methods

        private void GetData()
        {
            int activeTypeId = PageParameter( "CheckinTypeId" ).AsInteger();

            using ( var rockContext = new RockContext() )
            {
                var groupTypes = new List<GroupType>();

                // Get all of the check-in template group types that user is authorized to view
                foreach ( var groupType in new GroupTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t =>
                        t.GroupTypePurposeValueId.HasValue &&
                        t.GroupTypePurposeValueId.Value == _templatePurposeId )
                    .OrderBy( t => t.Name ) )
                {
                    if ( groupType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        groupTypes.Add( groupType );
                    }
                }

                rptCheckinTypes.DataSource = groupTypes.Select( t => new
                {
                    t.Id,
                    t.Name,
                    IconCssClass = string.IsNullOrWhiteSpace( t.IconCssClass ) ? "fa fa-sign-in" : t.IconCssClass,
                    ActiveCssClass = t.Id == activeTypeId ? "active" : ""
                } );
                rptCheckinTypes.DataBind();
            }
        }

        #endregion

    }
}