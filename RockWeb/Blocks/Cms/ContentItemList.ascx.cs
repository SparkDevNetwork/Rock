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

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Item List")]
    [Category("CMS")]
    [Description("Lists content items.")]

    [LinkedPage("Detail Page")]
    public partial class ContentItemList : RockBlock, ISecondaryBlock
    {
        #region Fields

        private int? _contentChannelId = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _contentChannelId = PageParameter( "contentChannelId" ).AsIntegerOrNull();

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gContentItems.DataKeyNames = new string[] { "id" };
            gContentItems.Actions.ShowAdd = canAddEditDelete;
            gContentItems.IsDeleteEnabled = canAddEditDelete;
            gContentItems.Actions.AddClick += gContentItems_Add;
            gContentItems.GridRebind += gContentItems_GridRebind;

            AddAttributeColumns();

            var securityField = new SecurityField();
            gContentItems.Columns.Add( securityField );
            securityField.TitleField = "Title";
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentItem ) ).Id;

            var deleteField = new DeleteField();
            gContentItems.Columns.Add( deleteField );
            deleteField.Click += gContentItems_Delete;

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
                if (_contentChannelId.HasValue)
                {
                    var contentChannel = new ContentChannelService( new RockContext() ).Get( _contentChannelId.Value );
                    if (contentChannel != null)
                    {
                        lContentChannel.Text = contentChannel.Name;
                    }
                }
                BindGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the gContentItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentItems_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentItemId", 0, "contentChannelId", _contentChannelId );
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

            ContentItem contentItem = contentItemService.Get( (int)e.RowKeyValue );

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
        private void gContentItems_GridRebind( object sender, EventArgs e )
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
            foreach ( var column in gContentItems.Columns.OfType<AttributeField>().ToList() )
            {
                gContentItems.Columns.Remove( column );
            }

            if ( _contentChannelId.HasValue )
            {
                // Add attribute columns
                int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentItem ) ).Id;
                string qualifier = _contentChannelId.Value.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifier ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gContentItems.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
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

                        gContentItems.Columns.Add( boundField );
                    }
                }
            }
        }
        
        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _contentChannelId.HasValue )
            {
                ContentItemService contentItemService = new ContentItemService( new RockContext() );
                SortProperty sortProperty = gContentItems.SortProperty;
                var contentItems = contentItemService.Queryable()
                    .Where( c => c.ContentChannelId == _contentChannelId.Value );

                if ( sortProperty != null )
                {
                    gContentItems.DataSource = contentItems.Sort( sortProperty ).ToList();
                }
                else
                {
                    gContentItems.DataSource = contentItems.OrderByDescending( p => p.StartDateTime ).ToList();
                }

                gContentItems.DataBind();
            }
        }


        protected string DisplayStatus (object status)
        {
            var contentItemStatus = (ContentItemStatus)status;

            string labelType = "warning";
            if ( contentItemStatus == ContentItemStatus.Approved )
            {
                labelType = "success";
            }
            else if ( contentItemStatus == ContentItemStatus.Denied )
            {
                labelType = "danger";
            }

            return string.Format( "<span class='label label-{0}'>{1}</span>", labelType, contentItemStatus.ConvertToString() );
        }

        #endregion
    }
}