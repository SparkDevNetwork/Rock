// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business List" )]
    [Category( "Finance" )]
    [Description( "Lists all businesses and provides filtering by business name and owner" )]
    [LinkedPage( "Detail Page" )]
    public partial class BusinessList : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gfBusinessFilter.ApplyFilterClick += gfBusinessFilter_ApplyFilterClick;
            gfBusinessFilter.DisplayFilterValue += gfBusinessFilter_DisplayFilterValue;

            gBusinessList.DataKeyNames = new string[] { "Id" };
            gBusinessList.Actions.ShowAdd = canEdit;
            gBusinessList.Actions.AddClick += gBusinessList_AddClick;
            gBusinessList.GridRebind += gBusinessList_GridRebind;
            gBusinessList.IsDeleteEnabled = canEdit;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfBusinessFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs"/> instance containing the event data.</param>
        private void gfBusinessFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Business Name":
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfBusinessFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gfBusinessFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfBusinessFilter.SaveUserPreference( "Business Name", tbBusinessName.Text );
            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfBusinessFilter.SaveUserPreference( "Active Status", string.Empty );
            }
            else
            {
                gfBusinessFilter.SaveUserPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBusinessList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBusinessList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gBusinessList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBusinessList_AddClick( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "businessId", "0" );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the RowSelected event of the gBusinessList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gBusinessList_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            var businessId = e.RowKeyId;
            parms.Add( "businessId", businessId.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }


        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var rockContext = new RockContext();

            // Business Name Filter
            tbBusinessName.Text = gfBusinessFilter.GetUserPreference( "Business Name" );
            
            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfBusinessFilter.GetUserPreference( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var recordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            var queryable = new PersonService( rockContext ).Queryable()
                .Where( q => q.RecordTypeValueId == recordTypeValueId );

            // Business Name Filter
            var businessName = gfBusinessFilter.GetUserPreference( "Business Name" );
            if ( !string.IsNullOrWhiteSpace( businessName ) )
            {
                queryable = queryable.Where( a => a.LastName.Contains( businessName ) );
            }

            var activeRecordStatusValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            string activeFilterValue = gfBusinessFilter.GetUserPreference( "Active Status" );
            if (activeFilterValue == "inactive")
            {
                queryable = queryable.Where( b => b.RecordStatusValueId != activeRecordStatusValueId );
            }
            else if (activeFilterValue == "active")
            {
                queryable = queryable.Where( b => b.RecordStatusValueId == activeRecordStatusValueId );
            }

            SortProperty sortProperty = gBusinessList.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( q => q.LastName );
            }

            var groupMemberQuery = new GroupMemberService( rockContext ).Queryable();

            var businessList = queryable.Select( b => new
            {
                Id = b.Id,
                b.LastName,
                BusinessName = b.LastName,
                PhoneNumber = b.PhoneNumbers.FirstOrDefault().NumberFormatted,
                Email = b.Email,
                Address = b.Members
                                .Where( m => m.Group.GroupType.Guid.ToString() == Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY )
                                .SelectMany( m => m.Group.GroupLocations )
                                .FirstOrDefault()
                                .Location,
                Contacts = b.Members
                                .Where( m => m.Group.GroupType.Guid.ToString() == Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS )
                                .SelectMany( m => m.Group.Members)
                                .Where( p => p.GroupRole.Guid.ToString() == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER && p.PersonId != b.Id)
                                .Select( p => p.Person.LastName + ", " + p.Person.NickName)
            } );

            gBusinessList.EntityTypeId = EntityTypeCache.Read<Person>().Id;
            gBusinessList.DataSource = businessList.ToList();
            gBusinessList.DataBind();
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToLinkedPage( "DetailPage", "businessId", id );
        }

        #endregion Internal Methods
    }
}