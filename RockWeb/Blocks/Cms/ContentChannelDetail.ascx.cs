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
using Rock.UniversalSearch;
using System.Text;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Content Channel Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details for a content channel." )]
    public partial class ContentChannelDetail : RockBlock, IDetailBlock
    {

        #region Properties

        private List<int> ChildContentChannelsList { get; set; }

        private List<string> ItemInheritedKey { get; set; }

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

            ChildContentChannelsList = ViewState["ChildContentChannelList"] as List<int> ?? new List<int>();

            ItemInheritedKey = ViewState["ItemInheritedKey"] as List<string> ?? new List<string>();
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

            gChildContentChannels.DataKeyNames = new string[] { "Id" };
            gChildContentChannels.Actions.ShowAdd = true;
            gChildContentChannels.Actions.AddClick += gChildContentChannels_Add;
            gChildContentChannels.GridRebind += gChildContentChannels_GridRebind;
            gChildContentChannels.EmptyDataText = Server.HtmlEncode( None.Text );

            gItemAttributes.DataKeyNames = new string[] { "Guid" };
            gItemAttributes.Actions.ShowAdd = true;
            gItemAttributes.Actions.AddClick += gItemAttributes_Add;
            gItemAttributes.GridRebind += gItemAttributes_GridRebind;
            gItemAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gItemAttributes.GridReorder += gItemAttributes_GridReorder;

            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ContentChannel ) ).Id;

            dvEditorTool.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS.AsGuid() ).Id;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            string script = @"
    $('.js-content-channel-enable-rss').change( function() {
        $(this).closest('div.form-group').siblings('div.js-content-channel-rss').slideToggle()
    });

    $('.js-content-channel-enable-tags').change( function() {
        $(this).closest('div.form-group').siblings('div.js-content-channel-tags').slideToggle()
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
                int? contentChannelId = PageParameter( "ContentChannelId" ).AsIntegerOrNull();
                if ( contentChannelId.HasValue )
                {
                    upnlContent.Visible = true;
                    ShowDetail( contentChannelId.Value );
                }
                else
                {
                    upnlContent.Visible = false;
                }
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
            ViewState["ChildContentChannelList"] = ChildContentChannelsList;

            ViewState["ItemInheritedKey"] = ItemInheritedKey;

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

            int? contentChannelId = PageParameter( pageReference, "ContentChannelId" ).AsIntegerOrNull();
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
                if ( channel != null &&
                    channel.ContentChannelTypeId.ToString() != ddlChannelType.SelectedValue &&
                    channel.Items.Any() )
                {
                    maContentChannelWarning.Show( "Changing the content type will result in all of this channel's items losing any data that is specific to the original content type!", ModalAlertType.Warning );
                }
            }

            if ( channel == null )
            {
                channel = new ContentChannel();
            }

            UpdateControlsForContentChannelType( channel );
        }

        /// <summary>
        /// Updates the type of the controls for content channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        private void UpdateControlsForContentChannelType( ContentChannel channel )
        {
            SetInheritedAttributeKeys( channel.Id );

            AddAttributeControls( channel );

            int contentChannelTypeId = ddlChannelType.SelectedValueAsInt() ?? 0;
            var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( contentChannelTypeId );
            if ( contentChannelType != null )
            {
                cbIsStructuredContent.Visible = !contentChannelType.DisableContentField;
                ddlContentControlType.Visible = !contentChannelType.DisableContentField && !channel.IsStructuredContent;
                dvEditorTool.Visible = !contentChannelType.DisableContentField && channel.IsStructuredContent;
                cbRequireApproval.Visible = !contentChannelType.DisableStatus;
            }
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
            CategoryService categoryService = new CategoryService( rockContext );

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
                contentChannel.IsStructuredContent = cbIsStructuredContent.Checked;
                contentChannel.StructuredContentToolValueId = dvEditorTool.SelectedDefinedValueId;
                contentChannel.ContentControlType = ddlContentControlType.SelectedValueAsEnum<ContentControlType>();
                contentChannel.RootImageDirectory = tbRootImageDirectory.Visible ? tbRootImageDirectory.Text : string.Empty;
                contentChannel.IconCssClass = tbIconCssClass.Text;

                // the cbRequireApproval will be hidden if contentChannelType.DisableStatus == True
                contentChannel.RequiresApproval = cbRequireApproval.Visible && cbRequireApproval.Checked;
                contentChannel.IsIndexEnabled = cbIndexChannel.Checked;
                contentChannel.ItemsManuallyOrdered = cbItemsManuallyOrdered.Checked;
                contentChannel.ChildItemsManuallyOrdered = cbChildItemsManuallyOrdered.Checked;
                contentChannel.EnableRss = cbEnableRss.Checked;
                contentChannel.ChannelUrl = tbChannelUrl.Text;
                contentChannel.ItemUrl = tbItemUrl.Text;
                contentChannel.TimeToLive = nbTimetoLive.Text.AsIntegerOrNull();
                contentChannel.ItemUrl = tbContentChannelItemPublishingPoint.Text;
                contentChannel.IsTaggingEnabled = cbEnableTag.Checked;
                contentChannel.ItemTagCategoryId = cbEnableTag.Checked ? cpCategory.SelectedValueAsInt() : ( int? ) null;

                // Add any categories
                contentChannel.Categories.Clear();
                foreach ( var categoryId in cpCategories.SelectedValuesAsInt() )
                {
                    contentChannel.Categories.Add( categoryService.Get( categoryId ) );
                }

                // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed
                contentChannel.ModifiedDateTime = RockDateTime.Now;

                contentChannel.ChildContentChannels = new List<ContentChannel>();
                contentChannel.ChildContentChannels.Clear();
                foreach ( var item in ChildContentChannelsList )
                {
                    var childContentChannel = contentChannelService.Get( item );
                    if ( childContentChannel != null )
                    {
                        contentChannel.ChildContentChannels.Add( childContentChannel );
                    }
                }

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

                    foreach ( var item in new ContentChannelItemService( rockContext )
                        .Queryable()
                        .Where( i =>
                            i.ContentChannelId == contentChannel.Id &&
                            i.ContentChannelTypeId != contentChannel.ContentChannelTypeId
                        ) )
                    {
                        item.ContentChannelTypeId = contentChannel.ContentChannelTypeId;
                    }

                    rockContext.SaveChanges();

                    // Save the Item Attributes
                    int entityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ) ).Id;
                    SaveAttributes( contentChannel.Id, entityTypeId, ItemAttributesState, rockContext );

                } );

                var pageReference = RockPage.PageReference;
                pageReference.Parameters.AddOrReplace( "ContentChannelId", contentChannel.Id.ToString() );
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

        /// <summary>
        /// Handles the CheckedChanged event of the cbIsStructuredContent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbIsStructuredContent_CheckedChanged( object sender, EventArgs e )
        {
            ddlContentControlType.Visible = !cbIsStructuredContent.Checked;
            dvEditorTool.Visible = cbIsStructuredContent.Checked;
            if ( !cbIsStructuredContent.Checked )
            {
                dvEditorTool.SelectedDefinedValueId = null;
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
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
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


            List<string> reservedKeys = ItemAttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();
            reservedKeys.AddRange( ItemInheritedKey );
            edtItemAttributes.ReservedKeyNames = reservedKeys;

            edtItemAttributes.SetAttributeProperties( attribute, typeof( ContentChannelItem ) );

            edtItemAttributes.IsIndexingEnabledVisible = cbIndexChannel.Visible && cbIndexChannel.Checked;

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
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
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
        /// <param name="contentChannelId">The content channel identifier.</param>
        public void ShowDetail( int contentChannelId )
        {
            ContentChannel contentChannel = null;

            cbIndexChannel.Visible = IndexContainer.IndexingEnabled;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            var rockContext = new RockContext();

            if ( !contentChannelId.Equals( 0 ) )
            {
                contentChannel = GetContentChannel( contentChannelId );
                if ( contentChannel != null )
                {
                    editAllowed = editAllowed || contentChannel.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
                pdAuditDetails.SetEntity( contentChannel, ResolveRockUrl( "~" ) );
            }

            if ( contentChannel == null )
            {
                contentChannel = new ContentChannel { Id = 0 };
                contentChannel.ChildContentChannels = new List<ContentChannel>();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            if ( contentChannel != null && contentChannel.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                hfId.Value = contentChannel.Id.ToString();

                bool readOnly = false;
                nbEditModeMessage.Text = string.Empty;

                if ( !editAllowed )
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

                btnSecurity.Visible = contentChannel.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                btnSecurity.Title = contentChannel.Name;
                btnSecurity.EntityId = contentChannel.Id;

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

                nbRoleMessage.Visible = false;
                if ( contentChannel.RequiresApproval && !IsApproverConfigured( contentChannel ) )
                {
                    nbRoleMessage.Text = "<p>No role or person is configured to approve the items for this channel. Please configure one or more roles or people in the security settings under the &quot;Approve&quot; tab.</p>";
                    nbRoleMessage.Visible = true;
                }

                lGroupDescription.Text = contentChannel.Description;

                var descriptionListLeft = new DescriptionList();
                var descriptionListRight = new DescriptionList();

                descriptionListLeft.Add( "Items Require Approval", contentChannel.RequiresApproval.ToYesNo() );
                descriptionListRight.Add( "Is Indexed", contentChannel.IsIndexEnabled.ToYesNo() );

                if ( contentChannel.EnableRss )
                {
                    descriptionListLeft.Add( "Channel URL", contentChannel.ChannelUrl );
                    descriptionListRight.Add( "Item URL", contentChannel.ItemUrl );
                }

                contentChannel.LoadAttributes();
                foreach ( var attribute in contentChannel.Attributes
                    .Where( a => a.Value.IsGridColumn )
                    .OrderBy( a => a.Value.Order )
                    .Select( a => a.Value ) )
                {
                    if ( contentChannel.AttributeValues.ContainsKey( attribute.Key ) )
                    {
                        string value = attribute.FieldType.Field.FormatValueAsHtml( null, attribute.EntityTypeId, contentChannel.Id,
                            contentChannel.AttributeValues[attribute.Key].Value, attribute.QualifierValues, false );
                        descriptionListLeft.Add( attribute.Name, value );
                    }
                }

                lDetailsLeft.Text = descriptionListLeft.Html;
                lDetailsRight.Text = descriptionListRight.Html;
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
                var categoryIds = contentChannel.Categories.Select( c => c.Id ).ToList();
                cpCategories.SetValues( categoryIds );
                cbIsStructuredContent.Checked = contentChannel.IsStructuredContent;
                if ( contentChannel.IsStructuredContent )
                {
                    dvEditorTool.SelectedDefinedValueId = contentChannel.StructuredContentToolValueId;
                }
                ddlContentControlType.SetValue( contentChannel.ContentControlType.ConvertToInt().ToString() );
                tbRootImageDirectory.Text = contentChannel.RootImageDirectory;
                tbRootImageDirectory.Visible = contentChannel.ContentControlType == ContentControlType.HtmlEditor;
                tbIconCssClass.Text = contentChannel.IconCssClass;
                cbRequireApproval.Checked = contentChannel.RequiresApproval;
                cbIndexChannel.Checked = contentChannel.IsIndexEnabled;
                cbItemsManuallyOrdered.Checked = contentChannel.ItemsManuallyOrdered;
                cbChildItemsManuallyOrdered.Checked = contentChannel.ChildItemsManuallyOrdered;
                cbEnableRss.Checked = contentChannel.EnableRss;
                tbContentChannelItemPublishingPoint.Text = contentChannel.ItemUrl;
                cbEnableTag.Checked = contentChannel.IsTaggingEnabled;
                cpCategory.SetValue( contentChannel.ItemTagCategoryId );

                divRss.Attributes["style"] = cbEnableRss.Checked ? "display:block" : "display:none";
                divTag.Attributes["style"] = cbEnableTag.Checked ? "display:block" : "display:none";

                tbChannelUrl.Text = contentChannel.ChannelUrl;
                tbItemUrl.Text = contentChannel.ItemUrl;
                nbTimetoLive.Text = ( contentChannel.TimeToLive ?? 0 ).ToString();

                ChildContentChannelsList = new List<int>();
                contentChannel.ChildContentChannels.ToList().ForEach( a => ChildContentChannelsList.Add( a.Id ) );
                BindChildContentChannelsGrid();

                UpdateControlsForContentChannelType( contentChannel );

                // load attribute data
                ItemAttributesState = new List<Attribute>();
                AttributeService attributeService = new AttributeService( new RockContext() );

                string qualifierValue = contentChannel.Id.ToString();

                attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, true ).AsQueryable()
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
        /// Sets the inherited attribute keys.
        /// </summary>
        /// <param name="contentChannelTypeId">The content channel type identifier.</param>
        private void SetInheritedAttributeKeys( int? contentChannelTypeId )
        {
            ItemInheritedKey = new List<string>();
            if ( contentChannelTypeId.HasValue && contentChannelTypeId.Value > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    int entityTypeID = new ContentChannelItem().TypeId;
                    string qualifierValue = contentChannelTypeId.Value.ToString();

                    ItemInheritedKey = new AttributeService( rockContext )
                        .GetByEntityTypeQualifier( entityTypeID, "ContentChannelTypeId", qualifierValue, true )
                        .Select( a => a.Key )
                        .ToList();
                }
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
            Rock.Attribute.Helper.AddEditControls( contentChannel, phAttributes, true, BlockValidationGroup );
        }

        /// <summary>
        /// Sets the heading information.
        /// </summary>
        /// <param name="contentChannel">Type of the content.</param>
        /// <param name="title">The title.</param>
        private void SetHeadingInfo( ContentChannel contentChannel, string title )
        {
            string cssIcon = contentChannel.IconCssClass;
            if ( string.IsNullOrWhiteSpace( cssIcon ) )
            {
                cssIcon = "fa fa-bullhorn";
            }
            lIcon.Text = string.Format( "<i class='{0}'></i>", cssIcon );
            lTitle.Text = title.FormatAsHtmlTitle();
            var categoriesHtml = new StringBuilder();
            foreach ( var category in contentChannel.Categories.OrderBy( a => a.Order ) )
            {
                categoriesHtml.AppendLine( string.Format( "<span class='label label-info' data-toggle='tooltip' title='{0}'>{0}</span>", category.Name ) );
            }
            lCategories.Text = categoriesHtml.ToString();
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
            var visibleContentChannelTypeList = new ContentChannelTypeService( new RockContext() ).Queryable()
                .Where( a => a.ShowInChannelList )
                .OrderBy( c => c.Name ).Select( a => new
                {
                    a.Id,
                    a.Name
                } )
                .ToList();

            foreach ( var contentType in visibleContentChannelTypeList )
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
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
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
                case "CHILDCONTENTCHANNELS":
                    {
                        dlgChildContentChannel.Show();
                        break;
                    }
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
                case "CHILDCONTENTCHANNELS":
                    {
                        dlgChildContentChannel.Hide();
                        break;
                    }
                case "ITEMATTRIBUTES":
                    {
                        dlgItemAttributes.Hide();
                        break;
                    }
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Check if there is any approver configured.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        public bool IsApproverConfigured( ContentChannel contentChannel )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );

            var authService = new AuthService( rockContext );
            var contentChannelEntityTypeId = EntityTypeCache.Get<Rock.Model.ContentChannel>().Id;

            var approvalAuths = authService.GetAuths( contentChannelEntityTypeId, contentChannel.Id, Rock.Security.Authorization.APPROVE );

            // Get a list of all PersonIds that are allowed that are included in the Auths
            // Then, when we get a list of all the allowed people that are in the auth as a specific Person or part of a Role (Group), we'll run all those people thru NoteType.IsAuthorized
            // That way, we don't have to figure out all the logic of Allow/Deny based on Order, etc
            List<int> authPersonIdListAll = new List<int>();
            var approvalAuthsAllowed = approvalAuths.Where( a => a.AllowOrDeny == "A" ).ToList();


            bool isValid = false;
            foreach ( var approvalAuth in approvalAuthsAllowed )
            {
                int? personId = null;

                if ( approvalAuth.PersonAlias != null )
                {
                    personId = approvalAuth.PersonAlias.PersonId;
                }

                if ( personId.HasValue )
                {
                    isValid = true;
                    break;
                }
                else if ( approvalAuth.GroupId.HasValue )
                {
                    isValid = true;
                    break;
                }
                else if ( approvalAuth.SpecialRole != SpecialRole.None )
                {
                    // Not Supported: Get people that belong to Special Roles like AllUsers, AllAuthenticatedUser, and AllUnAuthenicatedUsers doesn't really make sense, so ignore it
                }
            }

            return isValid;
        }

        #endregion

        #region Child ContentChannel Grid and Picker

        /// <summary>
        /// Handles the Add event of the gChildContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChildContentChannels_Add( object sender, EventArgs e )
        {
            // populate dropdown with all grouptypes that aren't already childgroups
            var contentChannelList = new ContentChannelService( new RockContext() )
                .Queryable()
                .Where( t => !ChildContentChannelsList.Contains( t.Id ) )
                .OrderBy( t => t.Name )
                .ToList();

            if ( contentChannelList.Count == 0 )
            {
                modalAlert.Show( "There are not any other content channels that can be added", ModalAlertType.Warning );
            }
            else
            {
                ddlChildContentChannel.DataSource = contentChannelList;
                ddlChildContentChannel.DataBind();
                ShowDialog( "ChildContentChannels" );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gChildContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gChildContentChannels_Delete( object sender, RowEventArgs e )
        {
            int childContentChannelId = e.RowKeyId;
            ChildContentChannelsList.Remove( childContentChannelId );
            BindChildContentChannelsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gChildContentChannels control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gChildContentChannels_GridRebind( object sender, EventArgs e )
        {
            BindChildContentChannelsGrid();
        }

        /// <summary>
        /// Binds the child content channels grid.
        /// </summary>
        private void BindChildContentChannelsGrid()
        {
            var contentChannelList = new ContentChannelService( new RockContext() )
                .Queryable()
                .Where( t => ChildContentChannelsList.Contains( t.Id ) )
                .OrderBy( t => t.Name )
                .ToList();

            gChildContentChannels.DataSource = contentChannelList;
            gChildContentChannels.DataBind();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgChildContentChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgChildContentChannel_SaveClick( object sender, EventArgs e )
        {
            ChildContentChannelsList.Add( ddlChildContentChannel.SelectedValueAsId() ?? 0 );
            BindChildContentChannelsGrid();
            HideDialog();
        }

        #endregion

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
