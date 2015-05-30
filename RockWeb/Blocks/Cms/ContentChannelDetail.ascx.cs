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
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Content Channel Detail")]
    [Category("CMS")]
    [Description("Displays the details for a content channel.")]
    public partial class ContentChannelDetail : RockBlock, IDetailBlock
    {

        #region Properties

        /// <summary>
        /// Gets or sets the state of the item attributes.
        /// </summary>
        /// <value>
        /// The state of the item attributes.
        /// </value>
        private List<Attribute> ItemAttributesState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["ItemAttributesState"] as string;
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

            gItemAttributes.DataKeyNames = new string[] { "Guid" };
            gItemAttributes.Actions.ShowAdd = true;
            gItemAttributes.Actions.AddClick += gItemAttributes_Add;
            gItemAttributes.GridRebind += gItemAttributes_GridRebind;
            gItemAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gItemAttributes.GridReorder += gItemAttributes_GridReorder;
            
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string script = @"
    $('.js-content-channel-enable-rss').change( function() {
        $(this).closest('div.form-group').siblings('div.js-content-channel-rss').slideToggle()
    });
";
            ScriptManager.RegisterStartupScript( cbEnableRss, cbEnableRss.GetType(), "enable-rss", script, true );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            maContentChannelWarning.Hide();

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "contentChannelId" ).AsInteger() );
            }
            else
            {
                if ( pnlEditDetails.Visible )
                {
                    var channel = new ContentChannel();
                    channel.Id = hfId.Value.AsInteger();
                    channel.ContentChannelTypeId = hfTypeId.Value.AsInteger();
                    channel.LoadAttributes();
                    phAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( channel, phAttributes, false, BlockValidationGroup );

                    ShowDialog();
                }
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

            int? contentChannelId = PageParameter( pageReference, "contentChannelId" ).AsIntegerOrNull();
            if ( contentChannelId != null )
            {
                ContentChannel contentChannel = new ContentChannelService( new RockContext() ).Get( contentChannelId.Value );
                if ( contentChannel != null )
                {
                    breadCrumbs.Add( new BreadCrumb( contentChannel.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Content Channel", pageReference ) );
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
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetContentChannel( hfId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlChannelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlChannelType_SelectedIndexChanged( object sender, EventArgs e )
        {
            ContentChannel channel = null;

            int contentChannelId = hfId.ValueAsInt();
            if ( contentChannelId != 0 )
            {
                channel = GetContentChannel( hfId.ValueAsInt() );
                if (channel != null && 
                    channel.ContentChannelTypeId.ToString() != ddlChannelType.SelectedValue && 
                    channel.Items.Any() )
                {
                    maContentChannelWarning.Show( "Changing the content type will result in all of this channel\\'s items losing any data that is specific to the original content type!", ModalAlertType.Warning );
                }
            }

            if (channel == null)
            {
                channel = new ContentChannel();
            }

            AddAttributeControls( channel );

        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlContentControlType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlContentControlType_SelectedIndexChanged( object sender, EventArgs e )
        {
            tbRootImageDirectory.Visible = ddlContentControlType.SelectedValueAsEnumOrNull<ContentControlType>() == ContentControlType.HtmlEditor;
        }
        
        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            ContentChannel contentChannel;

            ContentChannelService contentChannelService = new ContentChannelService( rockContext );

            int contentChannelId = hfId.Value.AsInteger();
             
            if ( contentChannelId == 0 )
            {
                contentChannel = new ContentChannel { Id = 0 };
                contentChannelService.Add( contentChannel );
            }
            else
            {
                contentChannel = contentChannelService.Get( contentChannelId );
            }

            if ( contentChannel != null )
            {
                contentChannel.Name = tbName.Text;
                contentChannel.Description = tbDescription.Text;
                contentChannel.ContentChannelTypeId = ddlChannelType.SelectedValueAsInt() ?? 0;
                contentChannel.ContentControlType = ddlContentControlType.SelectedValueAsEnum<ContentControlType>();
                contentChannel.RootImageDirectory = tbRootImageDirectory.Visible ? tbRootImageDirectory.Text : string.Empty;
                contentChannel.IconCssClass = tbIconCssClass.Text;
                contentChannel.RequiresApproval = cbRequireApproval.Checked;
                contentChannel.EnableRss = cbEnableRss.Checked;
                contentChannel.ChannelUrl = tbChannelUrl.Text;
                contentChannel.ItemUrl = tbItemUrl.Text;
                contentChannel.TimeToLive = nbTimetoLive.Text.AsIntegerOrNull();

                contentChannel.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, contentChannel );

                if ( !Page.IsValid || !contentChannel.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    contentChannel.SaveAttributeValues( rockContext );

                    foreach( var item in new ContentChannelItemService( rockContext )
                        .Queryable()
                        .Where( i => 
                            i.ContentChannelId == contentChannel.Id &&
                            i.ContentChannelTypeId != contentChannel.ContentChannelTypeId 
                        ))
                    {
                        item.ContentChannelTypeId = contentChannel.ContentChannelTypeId;
                    }

                    rockContext.SaveChanges();

                    // Save the Item Attributes
                    int entityTypeId = EntityTypeCache.Read( typeof( ContentChannelItem ) ).Id;
                    SaveAttributes( contentChannel.Id, entityTypeId, ItemAttributesState, rockContext );

                } );

                var pageReference = RockPage.PageReference;
                pageReference.Parameters.AddOrReplace( "contentChannelId", contentChannel.Id.ToString() );
                Response.Redirect( pageReference.BuildUrl(), false );
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            int contentChannelId = hfId.ValueAsInt();
            if ( contentChannelId != 0 )
            {
                ShowReadonlyDetails( GetContentChannel( contentChannelId ) );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            int contentChannelId = hfId.ValueAsInt();
            if ( contentChannelId != 0 )
            {
                ShowReadonlyDetails( GetContentChannel( contentChannelId ) );
            }
        }

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
        /// Gs the marketing campaign ad attribute type_ show edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void gItemAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtItemAttributes.ActionTitle = ActionTitle.Add( tbName.Text + " Item Attribute" );

            }
            else
            {
                attribute = ItemAttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtItemAttributes.ActionTitle = ActionTitle.Edit( tbName.Text + " Item Attribute" );
            }

            edtItemAttributes.ReservedKeyNames = ItemAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtItemAttributes.SetAttributeProperties( attribute, typeof( ContentChannelItem ) );

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
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtItemAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( ItemAttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = ItemAttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                ItemAttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = ItemAttributesState.Any() ? ItemAttributesState.Max( a => a.Order ) + 1 : 0;
            }
            ItemAttributesState.Add( attribute );

            BindItemAttributesGrid();

            HideDialog();
        }

        /// <summary>
        /// Binds the marketing campaign ad attribute type grid.
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
        /// <param name="contentChannelId">The content type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private ContentChannel GetContentChannel( int contentChannelId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            var contentChannel = new ContentChannelService( rockContext )
                .Queryable( "ContentChannelType" )
                .Where( t => t.Id == contentChannelId )
                .FirstOrDefault();
            return contentChannel;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="contentChannelId">The marketing campaign ad type identifier.</param>
        public void ShowDetail( int contentChannelId )
        {
            ContentChannel contentChannel = null;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            var rockContext = new RockContext();

            if ( !contentChannelId.Equals( 0 ) )
            {
                contentChannel = GetContentChannel( contentChannelId );
                if ( contentChannel != null )
                {
                    editAllowed = editAllowed || contentChannel.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            if ( contentChannel == null )
            {
                contentChannel = new ContentChannel { Id = 0 };
            }

            if ( contentChannel != null && contentChannel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                hfId.Value = contentChannel.Id.ToString();

                bool readOnly = false;
                nbEditModeMessage.Text = string.Empty;

                if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ContentChannel.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    lbEdit.Visible = false;
                    ShowReadonlyDetails( contentChannel );
                }
                else
                {
                    lbEdit.Visible = true;
                    if ( contentChannel.Id > 0 )
                    {
                        ShowReadonlyDetails( contentChannel );
                    }
                    else
                    {
                        ShowEditDetails( contentChannel );
                    }
                }

                lbSave.Visible = !readOnly;
            }
            else
            {
                nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToView( ContentChannel.FriendlyTypeName );
                pnlEditDetails.Visible = false;
                fieldsetViewSummary.Visible = false;
            }

        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        private void ShowReadonlyDetails( ContentChannel contentChannel )
        {
            SetEditMode( false );

            if ( contentChannel != null )
            {
                hfId.SetValue( contentChannel.Id );

                SetHeadingInfo( contentChannel, contentChannel.Name );
                SetEditMode( false );

                lGroupDescription.Text = contentChannel.Description;

                var descriptionList = new DescriptionList();
                if ( contentChannel.EnableRss )
                {
                    descriptionList
                    .Add( "Item's Require Approval", contentChannel.RequiresApproval.ToString() )
                    .Add( "Channel Url", contentChannel.ChannelUrl )
                    .Add( "Item Url", contentChannel.ItemUrl );
                }

                contentChannel.LoadAttributes();
                foreach ( var attribute in contentChannel.Attributes
                    .Where( a => a.Value.IsGridColumn )
                    .OrderBy( a => a.Value.Order )
                    .Select( a => a.Value ) )
                {
                    if ( contentChannel.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        string value = attribute.FieldType.Field.FormatValueAsHtml( null,
                            contentChannel.AttributeValues[attribute.Key].Value, attribute.QualifierValues, false );
                        descriptionList.Add( attribute.Name, value );
                    }
                }

                lDetails.Text = descriptionList.Html;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        protected void ShowEditDetails( ContentChannel contentChannel )
        {
            if ( contentChannel != null )
            {
                hfId.Value = contentChannel.Id.ToString();
                string title = contentChannel.Id > 0 ?
                    ActionTitle.Edit( ContentChannel.FriendlyTypeName ) :
                    ActionTitle.Add( ContentChannel.FriendlyTypeName );

                SetHeadingInfo( contentChannel, title );

                SetEditMode( true );

                LoadDropdowns();

                tbName.Text = contentChannel.Name;
                tbDescription.Text = contentChannel.Description;
                ddlChannelType.SetValue( contentChannel.ContentChannelTypeId );
                ddlContentControlType.SetValue( contentChannel.ContentControlType.ConvertToInt().ToString() );
                tbRootImageDirectory.Text = contentChannel.RootImageDirectory;
                tbRootImageDirectory.Visible = contentChannel.ContentControlType == ContentControlType.HtmlEditor;
                tbIconCssClass.Text = contentChannel.IconCssClass;
                cbRequireApproval.Checked = contentChannel.RequiresApproval;
                cbEnableRss.Checked = contentChannel.EnableRss;

                divRss.Attributes["style"] = cbEnableRss.Checked ? "display:block" : "display:none";
                tbChannelUrl.Text = contentChannel.ChannelUrl;
                tbItemUrl.Text = contentChannel.ItemUrl;
                nbTimetoLive.Text = ( contentChannel.TimeToLive ?? 0 ).ToString();

                AddAttributeControls( contentChannel );

                // load attribute data 
                ItemAttributesState = new List<Attribute>();
                AttributeService attributeService = new AttributeService( new RockContext() );

                string qualifierValue = contentChannel.Id.ToString();

                attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( qualifierValue ) )
                    .ToList()
                    .ForEach( a => ItemAttributesState.Add( a ) );

                // Set order 
                int newOrder = 0;
                ItemAttributesState.ForEach( a => a.Order = newOrder++ );

                BindItemAttributesGrid();
            }
        }

        /// <summary>
        /// Adds the attribute controls.
        /// </summary>
        /// <param name="contentChannel">The content channel.</param>
        private void AddAttributeControls( ContentChannel contentChannel )
        {
            int typeId = ddlChannelType.SelectedValueAsInt() ?? 0;
            hfTypeId.Value = typeId.ToString();

            contentChannel.ContentChannelTypeId = typeId;
            contentChannel.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( contentChannel, phAttributes, true );
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        /// <param name="title">The title.</param>
        private void SetHeadingInfo( ContentChannel contentChannel, string title )
        {
            string cssIcon = contentChannel.IconCssClass;
            if (string.IsNullOrWhiteSpace(cssIcon))
            {
                cssIcon = "fa fa-bullhorn";
            }
            lIcon.Text = string.Format("<i class='{0}'></i>", cssIcon);
            lTitle.Text = title.FormatAsHtmlTitle();
            hlContentChannel.Text = contentChannel.ContentChannelType != null ? contentChannel.ContentChannelType.Name : string.Empty;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Loads the dropdowns.
        /// </summary>
        private void LoadDropdowns()
        {
            ddlChannelType.Items.Clear();
            foreach ( var contentType in new ContentChannelTypeService( new RockContext() ).Queryable().OrderBy( c => c.Name ) )
            {
                ddlChannelType.Items.Add( new ListItem( contentType.Name, contentType.Id.ToString() ) );
            }

            ddlContentControlType.BindToEnum<ContentControlType>();

        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int channelId, int entityTypeId, List<Attribute> attributes, RockContext rockContext )
        {
            string qualifierColumn = "ContentChannelId";
            string qualifierValue = channelId.ToString();

            AttributeService attributeService = new AttributeService( rockContext );

            // Get the existing attributes for this entity type and qualifier value
            var existingAttributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                Rock.Web.Cache.AttributeCache.Flush( attr.Id );
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            int newOrder = 1000;

            // Update the Attributes that were assigned in the UI
            foreach ( var attr in attributes.OrderBy( a => a.Order ) )
            {
                // Artificially exaggerate the order so that all channel specific attributes are displayed after the content-type specific attributes (unless categorized)
                attr.Order = newOrder++;
                Rock.Attribute.Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }

            AttributeCache.FlushEntityAttributes();
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
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
}
}