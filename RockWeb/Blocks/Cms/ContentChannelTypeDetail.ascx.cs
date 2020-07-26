﻿// <copyright>
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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using System.ComponentModel;
using Rock.Security;
using Newtonsoft.Json;
using Rock.Web;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Channel Type Detail")]
    [Category("CMS")]
    [Description("Displays the details for a content channel type.")]
    public partial class ContentChannelTypeDetail : RockBlock, IDetailBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the channel attributes.
        /// </summary>
        /// <value>
        /// The state of the channel attributes.
        /// </value>
        private List<Attribute> ChannelAttributesState { get; set; }

        /// <summary>
        /// Gets or sets the state of the attributes.
        /// </summary>
        /// <value>
        /// The state of the attributes.
        /// </value>
        private List<Attribute> ItemAttributesState { get; set;}

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["ChannelAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ChannelAttributesState = new List<Attribute>();
            }
            else
            {
                ChannelAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

            json = ViewState["ItemAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ItemAttributesState = new List<Attribute>();
            }
            else
            {
                ItemAttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gChannelAttributes.DataKeyNames = new string[] { "Guid" };
            gChannelAttributes.Actions.ShowAdd = true;
            gChannelAttributes.Actions.AddClick += gChannelAttributes_Add;
            gChannelAttributes.GridRebind += gChannelAttributes_GridRebind;
            gChannelAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gChannelAttributes.GridReorder += gChannelAttributes_GridReorder;

            gItemAttributes.DataKeyNames = new string[] { "Guid" };
            gItemAttributes.Actions.ShowAdd = true;
            gItemAttributes.Actions.AddClick += gItemAttributes_Add;
            gItemAttributes.GridRebind += gItemAttributes_GridRebind;
            gItemAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gItemAttributes.GridReorder += gItemAttributes_GridReorder;

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "TypeId" ).AsInteger() );
            }
            else
            {
                ShowDialog();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["ChannelAttributesState"] = JsonConvert.SerializeObject( ChannelAttributesState, Formatting.None, jsonSetting );
            ViewState["ItemAttributesState"] = JsonConvert.SerializeObject( ItemAttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Gets the bread crumbs.
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? contentTypeId = PageParameter( pageReference, "TypeId" ).AsIntegerOrNull();
            if ( contentTypeId != null )
            {
                ContentChannelType contentType = new ContentChannelTypeService( new RockContext() ).Get( contentTypeId.Value );
                if ( contentType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( contentType.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Content Type", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }
        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannelType contentType;

            ContentChannelTypeService contentTypeService = new ContentChannelTypeService( rockContext );

            int contentTypeId = int.Parse( hfId.Value );

            if ( contentTypeId == 0 )
            {
                contentType = new ContentChannelType();
                contentTypeService.Add( contentType );
                contentType.CreatedByPersonAliasId = CurrentPersonAliasId;
                contentType.CreatedDateTime = RockDateTime.Now;
            }
            else
            {
                contentType = contentTypeService.Get( contentTypeId );
                contentType.ModifiedByPersonAliasId = CurrentPersonAliasId;
                contentType.ModifiedDateTime = RockDateTime.Now;
            }

            if ( contentType != null )
            {
                contentType.Name = tbName.Text;
                contentType.DateRangeType = ddlDateRangeType.SelectedValue.ConvertToEnum<ContentChannelDateType>();
                contentType.IncludeTime = cbIncludeTime.Checked;
                contentType.DisablePriority = cbDisablePriority.Checked;
                contentType.DisableContentField = cbDisableContentField.Checked;
                contentType.DisableStatus = cbDisableStatus.Checked;
                contentType.ShowInChannelList = cbShowInChannelList.Checked;

                if ( !Page.IsValid || !contentType.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {

                    rockContext.SaveChanges();

                    // get it back to make sure we have a good Id for it for the Attributes
                    contentType = contentTypeService.Get( contentType.Guid );

                    // Save the Channel Attributes
                    int entityTypeId = EntityTypeCache.Get( typeof( ContentChannel ) ).Id;
                    SaveAttributes( contentType.Id, entityTypeId, ChannelAttributesState, rockContext );

                    // Save the Item Attributes
                    entityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ) ).Id;
                    SaveAttributes( contentType.Id, entityTypeId, ItemAttributesState, rockContext );

                } );

                NavigateToParentPage();
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int contentTypeId = hfId.ValueAsInt();
            if ( contentTypeId != 0 )
            {
                ShowDetail( contentTypeId );
            }
        }

        #region Channel Attributes

        /// <summary>
        /// Handles the Add event of the gChannelAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChannelAttributes_Add( object sender, EventArgs e )
        {
            gChannelAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gChannelAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gChannelAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gChannelAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// gs the channel attributes show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gChannelAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtChannelAttributes.ActionTitle = ActionTitle.Add( tbName.Text + " Channel Attribute" );

            }
            else
            {
                attribute = ChannelAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtChannelAttributes.ActionTitle = ActionTitle.Edit( tbName.Text + " Channel Attribute" );
            }

            edtChannelAttributes.ReservedKeyNames = ChannelAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtChannelAttributes.SetAttributeProperties( attribute, typeof( ContentChannel ) );

            ShowDialog( "ChannelAttributes", true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gChannelAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gChannelAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var movedChannel = ChannelAttributesState.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedChannel != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherChannel in ChannelAttributesState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherChannel.Order = otherChannel.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherChannel in ChannelAttributesState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherChannel.Order = otherChannel.Order - 1;
                    }
                }

                movedChannel.Order = e.NewIndex;
            }

            BindChannelAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gChannelAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gChannelAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            ChannelAttributesState.RemoveEntity( attributeGuid );

            BindChannelAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gChannelAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChannelAttributes_GridRebind( object sender, EventArgs e )
        {
            BindChannelAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgChannelAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgChannelAttributes_SaveClick( object sender, EventArgs e )
        {
#pragma warning disable 0618 // Type or member is obsolete
            var attribute = SaveChangesToStateCollection( edtChannelAttributes, ChannelAttributesState );
#pragma warning restore 0618 // Type or member is obsolete

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            BindChannelAttributesGrid();

            HideDialog();
        }

        /// <summary>
        /// Binds the channel attributes grid.
        /// </summary>
        private void BindChannelAttributesGrid()
        {
            gChannelAttributes.DataSource = ChannelAttributesState
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            gChannelAttributes.DataBind();
        }

        #endregion

        #region Item Attributes

        /// <summary>
        /// Handles the Add event of the gItemAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gItemAttributes_Add( object sender, EventArgs e )
        {
            gItemAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gItemAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gItemAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gItemAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// gs the item attributes show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gItemAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtItemAttributes.ActionTitle = ActionTitle.Add( tbName.Text + " Item Attribute" );

            }
            else
            {
                attribute = ItemAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtItemAttributes.ActionTitle = ActionTitle.Edit( tbName.Text + " Item Attribute" );
            }

            edtItemAttributes.ReservedKeyNames = ItemAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtItemAttributes.SetAttributeProperties( attribute, typeof( ContentChannelItem ) );

            // always enable the display of the indexing option as the channel decides whether or not to index not the type
            edtItemAttributes.IsIndexingEnabledVisible = true;

            ShowDialog( "ItemAttributes", true );
        }

        /// <summary>
        /// Handles the GridReorder event of the gItemAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gItemAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var movedItem = ItemAttributesState.Where( a => a.Order == e.OldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( e.NewIndex < e.OldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in ItemAttributesState.Where( a => a.Order < e.OldIndex && a.Order >= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in ItemAttributesState.Where( a => a.Order > e.OldIndex && a.Order <= e.NewIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = e.NewIndex;
            }

            BindItemAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gItemAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gItemAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            ItemAttributesState.RemoveEntity( attributeGuid );

            BindItemAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gItemAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gItemAttributes_GridRebind( object sender, EventArgs e )
        {
            BindItemAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgItemAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgItemAttributes_SaveClick( object sender, EventArgs e )
        {
#pragma warning disable 0618 // Type or member is obsolete
            var attribute = SaveChangesToStateCollection( edtItemAttributes, ItemAttributesState );
#pragma warning restore 0618 // Type or member is obsolete

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            BindItemAttributesGrid();

            HideDialog();
        }

        /// <summary>
        /// Binds the item attributes grid.
        /// </summary>
        private void BindItemAttributesGrid()
        {
            gItemAttributes.DataSource = ItemAttributesState
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            gItemAttributes.DataBind();
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the type of the content.
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentChannelType GetContentChannelType( int contentTypeId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentType = new ContentChannelTypeService( rockContext )
                .Queryable()
                .Where( t => t.Id == contentTypeId )
                .FirstOrDefault();
            return contentType;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        public void ShowDetail( int contentTypeId )
        {
            var rockContext = new RockContext();
            ContentChannelType contentType = null;

            if ( !contentTypeId.Equals( 0 ) )
            {
                contentType = GetContentChannelType( contentTypeId );
                pdAuditDetails.SetEntity( contentType, ResolveRockUrl( "~" ) );
            }
            if ( contentType == null )
            {
                contentType = new ContentChannelType { Id = 0 };
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            string title = contentType.Id > 0 ?
                ActionTitle.Edit( ContentChannelType.FriendlyTypeName ) :
                ActionTitle.Add( ContentChannelType.FriendlyTypeName );
            lTitle.Text = title.FormatAsHtmlTitle();

            hfId.Value = contentType.Id.ToString();

            tbName.Text = contentType.Name;
            ddlDateRangeType.BindToEnum<ContentChannelDateType>();
            ddlDateRangeType.SetValue( (int)contentType.DateRangeType );
            ddlDateRangeType_SelectedIndexChanged( null, null );

            cbIncludeTime.Checked = contentType.IncludeTime;
            cbDisablePriority.Checked = contentType.DisablePriority;
            cbDisableContentField.Checked = contentType.DisableContentField;
            cbDisableStatus.Checked = contentType.DisableStatus;
            cbShowInChannelList.Checked = contentType.ShowInChannelList;
            // load attribute data 
            ChannelAttributesState = new List<Attribute>();
            ItemAttributesState = new List<Attribute>();

            AttributeService attributeService = new AttributeService( new RockContext() );

            string qualifierValue = contentType.Id.ToString();

            attributeService.GetByEntityTypeId( new ContentChannel().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .ToList()
                .ForEach( a => ChannelAttributesState.Add( a ) );
            
            // Set order 
            int newOrder = 0;
            ChannelAttributesState.ForEach( a => a.Order = newOrder++ );
                
            BindChannelAttributesGrid();

            attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                .ToList()
                .ForEach( a => ItemAttributesState.Add( a ) );
                
            // Set order 
            newOrder = 0;
            ItemAttributesState.ForEach( a => a.Order = newOrder++ );
            
            BindItemAttributesGrid();
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int contentTypeId, int entityTypeId, List<Attribute> attributes, RockContext rockContext )
        {
            string qualifierColumn = "ContentChannelTypeId";
            string qualifierValue = contentTypeId.ToString();

            AttributeService attributeService = new AttributeService( rockContext );

            // Get the existing attributes for this entity type and qualifier value
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            // Update the Attributes that were assigned in the UI
            foreach ( var attr in attributes )
            {
                Rock.Attribute.Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ITEMATTRIBUTES":
                    {
                        dlgItemAttributes.Show();
                        break;
                    }
                case "CHANNELATTRIBUTES":
                    {
                        dlgChannelAttributes.Show();
                        break;
                    }
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ITEMATTRIBUTES":
                    {
                        dlgItemAttributes.Hide();
                        break;
                    }
                case "CHANNELATTRIBUTES":
                    {
                        dlgChannelAttributes.Hide();
                        break;
                    }
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDateRangeType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDateRangeType_SelectedIndexChanged( object sender, EventArgs e )
        {
            cbIncludeTime.Visible = ddlDateRangeType.SelectedValueAsEnum<ContentChannelDateType>() != ContentChannelDateType.NoDates;
        }

        #region Obsolete Code

        /// <summary>
        /// Add or update the saved state of an Attribute using values from the AttributeEditor.
        /// Non-editable system properties of the existing Attribute state are preserved.
        /// </summary>
        /// <param name="editor">The AttributeEditor that holds the updated Attribute values.</param>
        /// <param name="attributeStateCollection">The stored state collection.</param>
        [RockObsolete( "1.11" )]
        [Obsolete( "This method is required for backward-compatibility - new blocks should use the AttributeEditor.SaveChangesToStateCollection() extension method instead." )]
        private Rock.Model.Attribute SaveChangesToStateCollection( AttributeEditor editor, List<Rock.Model.Attribute> attributeStateCollection )
        {
            // Load the editor values into a new Attribute instance.
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();

            editor.GetAttributeProperties( attribute );

            // Get the stored state of the Attribute, and copy the values of the non-editable properties.
            var attributeState = attributeStateCollection.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault();

            if ( attributeState != null )
            {
                attribute.Order = attributeState.Order;
                attribute.CreatedDateTime = attributeState.CreatedDateTime;
                attribute.CreatedByPersonAliasId = attributeState.CreatedByPersonAliasId;
                attribute.ForeignGuid = attributeState.ForeignGuid;
                attribute.ForeignId = attributeState.ForeignId;
                attribute.ForeignKey = attributeState.ForeignKey;

                attributeStateCollection.RemoveEntity( attribute.Guid );
            }
            else
            {
                // Set the Order of the new entry as the last item in the collection.
                attribute.Order = attributeStateCollection.Any() ? attributeStateCollection.Max( a => a.Order ) + 1 : 0;
            }

            attributeStateCollection.Add( attribute );

            return attribute;
        }

        #endregion
    }
}
