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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content - Item List")]
    [Category("CMS")]
    [Description("Lists content items.")]
    [LinkedPage("Detail Page")]
    public partial class ContentItemList : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            gContentItems.DataKeyNames = new string[] { "Id" };
            gContentItems.Actions.AddClick += gContentItems_Add;
            gContentItems.GridRebind += gContentItems_GridRebind;
            gContentItems.EmptyDataText = Server.HtmlEncode( None.Text );

            // Block Security on Ads grid (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gContentItems.Actions.ShowAdd = canAddEditDelete;
            gContentItems.IsDeleteEnabled = canAddEditDelete;
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

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Content Type", ddlContentType.SelectedValue );
            
            rFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            
            rFilter.SaveUserPreference( "Date Range", string.Format( "{0},{1}",
                pDateRange.LowerValue.HasValue ? pDateRange.LowerValue.Value.ToString( "d" ) : string.Empty,
                pDateRange.UpperValue.HasValue ? pDateRange.UpperValue.Value.ToString( "d" ) : string.Empty ) );

            rFilter.SaveUserPreference( "Priority Range", string.Format( "{0},{1}", 
                pPriorityRange.LowerValue, 
                pPriorityRange.UpperValue ) );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Content Type":
                    {
                        var contentType = new ContentTypeService( new RockContext() ).Get( e.Value.AsInteger() );
                        if ( contentType != null )
                        {
                            e.Value = contentType.Name;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Status":
                    {
                        int approvalStatusValue = e.Value.AsIntegerOrNull() ?? Rock.Constants.All.Id;
                        if ( approvalStatusValue != Rock.Constants.All.Id )
                        {
                            e.Value = e.Value.ConvertToEnum<ContentItemStatus>().ConvertToString();
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                case "Date Range":
                case "Priority Range":
                    {
                        string[] values = e.Value.Split( new char[] { ',' }, StringSplitOptions.None );
                        if ( values.Length == 2 )
                        {
                            if ( string.IsNullOrWhiteSpace( values[0] ) && string.IsNullOrWhiteSpace( values[1] ) )
                            {
                                e.Value = Rock.Constants.All.Text;
                            }
                            else if ( string.IsNullOrWhiteSpace( values[0] ) )
                            {
                                e.Value = "less than " + values[1];
                            }
                            else if ( string.IsNullOrWhiteSpace( values[1] ) )
                            {
                                e.Value = "greater than " + values[0];
                            }
                            else
                            {
                                e.Value = string.Format( "{0} to {1}", values[0], values[1] );
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }

                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }

            }
        }

        /// <summary>
        /// Handles the Add event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentItems_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentItemId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentItems_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentItemId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentItems_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ContentItemService contentItemService = new ContentItemService( rockContext );

            ContentItem contentItem = contentItemService.Get( e.RowKeyId );
            if ( contentItem != null )
            {
                string errorMessage;
                if ( !contentItemService.CanDelete( contentItem, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                contentItemService.Delete( contentItem );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentItems_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ContentTypeService contentTypeService = new ContentTypeService( new RockContext() );
            var contentTypeList = contentTypeService.Queryable().Select( a => new { a.Id, a.Name } ).OrderBy( a => a.Name ).ToList();
            foreach ( var contentType in contentTypeList )
            {
                ddlContentType.Items.Add( new ListItem( contentType.Name, contentType.Id.ToString() ) );
            }
            ddlContentType.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            ddlContentType.SetValue( rFilter.GetUserPreference( "Content Type" ) );

            ddlStatus.BindToEnum<ContentItemStatus>( true );
            ddlStatus.SetValue( rFilter.GetUserPreference( "Status" ) );

            string dateRangeValues = rFilter.GetUserPreference( "Date Range" );
            if ( !string.IsNullOrWhiteSpace( dateRangeValues ) )
            {
                string[] upperLowerValues = dateRangeValues.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( upperLowerValues.Length == 2 )
                {
                    string lowerValue = upperLowerValues[0];
                    if ( !string.IsNullOrWhiteSpace( lowerValue ) )
                    {
                        pDateRange.LowerValue = DateTime.Parse( lowerValue );
                    }
                    string upperValue = upperLowerValues[1];
                    if ( !string.IsNullOrWhiteSpace( upperValue ) )
                    {
                        pDateRange.UpperValue = DateTime.Parse( upperValue );
                    }
                }
            }

            string priorityRangeValues = rFilter.GetUserPreference( "Priority Range" );
            if ( !string.IsNullOrWhiteSpace( priorityRangeValues ) )
            {
                string[] upperLowerValues = priorityRangeValues.Split( new char[] { ',' }, StringSplitOptions.None );
                if ( upperLowerValues.Length == 2 )
                {
                    pPriorityRange.LowerValue = upperLowerValues[0].AsIntegerOrNull();
                    pPriorityRange.UpperValue = upperLowerValues[1].AsIntegerOrNull();
                }
            }

        }

        /// <summary>
        /// Binds the marketing campaign ads grid.
        /// </summary>
        private void BindGrid()
        {
            ContentItemService contentItemService = new ContentItemService( new RockContext() );
            var qry = contentItemService.Queryable( "ContentType" );

            // Ad Type
            int? contentTypeId = ddlContentType.SelectedValueAsInt();
            if ( contentTypeId.HasValue )
            {
                qry = qry.Where( a => a.ContentTypeId == contentTypeId.Value );
            }

            // Status
            var status = ddlStatus.SelectedValueAsEnumOrNull<ContentItemStatus>();
            if ( status.HasValue )
            {
                qry = qry.Where( a => a.Status == status.Value );
            }

            // Date Range
            if ( pDateRange.LowerValue.HasValue )
            {
                DateTime startDate = pDateRange.LowerValue.Value.Date;
                qry = qry.Where( a => (
                    ( a.ExpireDateTime.HasValue && a.ExpireDateTime >= startDate ) ||
                    ( !a.ExpireDateTime.HasValue && a.StartDateTime >= startDate )
                ) );
            }
            if ( pDateRange.UpperValue.HasValue )
            {
                // add a whole day to the selected endDate since users will expect to see all the stuff that happened 
                // on the endDate up until the very end of that day.

                // calculate the query endDate before including it in the qry statement to avoid Linq error
                var endDate = pDateRange.UpperValue.Value.AddDays( 1 );
                qry = qry.Where( a => a.StartDateTime < endDate );
            }

            // Priority Range
            if ( pPriorityRange.LowerValue.HasValue )
            {
                int lowerValue = (int)pPriorityRange.LowerValue.Value;
                qry = qry.Where( a => a.Priority >= lowerValue );
            }
            if ( pPriorityRange.UpperValue.HasValue )
            {
                int upperValue = (int)pPriorityRange.UpperValue.Value;
                qry = qry.Where( a => a.Priority <= upperValue );
            }

            SortProperty sortProperty = gContentItems.SortProperty;

            if ( sortProperty != null )
            {
                if ( sortProperty.Equals( "Approver" ) )
                {
                    qry = qry.OrderBy( a => a.ApprovedByPersonAlias.Person.LastName )
                        .ThenBy( a => a.ApprovedByPersonAlias.Person.NickName )
                        .ThenByDescending( a => a.StartDateTime )
                        .ThenBy( a => a.Priority )
                        .ThenBy( a => a.Title )
                        .ThenBy( a => a.ContentType.Name );
                }
                else
                {
                    qry = qry.Sort( sortProperty );
                }
            }
            else
            {
                qry = qry.OrderByDescending( a => a.StartDateTime )
                    .ThenBy( a => a.Priority )
                    .ThenBy( a => a.Title )
                    .ThenBy( a => a.ContentType.Name );
            }

            gContentItems.DataSource = qry.Select( i => new
            {
                i.Id,
                i.Title,
                ContentType = i.ContentType.Name,
                i.StartDateTime,
                i.ExpireDateTime,
                i.Priority,
                i.Status,
                Approver = ( i.ApprovedByPersonAlias != null && i.ApprovedByPersonAlias.Person != null ) ?
                    i.ApprovedByPersonAlias.Person.FullName : ""
            } ).ToList();

            gContentItems.DataBind();
        }

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [dimmed].</param>
        public void SetVisible( bool visible )
        {
            pnlContentItems.Visible = visible;
        }

        #endregion
    }
}