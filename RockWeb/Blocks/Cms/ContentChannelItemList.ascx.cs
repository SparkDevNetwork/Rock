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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.ComponentModel;
using Rock.Security;
using Rock.Web.Cache;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Channel Item List")]
    [Category("CMS")]
    [Description("Lists content channel items.")]

    [LinkedPage("Detail Page")]
    public partial class ContentChannelItemList : RockBlock, ISecondaryBlock
    {
        #region Fields

        private int? _channelId = null;
        private int _typeId = 0;
        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _channelId = PageParameter( "contentChannelId" ).AsIntegerOrNull();
            string cssIcon = "fa fa-bullhorn";
            var contentChannel = new ContentChannelService( new RockContext() ).Get( _channelId.Value );
            if ( contentChannel != null )
            {
                gItems.Columns[1].HeaderText = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange ? "Start" : "Active";
                gItems.Columns[2].Visible = contentChannel.ContentChannelType.DateRangeType == ContentChannelDateType.DateRange;
                gItems.Columns[3].Visible = !contentChannel.ContentChannelType.DisablePriority;
                lContentChannel.Text = contentChannel.Name;
                _typeId = contentChannel.ContentChannelTypeId;

                if ( !string.IsNullOrWhiteSpace( contentChannel.IconCssClass ) )
                {
                    cssIcon = contentChannel.IconCssClass;
                }
            }

            lIcon.Text = string.Format( "<i class='{0}'></i>", cssIcon );

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
            gfFilter.DisplayFilterValue += gfFilter_DisplayFilterValue;

            gItems.DataKeyNames = new string[] { "Id" };
            gItems.Actions.ShowAdd = canAddEditDelete;
            gItems.IsDeleteEnabled = canAddEditDelete;
            gItems.Actions.AddClick += gItems_Add;
            gItems.GridRebind += gItems_GridRebind;

            AddAttributeColumns();

            if ( contentChannel != null && contentChannel.RequiresApproval )
            {
                var statusField = new BoundField();
                gItems.Columns.Add( statusField );
                statusField.DataField = "Status";
                statusField.HeaderText = "Status";
                statusField.SortExpression = "Status";
                statusField.HtmlEncode = false;
            }
           
            var securityField = new SecurityField();
            gItems.Columns.Add( securityField );
            securityField.TitleField = "Title";
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannelItem ) ).Id;

            var deleteField = new DeleteField();
            gItems.Columns.Add( deleteField );
            deleteField.Click += gItems_Delete;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
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
                BindGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Events

        /// <summary>
        /// Gfs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Status":
                    {
                        var status = e.Value.ConvertToEnumOrNull<ContentChannelItemStatus>();
                        if ( status.HasValue )
                        {
                            {
                                e.Value = status.ConvertToString();
                            }
                        }
                        break;
                    }
                case "Title":
                    {
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
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SaveUserPreference( "Date Range", drpDateRange.DelimitedValues );
            gfFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            gfFilter.SaveUserPreference( "Title", tbTitle.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gItems_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentItemId", 0, "contentChannelId", _channelId );
        }

        /// <summary>
        /// Handles the Edit event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentItemId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItems_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelItemService contentItemService = new ContentChannelItemService( rockContext );

            ContentChannelItem contentItem = contentItemService.Get( e.RowKeyId );

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
        /// Handles the GridRebind event of the gItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gItems_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on it's page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        protected void AddAttributeColumns()
        {
            // Remove attribute columns
            foreach ( var column in gItems.Columns.OfType<AttributeField>().ToList() )
            {
                gItems.Columns.Remove( column );
            }

            // Add attribute columns
            int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannelItem ) ).Id;
            string qualifier = _typeId.ToString();
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn &&
                    a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifier ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = attribute.Key;
                bool columnExists = gItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.HeaderText = attribute.Name;
                    boundField.SortExpression = string.Empty;

                    var attributeCache = Rock.Web.Cache.AttributeCache.Read( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gItems.Columns.Add( boundField );
                }
            }
        }

        private void BindFilter()
        {
            drpDateRange.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
            ddlStatus.BindToEnum<ContentChannelItemStatus>( true );
            int? statusID = gfFilter.GetUserPreference( "Status" ).AsIntegerOrNull();
            if ( statusID.HasValue )
            {
                ddlStatus.SetValue( statusID.Value.ToString() );
            }

            tbTitle.Text = gfFilter.GetUserPreference( "Title" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _channelId.HasValue )
            {
                ContentChannelItemService contentItemService = new ContentChannelItemService( new RockContext() );
                var contentItems = contentItemService.Queryable()
                    .Where( c => c.ContentChannelId == _channelId.Value );

                var drp = new DateRangePicker();
                drp.DelimitedValues = gfFilter.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    contentItems = contentItems.Where( i =>
                        ( i.ExpireDateTime.HasValue && i.ExpireDateTime.Value >= drp.LowerValue.Value ) ||
                        ( !i.ExpireDateTime.HasValue && i.StartDateTime >= drp.LowerValue.Value ) );
                }
                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    contentItems = contentItems.Where( i => i.StartDateTime < upperDate );
                }

                var status = gfFilter.GetUserPreference( "Status" ).ConvertToEnumOrNull<ContentChannelItemStatus>();
                if ( status.HasValue )
                {
                    contentItems = contentItems.Where( i => i.Status == status );
                }

                string title = gfFilter.GetUserPreference( "Title" );
                if ( !string.IsNullOrWhiteSpace( title ) )
                {
                    contentItems = contentItems.Where( i => i.Title.Contains( title ) );
                }

                // TODO: Checking security of every item will take longer and longer as more items are added.  
                // Eventually we should implement server-side paging so that we only need to check security for
                // the items on the current page

                var items = new List<ContentChannelItem>();
                foreach ( var item in contentItems.ToList())
                {
                    if ( item.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ))
                    {
                        items.Add(item);
                    }
                }

                SortProperty sortProperty = gItems.SortProperty;
                if ( sortProperty != null )
                {
                    items = items.AsQueryable().Sort( sortProperty ).ToList();
                }
                else
                {
                    items = items.OrderByDescending( p => p.StartDateTime ).ToList();
                }

                gItems.ObjectList = new Dictionary<string, object>();
                items.ForEach( i => gItems.ObjectList.Add( i.Id.ToString(), i ) );
                gItems.EntityTypeId = EntityTypeCache.Read<ContentChannelItem>().Id;

                gItems.DataSource = items.Select( i => new
                {
                    i.Id,
                    i.Guid,
                    i.Title,
                    i.StartDateTime,
                    i.ExpireDateTime,
                    i.Priority,
                    Status = DisplayStatus( i.Status )
                } ).ToList();
                gItems.DataBind();
            }
        }


        protected string DisplayStatus (ContentChannelItemStatus contentItemStatus)
        {
            string labelType = "default";
            if ( contentItemStatus == ContentChannelItemStatus.Approved )
            {
                labelType = "success";
            }
            else if ( contentItemStatus == ContentChannelItemStatus.Denied )
            {
                labelType = "danger";
            }

            return string.Format( "<span class='label label-{0}'>{1}</span>", labelType, contentItemStatus.ConvertToString() );
        }

        #endregion
    }
}