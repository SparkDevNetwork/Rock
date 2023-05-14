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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Business List" )]
    [Category( "Finance" )]
    [Description( "Lists all businesses and provides filtering by business name" )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "1ACCF349-73A5-4568-B801-2A6A620791D9" )]
    public partial class BusinessList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

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
            gBusinessList.PersonIdField = "Id";
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
            gfBusinessFilter.SetFilterPreference( "Business Name", tbBusinessName.Text );
            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfBusinessFilter.SetFilterPreference( "Active Status", string.Empty );
            }
            else
            {
                gfBusinessFilter.SetFilterPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            // If it's there, strip the SearchTerm parameter from the query string and reload.
            if ( !string.IsNullOrWhiteSpace( PageParameter( "SearchTerm" ) ) )
            {
                var proxySafeUri = Request.UrlProxySafe();
                var parameters = System.Web.HttpUtility.ParseQueryString( proxySafeUri.Query );
                parameters.Remove( "SearchTerm" );
                string url = proxySafeUri.AbsolutePath + ( parameters.Count > 0 ? "?" + parameters.ToString() : string.Empty );
                Response.Redirect( url );
            }
            else
            {
                BindGrid();
            }
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
            parms.Add( "BusinessId", "0" );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
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
            parms.Add( "BusinessId", businessId.ToString() );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // Business Name Filter
            tbBusinessName.Text = gfBusinessFilter.GetFilterPreference( "Business Name" );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfBusinessFilter.GetFilterPreference( "Active Status" ) );
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
            var recordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            var businessQueryable = new PersonService( rockContext ).Queryable()
                .Where( q => q.RecordTypeValueId == recordTypeValueId );

            var businessName = string.Empty;
            bool viaSearch = false;

            // Use the name passed in the page parameter if given
            if ( !string.IsNullOrWhiteSpace( PageParameter( "SearchTerm" ) ) )
            {
                viaSearch = true;
                gfBusinessFilter.Visible = false;
                businessName = PageParameter( "SearchTerm" );
            }
            else
            {
                // Business Name Filter
                businessName = gfBusinessFilter.GetFilterPreference( "Business Name" );
            }

            if ( !string.IsNullOrWhiteSpace( businessName ) )
            {
                businessQueryable = businessQueryable.Where( a => a.LastName.Contains( businessName ) );
            }

            if ( !viaSearch )
            {
                var activeRecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                string activeFilterValue = gfBusinessFilter.GetFilterPreference( "Active Status" );
                if ( activeFilterValue == "inactive" )
                {
                    businessQueryable = businessQueryable.Where( b => b.RecordStatusValueId != activeRecordStatusValueId );
                }
                else if ( activeFilterValue == "active" )
                {
                    businessQueryable = businessQueryable.Where( b => b.RecordStatusValueId == activeRecordStatusValueId );
                }
            }

            bool showBusinessDetail = false;
            if ( viaSearch )
            {
                // if we got here from Business Search, and there is exactly one business that matches the search, continue in ShowBusinessDetail mode and don't do any of the grid related stuff
                showBusinessDetail = businessQueryable.Count() == 1;
            }

            if ( showBusinessDetail )
            {
                var businessId = businessQueryable.Select( a => a.Id ).FirstOrDefault();
                ShowDetailForm( businessId );
            }
            else
            {
                var workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() ).Id;

                int groupTypeRoleIdKnownRelationShipsOwner = 0;
                int groupTypeIdKnownRelationShips = 0;
                var groupTypeKnownRelationShips = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );

                if ( groupTypeKnownRelationShips != null )
                {
                    groupTypeIdKnownRelationShips = groupTypeKnownRelationShips.Id;
                    groupTypeRoleIdKnownRelationShipsOwner = groupTypeKnownRelationShips.Roles.Where( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ).Select( a => a.Id ).FirstOrDefault();
                }

                var businessSelectQry = businessQueryable
                    .Select( b => new BusinessSelectInfo
                    {
                        Id = b.Id,
                        LastName = b.LastName,
                        BusinessName = b.LastName,
                        PhoneNumber = b.PhoneNumbers.FirstOrDefault().NumberFormatted,
                        Email = b.Email,
                        Address = b.GivingGroup.GroupLocations
                                            .Where( gl => gl.GroupLocationTypeValueId == workLocationTypeId )
                                            .FirstOrDefault()
                                            .Location,
                        Contacts = b.Members
                                            .Where( m => m.Group.GroupTypeId == groupTypeIdKnownRelationShips )
                                            .SelectMany( m => m.Group.Members )
                                            .Where( p => p.GroupRoleId == groupTypeRoleIdKnownRelationShipsOwner && p.PersonId != b.Id )
                                            .Select( p => p.Person.LastName + ", " + p.Person.NickName ),
                        Campus = b.PrimaryCampus == null ? "" : b.PrimaryCampus.Name
                    } );

                SortProperty sortProperty = gBusinessList.SortProperty;
                if ( sortProperty != null )
                {
                    businessSelectQry = businessSelectQry.Sort( sortProperty );
                }
                else
                {
                    businessSelectQry = businessSelectQry.OrderBy( q => q.LastName );
                }

                gBusinessList.EntityTypeId = EntityTypeCache.Get<Person>().Id;
                gBusinessList.SetLinqDataSource( businessSelectQry );
                gBusinessList.DataBind();
            }
        }

        /// <summary>
        /// Shows the detail form.
        /// </summary>
        /// <param name="id">The id.</param>
        protected void ShowDetailForm( int id )
        {
            NavigateToLinkedPage( "DetailPage", "businessId", id );
        }

        protected string FormatContactInfo( string phone, string address )
        {
            var values = new List<string> { phone, address, "&nbsp;", "&nbsp;" };
            return values.Where( v => v.IsNotNullOrWhiteSpace() ).Take( 2 ).ToList().AsDelimited( "<br/>" );
        }

        #endregion Internal Methods

        /// <summary>
        /// Handles the DataBound event of the lContactInformation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lContactInformation_DataBound( object sender, RowEventArgs e )
        {
            Literal lContactInformation = e.Row.FindControl( "lContactInformation" ) as Literal;
            BusinessSelectInfo businessSelectInfo = e.Row.DataItem as BusinessSelectInfo;
            if ( lContactInformation != null && businessSelectInfo != null )
            {
                lContactInformation.Text = FormatContactInfo( businessSelectInfo.PhoneNumber, businessSelectInfo.Email );
            }
        }

        /// <summary>
        /// Handles the DataBound event of the lAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lAddress_DataBound( object sender, RowEventArgs e )
        {
            Literal lAddress = e.Row.FindControl( "lAddress" ) as Literal;
            BusinessSelectInfo businessSelectInfo = e.Row.DataItem as BusinessSelectInfo;
            if ( lAddress != null && businessSelectInfo != null && businessSelectInfo.Address != null )
            {
                lAddress.Text = businessSelectInfo.Address.FormattedHtmlAddress;
            }
        }

        /// <summary>
        /// Handles the DataBound event of the lContacts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void lContacts_DataBound( object sender, RowEventArgs e )
        {
            Literal lContacts = e.Row.FindControl( "lContacts" ) as Literal;
            BusinessSelectInfo businessSelectInfo = e.Row.DataItem as BusinessSelectInfo;
            if ( lContacts != null && businessSelectInfo != null )
            {
                lContacts.Text = businessSelectInfo.Contacts.ToList().AsDelimited( "<br />" );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="Rock.Utility.RockDynamic" />
        private class BusinessSelectInfo : RockDynamic
        {
            public int Id { get; set; }
            public string LastName { get; set; }
            public string BusinessName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public Location Address { get; set; }
            public IEnumerable<string> Contacts { get; set; }

            public string Campus { get; set; }
        }
    }
}