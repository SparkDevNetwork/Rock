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
    [DisplayName("Content Channel List")]
    [Category("CMS")]
    [Description("Lists content channels.")]

    [LinkedPage("Detail Page")]
    public partial class ContentChannelList : RockBlock, ISecondaryBlock
    {
        #region Fields

        private int? _contentTypeId = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _contentTypeId = PageParameter( "contentTypeId" ).AsIntegerOrNull();

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );

            gContentChannels.DataKeyNames = new string[] { "id" };
            gContentChannels.Actions.ShowAdd = canAddEditDelete;
            gContentChannels.IsDeleteEnabled = canAddEditDelete;
            gContentChannels.Actions.AddClick += gContentChannels_Add;
            gContentChannels.GridRebind += gContentChannels_GridRebind;

            AddAttributeColumns();

            var securityField = new SecurityField();
            gContentChannels.Columns.Add( securityField );
            securityField.TitleField = "Name";
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannel ) ).Id;

            var deleteField = new DeleteField();
            gContentChannels.Columns.Add( deleteField );
            deleteField.Click += gContentChannels_Delete;

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
                if (_contentTypeId.HasValue)
                {
                    var contentType = new ContentTypeService( new RockContext() ).Get( _contentTypeId.Value );
                    if (contentType != null)
                    {
                        lContentType.Text = contentType.Name;
                    }
                }
                BindGrid();
            }

            base.OnLoad( e );
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the Add event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentChannelId", 0, "contentTypeId", _contentTypeId );
        }

        /// <summary>
        /// Handles the Edit event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "contentChannelId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContentChannels_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            ContentChannel contentChannel = contentChannelService.Get( (int)e.RowKeyValue );

            if ( contentChannel != null )
            {
                string errorMessage;
                if ( !contentChannelService.CanDelete( contentChannel, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                contentChannelService.Delete( contentChannel );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gContentChannels_GridRebind( object sender, EventArgs e )
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
            foreach ( var column in gContentChannels.Columns.OfType<AttributeField>().ToList() )
            {
                gContentChannels.Columns.Remove( column );
            }

            if ( _contentTypeId.HasValue )
            {
                // Add attribute columns
                int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.ContentChannel ) ).Id;
                string qualifier = _contentTypeId.Value.ToString();
                foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                    .Where( a =>
                        a.EntityTypeId == entityTypeId &&
                        a.IsGridColumn &&
                        a.EntityTypeQualifierColumn.Equals( "ContentTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifier ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name ) )
                {
                    string dataFieldExpression = attribute.Key;
                    bool columnExists = gContentChannels.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
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

                        gContentChannels.Columns.Add( boundField );
                    }
                }
            }
        }
        
        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _contentTypeId.HasValue )
            {
                ContentChannelService contentChannelService = new ContentChannelService( new RockContext() );
                SortProperty sortProperty = gContentChannels.SortProperty;
                var channels = contentChannelService.Queryable( "Items" )
                    .Where( c => c.ContentTypeId == _contentTypeId.Value )
                    .ToList();

                gContentChannels.ObjectList = new Dictionary<string, object>();
                channels.ForEach( c => gContentChannels.ObjectList.Add( c.Id.ToString(), c ) );

                var now = RockDateTime.Now;
                var items = channels.Select( c => new
                {
                    c.Id,
                    c.Name,
                    c.EnableRss,
                    c.ChannelUrl,
                    ItemLastCreated = c.Items.Max( i => i.CreatedDateTime ),
                    TotalItems = c.Items.Count(),
                    ActiveItems = c.Items
                        .Where( i =>
                            ( i.StartDateTime.CompareTo( now ) < 0 ) &&
                            ( !i.ExpireDateTime.HasValue || i.ExpireDateTime.Value.CompareTo( now ) > 0 ) &&
                            ( i.ApprovedByPersonAliasId.HasValue || !c.RequiresApproval )
                        ).Count()
                } ).AsQueryable();

                if ( sortProperty != null )
                {
                    gContentChannels.DataSource = items.Sort( sortProperty ).ToList();
                }
                else
                {
                    gContentChannels.DataSource = items.OrderBy( p => p.Name ).ToList();
                }

                gContentChannels.DataBind();
            }
        }

        #endregion
    }
}